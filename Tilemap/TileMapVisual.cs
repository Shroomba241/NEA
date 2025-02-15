

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
    public class TileMapVisual : BaseTileMap
    {
        private int seed;

        public TileMapVisual(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY, int seed)
            : base(graphicsDevice, totalChunksX, totalChunksY, seed)
        {
            this.seed = seed;
            tileAtlas = TextureManager.ATLAS;
            NoiseGenerator.SetSeed(seed);
        }

        private GenerateBaseWorld _baseWorldGenerator;
        private GenerateBaseWorld BaseWorldGenerator
        {
            get
            {
                if (_baseWorldGenerator == null)
                {
                    _baseWorldGenerator = new GenerateBaseWorld(totalChunksX, totalChunksY, chunkSize, MOVEDEBUGTEST.SEED);
                }
                return _baseWorldGenerator;
            }
        }

        public void ResetBiomePartition()
        {
        }

        public override byte GenerateTile(int x, int y)
        {
            return BaseWorldGenerator.GenerateFinalTile(x, y);
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            int chunkX = (int)(player.Position.X / (48 * chunkSize));
            int chunkY = (int)(player.Position.Y / (48 * chunkSize));

            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    int currentChunkX = chunkX + offsetX;
                    int currentChunkY = chunkY + offsetY;
                    DrawChunk(spriteBatch, currentChunkX, currentChunkY);
                }
            }
        }

        private void DrawChunk(SpriteBatch spriteBatch, int chunkX, int chunkY)
        {
            if (!chunks.ContainsKey(new Point(chunkX, chunkY)))
            {
                return;
            }

            byte[,] chunk = chunks[new Point(chunkX, chunkY)];

            for (int localY = 0; localY < chunkSize; localY++)
            {
                for (int localX = 0; localX < chunkSize; localX++)
                {
                    byte tileID = chunk[localY, localX];
                    if (!tileTypes.ContainsKey(tileID))
                        continue;

                    TileType tileType = tileTypes[tileID];

                    int worldTileX = chunkX * chunkSize + localX;
                    int worldTileY = chunkY * chunkSize + localY;

                    Vector2 position = new Vector2(worldTileX * 48, worldTileY * 48);

                    Rectangle sourceRect = tileType.GetEffectiveTextureRegion(worldTileX, worldTileY);
                    sourceRect.Inflate(-1, -1);

                    Rectangle destinationRect = new Rectangle((int)position.X, (int)position.Y, 48, 48);
                    spriteBatch.Draw(tileAtlas, destinationRect, sourceRect, Color.White);
                    //spriteBatch.Draw(tileAtlas, destinationRect, sourceRect, tileType.Color);
                }
            }
        }

        public Texture2D GenerateTemperatureTexture(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight)
        {
            return _baseWorldGenerator.GenerateTemperatureTexture(graphicsDevice, textureWidth, textureHeight);
        }

        public Texture2D GenerateMoistureTexture(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight)
        {
            return _baseWorldGenerator.GenerateMoistureTexture(graphicsDevice, textureWidth, textureHeight);
        }

        public Texture2D GenerateMapTexture(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight, StructureTileMap structures)
        {
            Texture2D mapTexture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
            Color[] mapData = new Color[textureWidth * textureHeight];

            int worldWidthInTiles = totalChunksX * chunkSize;
            int worldHeightInTiles = totalChunksY * chunkSize;

            for (int chunkY = 0; chunkY < totalChunksY; chunkY++)
            {
                for (int chunkX = 0; chunkX < totalChunksX; chunkX++)
                {
                    if (!chunks.ContainsKey(new Point(chunkX, chunkY)))
                        continue;

                    byte[,] chunk = chunks[new Point(chunkX, chunkY)];

                    for (int tileY = 0; tileY < chunkSize; tileY++)
                    {
                        for (int tileX = 0; tileX < chunkSize; tileX++)
                        {
                            byte tileID = chunk[tileY, tileX];
                            Color tileColor = tileTypes.ContainsKey(tileID) ? tileTypes[tileID].Color : Color.Red;
                            int worldX = chunkX * chunkSize + tileX;
                            int worldY = chunkY * chunkSize + tileY;
                            int pixelX = (int)((worldX / (float)worldWidthInTiles) * textureWidth);
                            int pixelY = (int)((worldY / (float)worldHeightInTiles) * textureHeight);
                            pixelX = Math.Clamp(pixelX, 0, textureWidth - 1);
                            pixelY = Math.Clamp(pixelY, 0, textureHeight - 1);
                            mapData[pixelY * textureWidth + pixelX] = tileColor;
                        }
                    }
                }
            }

            mapTexture.SetData(mapData);
            Console.WriteLine("map generated");
            return mapTexture;
        }
    }

}
