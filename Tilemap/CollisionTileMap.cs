using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using CompSci_NEA.WorldGeneration;
using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using System.Collections.Generic;
using CompSci_NEA.Scenes;

namespace CompSci_NEA.Tilemap
{
    public class CollisionTileMap : BaseTileMap
    {
        public List<Rectangle> ExtraColliders { get; set; } = new List<Rectangle>();

        public CollisionTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY, int seed)
            : base(graphicsDevice, totalChunksX, totalChunksY, seed)
        {
            NoiseGenerator.SetSeed(MOVEDEBUGTEST.SEED);
        }

        public override byte GenerateTile(int x, int y)
        {
            float centerX = (totalChunksX * chunkSize) * 0.5f;
            float centerY = (totalChunksY * chunkSize) * 0.5f;

            float ovalWidthScale = 0.35f;
            float ovalHeightScale = 0.25f;
            float normX = (x - centerX) / (totalChunksX * chunkSize * ovalWidthScale);
            float normY = (y - centerY) / (totalChunksY * chunkSize * ovalHeightScale);
            float baseShape = (normX * normX) + (normY * normY);

            float baseNoise = NoiseGenerator.Generate(x / 200f, y / 200f) * 0.3f;
            baseShape -= baseNoise;

            float largeScaleNoise = NoiseGenerator.Generate(x / 150f, y / 150f, 3, 50f, 0.5f, 2.5f);
            float mediumNoise = NoiseGenerator.Generate(x / 40f, y / 40f, 4, 20f, 0.6f, 2.2f);
            float smallNoise = NoiseGenerator.Generate(x / 10f, y / 10f, 5, 10f, 0.8f, 2.0f);

            float coastlineDistortion = (largeScaleNoise * 0.4f) + (mediumNoise * 0.3f) + (smallNoise * 0.6f);
            float finalShape = baseShape - coastlineDistortion;

            float landThreshold = 0.9f - (largeScaleNoise * 0.1f) + (mediumNoise * 0.05f);

            return finalShape < landThreshold ? (byte)1 : (byte)2;
        }

        public bool IsTileCollidable(int tileX, int tileY)
        {
            return GetTile(tileX, tileY) == 2;
        }

        public bool IsColliding(Rectangle rect)
        {
            int tileSize = 48;

            Point[] samplePoints = new Point[]
            {
                new Point(rect.Left, rect.Top),
                new Point(rect.Right - 1, rect.Top),
                new Point(rect.Left, rect.Bottom - 1),
                new Point(rect.Right - 1, rect.Bottom - 1)
            };

            foreach (Point pt in samplePoints)
            {
                int tileX = pt.X / tileSize;
                int tileY = pt.Y / tileSize;
                if (IsTileCollidable(tileX, tileY))
                    return true;
            }

            Point center = new Point(rect.Center.X, rect.Center.Y);
            int centerTileX = center.X / tileSize;
            int centerTileY = center.Y / tileSize;
            if (IsTileCollidable(centerTileX, centerTileY))
                return true;

            foreach (var collider in ExtraColliders)
            {
                if (rect.Intersects(collider))
                    return true;
            }

            return false;
        }

        public void DrawDebug(SpriteBatch spriteBatch, Texture2D debugTexture, Vector2 playerPosition)
        {
            int tilePixelSize = 48;
            int tilesPerChunk = chunkSize;
            int chunkPixelSize = tilesPerChunk * tilePixelSize;

            int playerChunkX = (int)(playerPosition.X / chunkPixelSize);
            int playerChunkY = (int)(playerPosition.Y / chunkPixelSize);

            int debugRange = 2;
            for (int offsetY = -debugRange; offsetY <= debugRange; offsetY++)
            {
                for (int offsetX = -debugRange; offsetX <= debugRange; offsetX++)
                {
                    int currentChunkX = playerChunkX + offsetX;
                    int currentChunkY = playerChunkY + offsetY;
                    Point chunkPoint = new Point(currentChunkX, currentChunkY);
                    if (!chunks.ContainsKey(chunkPoint))
                        continue;

                    byte[,] chunk = chunks[chunkPoint];

                    for (int y = 0; y < chunkSize; y++)
                    {
                        for (int x = 0; x < chunkSize; x++)
                        {
                            int worldX = (currentChunkX * chunkSize + x) * tilePixelSize;
                            int worldY = (currentChunkY * chunkSize + y) * tilePixelSize;

                            if (chunk[y, x] == 2)
                            {
                                Rectangle destRect = new Rectangle(worldX, worldY, tilePixelSize, tilePixelSize);
                                spriteBatch.Draw(debugTexture, destRect, Color.Red * 0.5f);
                            }
                        }
                    }
                }
            }

            foreach (var collider in ExtraColliders)
            {
                spriteBatch.Draw(debugTexture, collider, Color.Blue * 0.5f);
            }
        }
    }
}
