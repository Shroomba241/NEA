using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.WorldGeneration
{
    public static class PerlinNoise
    {
        private static Random random = new Random();

        public static float[,] GenerateNoise(int width, int height, float scale)
        {
            float[,] noiseMap = new float[width, height];

            if (scale <= 0)
                scale = 0.01f;

            float offsetX = random.Next(0, 100000);
            float offsetY = random.Next(0, 100000);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sampleX = (x + offsetX) / scale;
                    float sampleY = (y + offsetY) / scale;

                    float noiseValue = (float)NoiseGenerator.Generate(sampleX, sampleY);
                    noiseValue = (noiseValue + 1) / 2; // Normalize to 0-1
                    noiseMap[x, y] = noiseValue;
                }
            }

            return noiseMap;
        }
    }
}
