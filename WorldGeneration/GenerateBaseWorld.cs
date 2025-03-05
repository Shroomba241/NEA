using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompSci_NEA.WorldGeneration
{
    public class GenerateBaseWorld
    {
        private readonly int totalChunksX;
        private readonly int totalChunksY;
        private readonly int chunkSize;
        private readonly int seed;
        private readonly float _centerX;
        private readonly float _centerY;
        private readonly float _totalWidth;
        private readonly float _totalHeight;
        private readonly float _scaleFactor;

        private byte[,] _biomeMap;
        private BiomeGenerator leftBiomeGen, middleBiomeGen, rightBiomeGen;

        public GenerateBaseWorld(int totalChunksX, int totalChunksY, int chunkSize, int seed)
        {
            this.totalChunksX = totalChunksX;
            this.totalChunksY = totalChunksY;
            this.chunkSize = chunkSize;
            this.seed = seed;

            _totalWidth = totalChunksX * chunkSize;
            _totalHeight = totalChunksY * chunkSize;
            _centerX = _totalWidth * 0.5f;
            _centerY = _totalHeight * 0.5f;
            _scaleFactor = totalChunksX / 16f;
        }

        public byte GenerateTile(int x, int y)
        {
            float normX = (x - _centerX) / (_totalWidth * 0.35f);
            float normY = (y - _centerY) / (_totalHeight * 0.25f);
            float baseShape = (normX * normX) + (normY * normY);
            baseShape -= NoiseGenerator.Generate(x * 0.005f, y * 0.005f) * 0.3f;

            float largeNoise = NoiseGenerator.Generate(x * 0.0067f, y * 0.0067f, 3, 50f, 0.5f, 2.5f);
            float mediumNoise = NoiseGenerator.Generate(x * 0.025f, y * 0.025f, 4, 20f, 0.6f, 2.2f);
            float smallNoise = NoiseGenerator.Generate(x * 0.1f, y * 0.1f, 5, 10f, 0.8f, 2.0f);

            float coastlineDistortion = (largeNoise * 0.4f) + (mediumNoise * 0.3f) + (smallNoise * 0.6f);
            float finalShape = baseShape - coastlineDistortion;
            float landThreshold = 0.9f - (largeNoise * 0.1f) + (mediumNoise * 0.05f);

            float t = y / _totalHeight;

            float leftRiverCenter = MathHelper.Lerp(_centerX - 110f * _scaleFactor, _centerX - 150f * _scaleFactor, t) +
                                    ((NoiseGenerator.Generate(1000, y * 0.01f) - 0.5f) * 2 * _totalWidth * 0.05f);

            float rightRiverCenter = MathHelper.Lerp(_centerX + 200f * _scaleFactor, _centerX + 400f * _scaleFactor, t) -
                                     (100f * _scaleFactor * 4 * t * (1 - t)) -
                                     ((NoiseGenerator.Generate(1000, y * 0.01f) - 0.5f) * 2 * _totalWidth * 0.05f);

            if (MathF.Abs(x - leftRiverCenter) < 5f || MathF.Abs(x - rightRiverCenter) < 5f)
                return 2; // River or sea tile

            // Zone assignment: Left zone is tile 3, middle zone is tile 5, right zone is tile 4.
            return finalShape < landThreshold
                ? (x < leftRiverCenter ? (byte)3 : x > rightRiverCenter ? (byte)4 : (byte)5)
                : (byte)2;
        }

        public byte GenerateFinalTile(int x, int y)
        {
            byte baseTile = GenerateTile(x, y);

            if (baseTile == 3 || baseTile == 5 || baseTile == 4)
            {
                if (_biomeMap == null)
                {
                    byte[,] world = new byte[totalChunksX * chunkSize, totalChunksY * chunkSize];
                    for (int i = 0; i < world.GetLength(0); i++)
                        for (int j = 0; j < world.GetLength(1); j++)
                            world[i, j] = GenerateTile(i, j);

                    // Define rectangles for each zone.
                    // (For demonstration, these are fixed. In a real system, compute them based on tile type.)
                    Rectangle leftZoneRect = new Rectangle(0, 0, 800, world.GetLength(1));
                    Rectangle middleZoneRect = new Rectangle(200, 0, 1000, world.GetLength(1));
                    Rectangle rightZoneRect = new Rectangle(800, 0, world.GetLength(0) -800, world.GetLength(1));

                    // Left zone (tile 3): use Normal functions with custom percentages.
                    leftBiomeGen = new BiomeGenerator(
                        world,
                        leftZoneRect,
                        safeTile: 3,
                        temperatureFunc: BiomeFuncts.NormalTemperature,
                        secondaryFunc: BiomeFuncts.NormalMoisture,
                        mappingFunc: BiomeFuncts.NormalMapping,
                        coldPercentage: 0.10f,
                        mediumPercentage: 0.60f);

                    middleBiomeGen = new BiomeGenerator(
                        world,
                        middleZoneRect,
                        safeTile: 5,
                        temperatureFunc: BiomeFuncts.SpookyTemperature,
                        secondaryFunc: BiomeFuncts.Spookiness,
                        mappingFunc: BiomeFuncts.SpookyMapping,
                        coldPercentage: 0.25f,
                        mediumPercentage: 0.45f);

                    rightBiomeGen = new BiomeGenerator(
                        world,
                        rightZoneRect,
                        safeTile: 4,
                        temperatureFunc: BiomeFuncts.BloodTemperature,
                        secondaryFunc: BiomeFuncts.BloodIntensity,
                        mappingFunc: BiomeFuncts.BloodMapping,
                        coldPercentage: 0.2f,
                        mediumPercentage: 0.6f);

                    // Merge the biome maps based on the original zone assignment.
                    _biomeMap = new byte[world.GetLength(0), world.GetLength(1)];
                    byte[,] mapLeft = leftBiomeGen.GetBiomeMap();
                    byte[,] mapMiddle = middleBiomeGen.GetBiomeMap();
                    byte[,] mapRight = rightBiomeGen.GetBiomeMap();
                    for (int i = 0; i < world.GetLength(0); i++)
                    {
                        for (int j = 0; j < world.GetLength(1); j++)
                        {
                            byte tile = world[i, j];
                            if (tile == 3)
                                _biomeMap[i, j] = mapLeft[i - leftZoneRect.X, j - leftZoneRect.Y];
                            else if (tile == 5)
                                _biomeMap[i, j] = mapMiddle[i - middleZoneRect.X, j - middleZoneRect.Y];
                            else if (tile == 4)
                                _biomeMap[i, j] = mapRight[i - rightZoneRect.X, j - rightZoneRect.Y];
                            else
                                _biomeMap[i, j] = tile;
                        }
                    }
                }
                return _biomeMap[x, y];
            }

            return baseTile;
        }

        // Texture generation methods can remain unchanged.
    }
}
