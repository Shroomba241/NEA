using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using CompSci_NEA.WorldGeneration;
using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using System.Collections.Generic;
using CompSci_NEA.Scenes;
using CompSci_NEA.WorldGeneration.Structures;

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
            float totalWidth = totalChunksX * chunkSize;
            float totalHeight = totalChunksY * chunkSize;
            float centerX = totalWidth * 0.5f;
            float centerY = totalHeight * 0.5f;
            float scaleFactor = totalChunksX / 16f;
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
                return 2;

            return finalShape < landThreshold
                ? (x < leftRiverCenter ? (byte)3 : x > rightRiverCenter ? (byte)4 : (byte)5) : (byte)2;
        }
        

        public bool IsTileCollidable(int tileX, int tileY)
        {
            return GetTile(tileX, tileY) == 2;
        }

        public void ClearCollisionUnderBridge(WoodBridge bridge)
        {
            
            Rectangle bridgeRect = new Rectangle(
                (int)bridge.Position.X,
                (int)bridge.Position.Y,
                480,
                144
            );

            int startTileX = bridgeRect.X / 48;
            int startTileY = bridgeRect.Y / 48;
            int endTileX = (bridgeRect.X + bridgeRect.Width) / 48;
            int endTileY = (bridgeRect.Y + bridgeRect.Height) / 48;

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    SetTile(x, y, 1);
                }
            }
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
