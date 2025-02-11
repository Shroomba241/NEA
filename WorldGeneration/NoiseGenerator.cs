using CompSci_NEA.Scenes;
using System;

namespace CompSci_NEA.WorldGeneration
{
    public static class NoiseGenerator
    {
        private static int[] permutationTable;
        private static Random random;

        public static void SetSeed(int seed)
        {
            random = new Random(MOVEDEBUGTEST.SEED);
            GeneratePermutationTable();
        }

        static NoiseGenerator()
        {
            random = new Random(MOVEDEBUGTEST.SEED); 
            GeneratePermutationTable();
        }

        private static void GeneratePermutationTable()
        {
            permutationTable = new int[512];
            int[] p = new int[256];

            for (int i = 0; i < 256; i++)
                p[i] = i;

            for (int i = 0; i < 256; i++)
            {
                int swap = random.Next(256);
                (p[i], p[swap]) = (p[swap], p[i]);
            }

            for (int i = 0; i < 512; i++)
                permutationTable[i] = p[i % 256];
        }

        private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
        private static float Lerp(float a, float b, float t) => a + t * (b - a);
        private static float Grad(int hash, float x, float y)
        {
            int h = hash & 7;
            float u = h < 4 ? x : y;
            float v = h < 4 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        public static float Generate(float x, float y, int octaves = 5, float frequency = 18f, float persistence = 0.6f, float lacunarity = 2.5f)
        {
            float amplitude = 1;
            float maxValue = 0;
            float noiseSum = 0;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = x / frequency;
                float sampleY = y / frequency;

                int X = (int)Math.Floor(sampleX) & 255;
                int Y = (int)Math.Floor(sampleY) & 255;

                float localX = sampleX - MathF.Floor(sampleX);
                float localY = sampleY - MathF.Floor(sampleY);

                float u = Fade(localX);
                float v = Fade(localY);

                int A = permutationTable[X] + Y;
                int B = permutationTable[X + 1] + Y;

                float value = Lerp(
                    Lerp(Grad(permutationTable[A], localX, localY), Grad(permutationTable[B], localX - 1, localY), u),
                    Lerp(Grad(permutationTable[A + 1], localX, localY - 1), Grad(permutationTable[B + 1], localX - 1, localY - 1), u),
                    v);

                noiseSum += value * amplitude;
                maxValue += amplitude;

                amplitude *= persistence;
                frequency /= lacunarity;
            }

            return (noiseSum / maxValue + 1) / 2; // Normalize to 0 < noise < 1
        }
    }
}
