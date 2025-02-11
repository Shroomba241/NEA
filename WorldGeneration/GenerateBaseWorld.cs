/*using Microsoft.Xna.Framework;
using System;

namespace CompSci_NEA.WorldGeneration
{
    public class GenerateBaseWorld
    {
        public int totalChunksX;
        public int totalChunksY;
        public int chunkSize;
        public int seed;

        public GenerateBaseWorld(int totalChunksX, int totalChunksY, int chunkSize, int seed)
        {
            this.totalChunksX = totalChunksX;
            this.totalChunksY = totalChunksY;
            this.chunkSize = chunkSize;
            this.seed = seed;
        }

        public byte GenerateTile(int x, int y)
        {
            // Compute total dimensions in tiles.
            float totalWidth = totalChunksX * chunkSize;
            float totalHeight = totalChunksY * chunkSize;

            // Find the center of the map.
            float centerX = totalWidth * 0.5f;
            float centerY = totalHeight * 0.5f;

            // Create an oval (elliptical) shape for the island.
            float ovalWidthScale = 0.35f;
            float ovalHeightScale = 0.25f;
            float normX = (x - centerX) / (totalWidth * ovalWidthScale);
            float normY = (y - centerY) / (totalHeight * ovalHeightScale);
            float baseShape = (normX * normX) + (normY * normY);

            // Add some base noise.
            float baseNoise = NoiseGenerator.Generate(x / 200f, y / 200f) * 0.3f;
            baseShape -= baseNoise;

            // Add additional noise at different scales.
            float largeScaleNoise = NoiseGenerator.Generate(x / 150f, y / 150f, 3, 50f, 0.5f, 2.5f);
            float mediumNoise = NoiseGenerator.Generate(x / 40f, y / 40f, 4, 20f, 0.6f, 2.2f);
            float smallNoise = NoiseGenerator.Generate(x / 10f, y / 10f, 5, 10f, 0.8f, 2.0f);
            float coastlineDistortion = (largeScaleNoise * 0.4f) + (mediumNoise * 0.3f) + (smallNoise * 0.6f);
            float finalShape = baseShape - coastlineDistortion;

            float landThreshold = 0.9f - (largeScaleNoise * 0.1f) + (mediumNoise * 0.05f);

            // Compute the vertical fraction (0 at the top, 1 at the bottom).
            float t = y / totalHeight;

            //
            // Scale the river offsets based on the current total chunks.
            // (Assuming the original tuning was for 16 chunks horizontally.)
            //
            float scaleFactor = totalChunksX / 16f;  // Adjust this base if needed.

            // These numbers come from your original offsets.
            // They will be scaled up/down so that the relative positions remain similar.
            float leftRiverTopXOffset = -110f * scaleFactor;
            float leftRiverBottomXOffset = -150f * scaleFactor;
            float rightRiverTopXOffset = 200f * scaleFactor;
            float rightRiverBottomXOffset = 400f * scaleFactor;
            float rightRiverCurvature = 100f * scaleFactor;

            // Compute the ideal (noisy-free) positions for the river centers.
            float leftRiverIdeal = MathHelper.Lerp(centerX + leftRiverTopXOffset, centerX + leftRiverBottomXOffset, t);
            float rightRiverIdealLinear = MathHelper.Lerp(centerX + rightRiverTopXOffset, centerX + rightRiverBottomXOffset, t);
            float rightRiverParabolicOffset = rightRiverCurvature * 4 * t * (1 - t);
            float rightRiverIdeal = rightRiverIdealLinear - rightRiverParabolicOffset;

            // Add some horizontal deviation to make the rivers meander.
            float deviationMagnitude = totalWidth * 0.05f;
            float deviation = (NoiseGenerator.Generate(1000, y / 100f) - 0.5f) * 2 * deviationMagnitude;

            float leftRiverCenter = leftRiverIdeal + deviation;
            float rightRiverCenter = rightRiverIdeal - deviation;

            // Define a constant river half-width (in tiles).
            float riverHalfWidth = 5f;
            if (MathF.Abs(x - leftRiverCenter) < riverHalfWidth ||
                MathF.Abs(x - rightRiverCenter) < riverHalfWidth)
            {
                return 2;  // River tile.
            }

            // Choose land biome based on x-position relative to the rivers.
            if (finalShape < landThreshold)
            {
                if (x < leftRiverCenter)
                {
                    return 3;  // Left Land Zone (Biome 1).
                }
                else if (x > rightRiverCenter)
                {
                    return 4;  // Right Land Zone (Biome 3).
                }
                else
                {
                    return 5;  // Middle Land Zone (Biome 2).
                }
            }
            else
            {
                return 2;  // Sea tile.
            }
        }
    }
}
*/

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
                return 2; // River tile

            return finalShape < landThreshold
                ? (x < leftRiverCenter ? (byte)3 : x > rightRiverCenter ? (byte)4 : (byte)5)
                : (byte)2; // Sea tile
        }
    }
}
