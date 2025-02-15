using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CompSci_NEA.WorldGeneration
{
    public class BiomeGenerator
    {
        private int width, height;
        private byte[,] world;
        private byte[,] mask;
        private byte[,] biomeMap;

        public BiomeGenerator(byte[,] world, int width, int height)
        {
            for (int a = 0; a < 10; a++)
            {
                for (int b = 0; b < 10; b++)
                {
                    float temp = GetBaseTemperature(b, a, 10, 10);
                    Console.Write($"{temp:0.00} ");
                }
                Console.WriteLine();
            }

            this.width = width;
            this.height = height;
            this.world = world;
            this.mask = ExtractMask(world, 3);
            this.biomeMap = new byte[width, height];

            float minTemperature = 0f;
            float maxTemperature = 1f;
            float noiseScale = 0.05f;
            float noiseAmplitude = 0.2f;

            float desiredTundraPerc = 0.10f;  // biome 9
            float desiredFieldsPerc = 0.60f;  // biome 7
            float desiredWastelandPerc = 0.30f;  // biome 10

            float th1 = 0.2f; 
            float th3 = 0.8f; 

            int safeCount = 0;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (mask[x, y] == 1)
                        safeCount++;

            int maxIterations = 10;
            const float delta = 0.02f;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (mask[x, y] == 1)
                        {
                            float baseTemp = GetBaseTemperature(x, y, width, height);
                            float noiseValue = NoiseGenerator.Generate(x * noiseScale, y * noiseScale, octaves: 3, frequency: 10f);
                            float adjustedNoise = (noiseValue - 0.5f) * noiseAmplitude;
                            float temperature = MathHelper.Clamp(baseTemp + adjustedNoise, minTemperature, maxTemperature);
                            byte biome = (temperature < th1) ? (byte)9 : (temperature < th3 ? (byte)7 : (byte)10);
                            biomeMap[x, y] = biome;
                        }
                        else
                        {
                            biomeMap[x, y] = world[x, y];
                        }
                    }
                }

                Dictionary<byte, int> counts = new Dictionary<byte, int>() {
                    {9, 0}, {7, 0}, {10, 0}
                };
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        if (mask[x, y] == 1)
                        {
                            byte b = biomeMap[x, y];
                            if (counts.ContainsKey(b))
                                counts[b]++;
                        }
                float percTundra = counts[9] / (float)safeCount;
                float percFields = counts[7] / (float)safeCount;
                float percWasteland = counts[10] / (float)safeCount;
                bool distributionOk = true;
                if (percTundra < desiredTundraPerc)
                {
                    th1 = Math.Min(th1 + delta, th3 - 0.01f);
                    distributionOk = false;
                }
                if (percFields < desiredFieldsPerc)
                {
                    th3 = Math.Min(th3 + delta, 1f);
                    distributionOk = false;
                }
                if (percWasteland < desiredWastelandPerc)
                {
                    th3 = Math.Max(th3 - delta, th1 + 0.01f);
                    distributionOk = false;
                }
                if (distributionOk)
                    break;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (mask[x, y] == 1)
                    {
                        float baseTemp = GetBaseTemperature(x, y, width, height);
                        float noiseValue = NoiseGenerator.Generate(x * noiseScale, y * noiseScale, octaves: 3, frequency: 10f);
                        float adjustedNoise = (noiseValue - 0.5f) * noiseAmplitude;
                        float temperature = MathHelper.Clamp(baseTemp + adjustedNoise, minTemperature, maxTemperature);
                        float baseMoisture = GetBaseMoisture(x, y, width, height);
                        float moisture = MathHelper.Clamp(baseMoisture + adjustedNoise, minTemperature, maxTemperature);

                        byte biome;
                        if (temperature < 0.3f)
                        {
                            biome = 9; // Tundra
                        }
                        else if (temperature < 0.7f)
                        {
                            if (moisture >= 0.7f)
                                biome = 8; // ColourfulPlains
                            else
                                biome = 7; // Plains
                        }
                        else
                        {
                            if (moisture < 0.3f)
                                biome = 11; // Savanna
                            else if (moisture < 0.7f)
                                biome = 6;  // Beach
                            else
                                biome = 10; // Wetlands
                        }
                        biomeMap[x, y] = biome;
                    }
                    else
                    {
                        biomeMap[x, y] = world[x, y];
                    }
                }
            }
        }


        float GetBaseTemperature(int x, int y, int width, int height, float[,] matrix = null, float verticalExp = 1f)
        {
            if (matrix == null)
            {
                matrix = new float[10, 10]
                {
                    {0.5f, 0.5f, 0.4f, 0.3f, 0.1f, 0.1f, 0.0f, 0.0f, 0.0f, 0.0f},
                    {0.5f, 0.5f, 0.4f, 0.4f, 0.2f, 0.1f, 0.0f, 0.0f, 0.0f, 0.0f},
                    {0.5f, 0.5f, 0.5f, 0.4f, 0.3f, 0.1f, 0.0f, 0.0f, 0.0f, 0.0f},
                    {0.5f, 0.5f, 0.5f, 0.4f, 0.4f, 0.3f, 0.3f, 0.3f, 0.0f, 0.0f},
                    {0.5f, 0.5f, 0.6f, 0.6f, 0.4f, 0.4f, 0.6f, 0.3f, 0.3f, 0.0f},
                    {0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.5f},
                    {0.6f, 0.6f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f},
                    {0.6f, 0.7f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 1.0f, 1.0f, 1.0f},
                    {0.7f, 0.8f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
                    {0.7f, 0.8f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
                };
            }
            float u = (float)x / (width - 1);
            float v = (float)y / (height - 1);
            float matrixX = u * 9f;
            float matrixY = v * 9f;
            int i = (int)Math.Floor(matrixX);
            int j = (int)Math.Floor(matrixY);
            int i2 = Math.Min(i + 1, 9);
            int j2 = Math.Min(j + 1, 9);
            float fracX = matrixX - i;
            float fracY = matrixY - j;
            float topInterp = MathHelper.Lerp(matrix[j, i], matrix[j, i2], fracX);
            float bottomInterp = MathHelper.Lerp(matrix[j2, i], matrix[j2, i2], fracX);
            float baseTemp = MathHelper.Lerp(topInterp, bottomInterp, fracY);
            baseTemp = MathF.Pow(baseTemp, verticalExp);
            return MathHelper.Clamp(baseTemp, 0f, 1f);
        }

        float GetBaseMoisture(int x, int y, int width, int height, float[,] matrix = null, float verticalExp = 1f)
        {
            if (matrix == null)
            {
                matrix = new float[10, 10]
                {
                    {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
                    {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f},
                    {0.5f, 0.5f, 0.5f, 0.6f, 0.7f, 0.8f, 0.6f, 0.7f, 0.7f, 0.7f},
                    {0.5f, 0.6f, 0.6f, 0.6f, 0.6f, 0.8f, 0.8f, 0.7f, 0.7f, 0.8f},
                    {0.5f, 0.6f, 0.6f, 0.6f, 0.7f, 0.8f, 0.8f, 0.8f, 0.8f, 0.9f},
                    {0.5f, 0.4f, 0.2f, 0.3f, 0.3f, 0.7f, 0.8f, 0.9f, 0.9f, 1.0f},
                    {0.4f, 0.3f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f},
                    {0.3f, 0.3f, 0.2f, 0.5f, 0.4f, 0.4f, 0.5f, 0.3f, 0.2f, 0.2f},
                    {0.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.4f, 0.2f},
                    {0.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, 0.0f}
                };
            }
            float u = (float)x / (width - 1);
            float v = (float)y / (height - 1);
            float matrixX = u * 9f;
            float matrixY = v * 9f;
            int i = (int)Math.Floor(matrixX);
            int j = (int)Math.Floor(matrixY);
            int i2 = Math.Min(i + 1, 9);
            int j2 = Math.Min(j + 1, 9);
            float fracX = matrixX - i;
            float fracY = matrixY - j;
            float topInterp = MathHelper.Lerp(matrix[j, i], matrix[j, i2], fracX);
            float bottomInterp = MathHelper.Lerp(matrix[j2, i], matrix[j2, i2], fracX);
            float baseMoisture = MathHelper.Lerp(topInterp, bottomInterp, fracY);
            baseMoisture = MathF.Pow(baseMoisture, verticalExp);
            return MathHelper.Clamp(baseMoisture, 0f, 1f);
        }

        private byte[,] ExtractMask(byte[,] world, byte maskValue)
        {
            byte[,] mask = new byte[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    mask[x, y] = (world[x, y] == maskValue) ? (byte)1 : (byte)0;
            return mask;
        }

        public byte[,] GetBiomeMap() => biomeMap;

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

                    float temp = GetBaseTemperature(ix, iy, width, height);
                    Color baseColor = Color.Lerp(Color.Blue, Color.Red, temp);

                    bool isSafe = (mask[ix, iy] == 1);
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
                                if (nx < 0 || nx >= width || ny < 0 || ny >= height || mask[nx, ny] == 0)
                                {
                                    isEdge = true;
                                }
                            }
                        }
                    }
                    if (isEdge)
                    {
                        baseColor = Color.Lerp(baseColor, Color.White, 0.9f);
                    }

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

                    float moisture = GetBaseMoisture(ix, iy, width, height);
                    Color baseColor = Color.Lerp(Color.SandyBrown, Color.Green, moisture);

                    bool isSafe = (mask[ix, iy] == 1);
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
                                if (nx < 0 || nx >= width || ny < 0 || ny >= height || mask[nx, ny] == 0)
                                {
                                    isEdge = true;
                                }
                            }
                        }
                    }
                    if (isEdge)
                    {
                        baseColor = Color.Lerp(baseColor, Color.White, 0.9f);
                    }

                    textureData[ty * textureWidth + tx] = baseColor;
                }
            }

            texture.SetData(textureData);
            return texture;
        }
    }
}
