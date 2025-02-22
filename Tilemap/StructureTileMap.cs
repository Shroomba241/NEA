using CompSci_NEA.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using CompSci_NEA.WorldGeneration.Structures;

namespace CompSci_NEA.Tilemap
{
    public class StructureTileMap : BaseTileMap
    {
        private StoneBridge stoneBridge;

        public StructureTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY, int seed)
            : base(graphicsDevice, totalChunksX, totalChunksY, seed)
        {
            tileAtlas = TextureManager.ATLAS;
            stoneBridge = new StoneBridge(tileAtlas, seed);
            stoneBridge.Generate(400 * 48, 400 * 48, 100);
        }

        public override byte GenerateTile(int x, int y)
        {
            return 0;
        }

        public void DrawBackgroundLayer(SpriteBatch spriteBatch, Entities.Player player)
        {
            int chunkX = (int)(player.Position.X / (48 * chunkSize));
            int chunkY = (int)(player.Position.Y / (48 * chunkSize));
            for (int offsetY = -2; offsetY <= 2; offsetY++)
            {
                for (int offsetX = -2; offsetX <= 2; offsetX++)
                {
                    int currentChunkX = chunkX + offsetX;
                    int currentChunkY = chunkY + offsetY;
                    Rectangle chunkRect = new Rectangle(currentChunkX * chunkSize * 48, currentChunkY * chunkSize * 48, chunkSize * 48, chunkSize * 48);
                    if (chunks.ContainsKey(new Point(currentChunkX, currentChunkY)))
                    {
                        byte[,] chunk = chunks[new Point(currentChunkX, currentChunkY)];
                        for (int y = 0; y < chunkSize; y++)
                        {
                            for (int x = 0; x < chunkSize; x++)
                            {
                                byte tileID = chunk[y, x];
                                if (!tileTypes.ContainsKey(tileID))
                                    continue;

                                TileType tileType = tileTypes[tileID];
                                Vector2 position = new Vector2((currentChunkX * chunkSize + x) * 48, (currentChunkY * chunkSize + y) * 48);
                                // Use the new GetEffectiveTextureRegion call.
                                Rectangle sourceRect = tileType.GetEffectiveTextureRegion((int)position.X, (int)position.Y);
                                spriteBatch.Draw(tileAtlas, position, sourceRect, tileType.Color, 0f, Vector2.Zero, 3.0f, SpriteEffects.None, 0f);
                            }
                        }
                    }
                    stoneBridge.DrawBackgroundInRect(spriteBatch, chunkRect);
                }
            }
        }

        public void DrawForegroundLayer(SpriteBatch spriteBatch, Entities.Player player)
        {
            int chunkX = (int)(player.Position.X / (48 * chunkSize));
            int chunkY = (int)(player.Position.Y / (48 * chunkSize));
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    int currentChunkX = chunkX + offsetX;
                    int currentChunkY = chunkY + offsetY;
                    Rectangle chunkRect = new Rectangle(currentChunkX * chunkSize * 48, currentChunkY * chunkSize * 48, chunkSize * 48, chunkSize * 48);
                    stoneBridge.DrawForegroundInRect(spriteBatch, chunkRect);
                }
            }
        }

        public List<Rectangle> StoneBridgeColliders
        {
            get { return stoneBridge.GetBottomRailingColliders(); }
        }
    }
}
