using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompSci_NEA.WorldGeneration
{
    // Delegate for mapping temperature and secondary value (e.g. moisture, spookiness, intensity)
    // into a biome ID based on thresholds th1 and th3.
    public delegate byte BiomeMappingFunc(float temperature, float secondaryValue, float th1, float th3);

    public class BiomeGenerator
    {
        private int width, height;
        private int offsetX, offsetY; // Offset within the world array.
        private byte[,] world;
        private byte safeTile;
        private byte[,] _mask;
        private byte[,] _biomeMap;
        private Func<int, int, int, int, float> temperatureFunc;
        private Func<int, int, int, int, float> secondaryFunc;
        private BiomeMappingFunc mappingFunc;

        // Constructor now takes a Rectangle defining the sub-region (in tile coordinates) to process.
        // The rectangle's Width and Height become the region's dimensions and X/Y become the offset.
        // Also accepts custom percentages for cold and medium thresholds.
        public BiomeGenerator(
            byte[,] world,
            Rectangle area,
            byte safeTile,
            Func<int, int, int, int, float> temperatureFunc,
            Func<int, int, int, int, float> secondaryFunc,
            BiomeMappingFunc mappingFunc,
            float coldPercentage,      // Percentage of safe tiles considered "cold"
            float mediumPercentage)    // Percentage of safe tiles considered "medium"
        {
            this.offsetX = area.X;
            this.offsetY = area.Y;
            this.width = area.Width;
            this.height = area.Height;
            this.world = world;
            this.safeTile = safeTile;
            this.temperatureFunc = temperatureFunc;
            this.secondaryFunc = secondaryFunc;
            this.mappingFunc = mappingFunc;

            // Build a mask and biome map for only the sub-region.
            this._mask = ExtractMask(world, safeTile);
            this._biomeMap = new byte[width, height];

            // Biome generation parameters.
            float minTemperature = 0f;
            float maxTemperature = 1f;
            float noiseScale = 0.05f;
            float noiseAmplitude = 0.2f;

            // Use the provided percentages.
            float desiredColdPerc = coldPercentage;
            float desiredMediumPerc = mediumPercentage;

            // First pass: collect "safe" temperatures where the tile equals safeTile in the region.
            List<float> safeTemperatures = new List<float>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Get world coordinate.
                    int worldX = x + offsetX;
                    int worldY = y + offsetY;
                    if (_mask[x, y] == 1)
                    {
                        float baseTemp = temperatureFunc(x, y, width, height);
                        // Optionally, use global coordinates for noise:
                        float noiseValue = NoiseGenerator.Generate((x + offsetX) * noiseScale, (y + offsetY) * noiseScale, 3, 10f);
                        float adjustedNoise = (noiseValue - 0.5f) * noiseAmplitude;
                        float temperature = MathHelper.Clamp(baseTemp + adjustedNoise, minTemperature, maxTemperature);
                        safeTemperatures.Add(temperature);
                    }
                }
            }
            safeTemperatures.Sort();
            int safeCount = safeTemperatures.Count;
            float th1 = safeTemperatures[(int)(desiredColdPerc * safeCount)];
            float th3 = safeTemperatures[(int)((desiredColdPerc + desiredMediumPerc) * safeCount)];

            // Second pass: compute the biome ID for each safe tile using the mapping function.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int worldX = x + offsetX;
                    int worldY = y + offsetY;
                    if (_mask[x, y] == 1)
                    {
                        float baseTemp = temperatureFunc(x, y, width, height);
                        float noiseValue = NoiseGenerator.Generate((x + offsetX) * noiseScale, (y + offsetY) * noiseScale, 3, 10f);
                        float adjustedNoise = (noiseValue - 0.5f) * noiseAmplitude;
                        float temperature = MathHelper.Clamp(baseTemp + adjustedNoise, minTemperature, maxTemperature);

                        float baseSecondary = secondaryFunc(x, y, width, height);
                        float secondaryValue = MathHelper.Clamp(baseSecondary + adjustedNoise, minTemperature, maxTemperature);

                        // Determine the biome using the mapping function.
                        byte biome = mappingFunc(temperature, secondaryValue, th1, th3);
                        _biomeMap[x, y] = biome;
                    }
                    else
                    {
                        // Use the world tile from the sub-region.
                        _biomeMap[x, y] = world[x + offsetX, y + offsetY];
                    }
                }
            }
        }

        // Extracts a mask for the sub-region defined by offset and region size.
        private byte[,] ExtractMask(byte[,] world, byte maskValue)
        {
            byte[,] mask = new byte[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    mask[x, y] = (world[x + offsetX, y + offsetY] == maskValue) ? (byte)1 : (byte)0;
            return mask;
        }

        public byte[,] GetBiomeMap() => _biomeMap;

        public Texture2D GenerateTemperatureTexture(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight)
        {
            Texture2D texture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
            Color[] textureData = new Color[textureWidth * textureHeight];

            for (int ty = 0; ty < textureHeight; ty++)
            {
                for (int tx = 0; tx < textureWidth; tx++)
                {
                    float wx = (float)tx / (textureWidth - 1) * (width - 1);
                    float wy = (float)ty / (textureHeight - 1) * (height - 1);
                    int ix = (int)Math.Round(wx);
                    int iy = (int)Math.Round(wy);
                    ix = MathHelper.Clamp(ix, 0, width - 1);
                    iy = MathHelper.Clamp(iy, 0, height - 1);

                    float temp = temperatureFunc(ix, iy, width, height);
                    Color baseColor = Color.Lerp(Color.Blue, Color.Red, temp);

                    bool isSafe = (_mask[ix, iy] == 1);
                    bool isEdge = false;
                    if (isSafe)
                    {
                        for (int dx = -1; dx <= 1 && !isEdge; dx++)
                        {
                            for (int dy = -1; dy <= 1 && !isEdge; dy++)
                            {
                                if (dx == 0 && dy == 0)
                                    continue;
                                int nx = ix + dx;
                                int ny = iy + dy;
                                if (nx < 0 || nx >= width || ny < 0 || ny >= height || _mask[nx, ny] == 0)
                                    isEdge = true;
                            }
                        }
                    }
                    if (isEdge)
                        baseColor = Color.Lerp(baseColor, Color.White, 0.9f);

                    textureData[ty * textureWidth + tx] = baseColor;
                }
            }
            texture.SetData(textureData);
            return texture;
        }

        public Texture2D GenerateMoistureTexture(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight)
        {
            Texture2D texture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
            Color[] textureData = new Color[textureWidth * textureHeight];

            for (int ty = 0; ty < textureHeight; ty++)
            {
                for (int tx = 0; tx < textureWidth; tx++)
                {
                    float wx = (float)tx / (textureWidth - 1) * (width - 1);
                    float wy = (float)ty / (textureHeight - 1) * (height - 1);
                    int ix = (int)Math.Round(wx);
                    int iy = (int)Math.Round(wy);
                    ix = MathHelper.Clamp(ix, 0, width - 1);
                    iy = MathHelper.Clamp(iy, 0, height - 1);

                    float moisture = secondaryFunc(ix, iy, width, height);
                    Color baseColor = Color.Lerp(Color.SandyBrown, Color.Green, moisture);

                    bool isSafe = (_mask[ix, iy] == 1);
                    bool isEdge = false;
                    if (isSafe)
                    {
                        for (int dx = -1; dx <= 1 && !isEdge; dx++)
                        {
                            for (int dy = -1; dy <= 1 && !isEdge; dy++)
                            {
                                if (dx == 0 && dy == 0)
                                    continue;
                                int nx = ix + dx;
                                int ny = iy + dy;
                                if (nx < 0 || nx >= width || ny < 0 || ny >= height || _mask[nx, ny] == 0)
                                    isEdge = true;
                            }
                        }
                    }
                    if (isEdge)
                        baseColor = Color.Lerp(baseColor, Color.White, 0.9f);

                    textureData[ty * textureWidth + tx] = baseColor;
                }
            }
            texture.SetData(textureData);
            return texture;
        }
    }
}
