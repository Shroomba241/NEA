using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using CompSci_NEA.WorldGeneration; 
using Microsoft.Xna.Framework.Graphics;

namespace CompSci_NEA.WorldGeneration
{
    public class GenerateBaseWorld
    {
        private readonly int totalChunksX;
        private readonly int totalChunksY;
        private readonly int chunkSize;
        private readonly int seed;
        private readonly float centerX;
        private readonly float centerY;
        private readonly float totalWidth;
        private readonly float totalHeight;
        private readonly float scaleFactor;

        private BiomeGenerator biomeGenerator;
        private byte[,] biomeMap;

        public GenerateBaseWorld(int totalChunksX, int totalChunksY, int chunkSize, int seed)
        {
            this.totalChunksX = totalChunksX;
            this.totalChunksY = totalChunksY;
            this.chunkSize = chunkSize;
            this.seed = seed;

            totalWidth = totalChunksX * chunkSize;
            totalHeight = totalChunksY * chunkSize;
            centerX = totalWidth * 0.5f;
            centerY = totalHeight * 0.5f;
            scaleFactor = totalChunksX / 16f;
        }

        public byte GenerateTile(int x, int y)
        {
            float normX = (x - centerX) / (totalWidth * 0.35f);
            float normY = (y - centerY) / (totalHeight * 0.25f);
            float baseShape = (normX * normX) + (normY * normY);
            baseShape -= NoiseGenerator.Generate(x * 0.005f, y * 0.005f) * 0.3f;

            float largeNoise = NoiseGenerator.Generate(x * 0.0067f, y * 0.0067f, 3, 50f, 0.5f, 2.5f);
            float mediumNoise = NoiseGenerator.Generate(x * 0.025f, y * 0.025f, 4, 20f, 0.6f, 2.2f);
            float smallNoise = NoiseGenerator.Generate(x * 0.1f, y * 0.1f, 5, 10f, 0.8f, 2.0f);

            float coastlineDistortion = (largeNoise * 0.4f) + (mediumNoise * 0.3f) + (smallNoise * 0.6f);
            float finalShape = baseShape - coastlineDistortion;

            float landThreshold = 0.9f - (largeNoise * 0.1f) + (mediumNoise * 0.05f);

            float t = y / totalHeight;

            float leftRiverCenter = MathHelper.Lerp(centerX - 110f * scaleFactor, centerX - 150f * scaleFactor, t) +
                                    ((NoiseGenerator.Generate(1000, y * 0.01f) - 0.5f) * 2 * totalWidth * 0.05f);

            float rightRiverCenter = MathHelper.Lerp(centerX + 200f * scaleFactor, centerX + 400f * scaleFactor, t) -
                                     (100f * scaleFactor * 4 * t * (1 - t)) -
                                     ((NoiseGenerator.Generate(1000, y * 0.01f) - 0.5f) * 2 * totalWidth * 0.05f);

            if (MathF.Abs(x - leftRiverCenter) < 5f || MathF.Abs(x - rightRiverCenter) < 5f)
                return 2; // River or sea tile

            return finalShape < landThreshold
                ? (x < leftRiverCenter ? (byte)3 : x > rightRiverCenter ? (byte)4 : (byte)5)
                : (byte)2;
        }

        public byte GenerateFinalTile(int x, int y)
        {
            byte baseTile = GenerateTile(x, y);

            if (baseTile == 3)
            {
                if (biomeMap == null)
                {
                    byte[,] world = new byte[totalChunksX * chunkSize, totalChunksY * chunkSize];

                    for (int i = 0; i < world.GetLength(0); i++)
                    {
                        for (int j = 0; j < world.GetLength(1); j++)
                        {
                            world[i, j] = GenerateTile(i, j);
                        }
                    }

                    biomeGenerator = new BiomeGenerator(world, 800, world.GetLength(1)); //300 or 800
                    biomeMap = biomeGenerator.GetBiomeMap();
                }

                return biomeMap[x, y]; 
            }

            return baseTile; 
        }

        public Texture2D GenerateTemperatureTexture(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight)
        {
            return biomeGenerator.GenerateTemperatureTexture(graphicsDevice, textureWidth, textureHeight);
        }

        public Texture2D GenerateMoistureTexture(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight)
        {
            return biomeGenerator.GenerateMoistureTexture(graphicsDevice, textureWidth, textureHeight);
        }
    }
}
