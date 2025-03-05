using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompSci_NEA.WorldGeneration
{
    public static class BiomeFuncts
    {
        public static float InterpolateMatrix(float[,] matrix, int x, int y, int width, int height, float verticalExp = 1f)
        {
            float u = (float)x / (width - 1);
            float v = (float)y / (height - 1);
            int mWidth = matrix.GetLength(1);
            int mHeight = matrix.GetLength(0);
            float matrixX = u * (mWidth - 1);
            float matrixY = v * (mHeight - 1);
            int i = (int)Math.Floor(matrixX);
            int j = (int)Math.Floor(matrixY);
            int i2 = Math.Min(i + 1, mWidth - 1);
            int j2 = Math.Min(j + 1, mHeight - 1);
            float fracX = matrixX - i;
            float fracY = matrixY - j;
            float topInterp = MathHelper.Lerp(matrix[j, i], matrix[j, i2], fracX);
            float bottomInterp = MathHelper.Lerp(matrix[j2, i], matrix[j2, i2], fracX);
            float value = MathHelper.Lerp(topInterp, bottomInterp, fracY);
            value = (float)Math.Pow(value, verticalExp);
            return MathHelper.Clamp(value, 0f, 1f);
        }

        static float[,] normalTempMatrix = new float[10, 10]
        {
            {0.5f, 0.5f, 0.4f, 0.3f, 0.1f, 0.1f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.5f, 0.5f, 0.4f, 0.4f, 0.2f, 0.1f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.5f, 0.5f, 0.5f, 0.4f, 0.3f, 0.1f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.5f, 0.5f, 0.5f, 0.4f, 0.4f, 0.3f, 0.3f, 0.3f, 0.0f, 0.0f},
            {0.5f, 0.5f, 0.6f, 0.6f, 0.4f, 0.4f, 0.4f, 0.3f, 0.3f, 0.0f},
            {0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.5f, 0.5f, 0.6f, 0.6f, 0.5f},
            {0.6f, 0.7f, 0.8f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f},
            {0.7f, 0.7f, 0.9f, 0.8f, 0.8f, 0.8f, 0.8f, 1.0f, 1.0f, 1.0f},
            {0.7f, 0.9f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
            {0.7f, 0.9f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
        };

        static float[,] normalMoistureMatrix = new float[10, 10]
        {
            {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
            {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f},
            {0.5f, 0.5f, 0.5f, 0.6f, 0.7f, 0.8f, 0.6f, 0.7f, 0.7f, 0.7f},
            {0.5f, 0.6f, 0.6f, 0.6f, 0.8f, 0.8f, 0.8f, 0.7f, 0.7f, 0.8f},
            {0.5f, 0.6f, 0.6f, 0.6f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.9f},
            {0.5f, 0.4f, 0.2f, 0.4f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 1.0f},
            {0.4f, 0.3f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f},
            {0.3f, 0.3f, 0.2f, 0.5f, 0.4f, 0.4f, 0.5f, 0.3f, 0.2f, 0.2f},
            {0.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.4f, 0.2f},
            {0.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, 0.0f}
        };

        public static float NormalTemperature(int x, int y, int width, int height)
        {
            return InterpolateMatrix(normalTempMatrix, x, y, width, height);
        }
        public static float NormalMoisture(int x, int y, int width, int height)
        {
            return InterpolateMatrix(normalMoistureMatrix, x, y, width, height);
        }

        static float[,] spookyTempMatrix = new float[10, 10]
        {
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.4f, 0.4f, 0.5f, 0.5f, 0.5f, 0.3f, 0.3f, 0.2f, 0.2f, 0.0f},
            {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.4f, 0.4f, 0.5f, 0.5f, 0.4f},
            {0.5f, 0.6f, 0.7f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f},
            {0.6f, 0.6f, 0.8f, 0.7f, 0.7f, 0.7f, 0.7f, 0.9f, 0.9f, 0.9f},
            {0.6f, 0.8f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.6f, 0.8f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
        };

        static float[,] spookinessMatrix = new float[10, 10]
        {
            {0.0f, 0.1f, 0.1f, 0.1f, 0.2f, 0.2f, 0.5f, 0.7f, 0.9f, 0.9f},
            {0.0f, 0.1f, 0.1f, 0.2f, 0.2f, 0.3f, 0.5f, 0.7f, 0.9f, 0.9f},
            {0.0f, 0.1f, 0.1f, 0.3f, 0.3f, 0.3f, 0.4f, 0.7f, 0.9f, 0.9f},
            {0.0f, 0.2f, 0.2f, 0.2f, 0.3f, 0.3f, 0.4f, 0.6f, 0.9f, 0.9f},
            {0.0f, 0.2f, 0.2f, 0.2f, 0.3f, 0.3f, 0.4f, 0.5f, 0.9f, 0.9f},
            {0.0f, 0.2f, 0.2f, 0.2f, 0.3f, 0.3f, 0.4f, 0.4f, 0.7f, 0.7f},
            {0.0f, 0.2f, 0.2f, 0.2f, 0.2f, 0.3f, 0.3f, 0.3f, 0.6f, 0.6f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.3f, 0.6f, 0.6f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.3f, 0.6f, 0.6f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.3f, 0.6f, 0.6f},
        };

        public static float SpookyTemperature(int x, int y, int width, int height)
        {
            return InterpolateMatrix(spookyTempMatrix, x, y, width, height);
        }
        public static float Spookiness(int x, int y, int width, int height)
        {
            return InterpolateMatrix(spookinessMatrix, x, y, width, height);
        }

        static float[,] bloodTempMatrix = new float[10, 10]
        {
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.2f, 0.2f, 0.2f, 0.2f},
            {0.4f, 0.4f, 0.3f, 0.3f, 0.5f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f},
            {0.4f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
            {0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
        };

        static float[,] bloodMatrix = new float[10, 10]
        {
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
            {0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f},
        };

        public static float BloodTemperature(int x, int y, int width, int height)
        {
            return InterpolateMatrix(bloodTempMatrix, x, y, width, height);
        }
        public static float BloodIntensity(int x, int y, int width, int height)
        {
            return InterpolateMatrix(bloodMatrix, x, y, width, height);
        }

        public static byte NormalMapping(float temperature, float moisture, float th1, float th3)
        {
            if (temperature < th1)
                return 9;
            else if (temperature < th3)
                return (moisture >= 0.7f) ? (byte)8 : (byte)7;
            else
            {
                if (moisture < 0.3f)
                    return 11;
                else if (moisture < 0.7f)
                    return 6;
                else
                    return 11;
            }
        }

        public static byte SpookyMapping(float temperature, float spookiness, float th1, float th3)
        {
            if (temperature < th1) 
            { 
                if (spookiness < 0.5f)
                    return 9;
                else
                    return 15;
            }
            else if (temperature < th3)
            {
                if (spookiness < 0.4f)
                    return 13;
                else
                    return 14;
            }
            else
            {
                if (spookiness < 0.3f)
                    return 6;
                else
                    return 12;
            }
        }

        public static byte BloodMapping(float temperature, float intensity, float th1, float th3)
        {
            if (temperature < th1)
                return 15;
            else if (temperature < th3)
                return 16;
            else
            {
                return 12;
            }
        }
    }
}
