using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using CompSci_NEA.WorldGeneration;
using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using CompSci_NEA.Scenes;

namespace CompSci_NEA.Tilemap
{
    public class VisualTileMap : BaseTileMap
    {
        private int seed;
        private Texture2D mapTexture;  // Our map texture
        private Color[] mapData;       // Pixel data for the map texture
        private int textureWidth, textureHeight;
        // Keep track of which chunks have been added to the map texture.
        private HashSet<Point> processedChunks = new HashSet<Point>();

        public VisualTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY, int seed)
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
            Point key = new Point(chunkX, chunkY);
            if (!chunks.ContainsKey(key))
            {
                // Lazily generate the chunk.
                chunks[key] = CreateChunk(chunkX, chunkY);
            }

            byte[,] chunk = chunks[key];

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
                }
            }
        }

        /// <summary>
        /// Call this when the map is (re)generated so the texture is reset.
        /// </summary>
        public void InitializeMapTexture(GraphicsDevice graphicsDevice, int texWidth, int texHeight, StructureTileMap structures)
        {
            textureWidth = texWidth;
            textureHeight = texHeight;
            mapTexture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
            mapData = new Color[textureWidth * textureHeight];
            // Initialize all pixels to a default background (e.g., black)
            for (int i = 0; i < mapData.Length; i++)
                mapData[i] = Color.Black;
            processedChunks.Clear();
        }

        /// <summary>
        /// Incrementally updates the map texture with any chunks that have been generated but not yet added.
        /// Call this (for example) every 5 seconds.
        /// </summary>
        public Texture2D UpdateMapTexture(GraphicsDevice graphicsDevice)
        {
            // Calculate world dimensions in tiles.
            int worldWidthInTiles = totalChunksX * chunkSize;
            int worldHeightInTiles = totalChunksY * chunkSize;

            foreach (var kvp in chunks)
            {
                Point chunkCoords = kvp.Key;
                // Skip if this chunk is already processed.
                if (processedChunks.Contains(chunkCoords))
                    continue;

                byte[,] chunk = kvp.Value;
                for (int localY = 0; localY < chunkSize; localY++)
                {
                    for (int localX = 0; localX < chunkSize; localX++)
                    {
                        byte tileID = chunk[localY, localX];
                        Color tileColor = tileTypes.ContainsKey(tileID) ? tileTypes[tileID].Color : Color.Red;

                        int worldTileX = chunkCoords.X * chunkSize + localX;
                        int worldTileY = chunkCoords.Y * chunkSize + localY;

                        // Map the tile position to a pixel in the texture.
                        int pixelX = (int)((worldTileX / (float)worldWidthInTiles) * textureWidth);
                        int pixelY = (int)((worldTileY / (float)worldHeightInTiles) * textureHeight);

                        // Clamp to ensure we don't exceed the texture bounds.
                        pixelX = Math.Clamp(pixelX, 0, textureWidth - 1);
                        pixelY = Math.Clamp(pixelY, 0, textureHeight - 1);

                        mapData[pixelY * textureWidth + pixelX] = tileColor;
                    }
                }
                // Mark this chunk as processed.
                processedChunks.Add(chunkCoords);
            }

            // Update the texture with the new map data.
            mapTexture.SetData(mapData);
            return mapTexture;
        }

        // The existing GenerateMapTexture method is left here if needed.
        public Texture2D GenerateMapTexture(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight, StructureTileMap structures)
        {
            // This version generates the texture from scratch. 
            // You might call InitializeMapTexture and then repeatedly call UpdateMapTexture() to add new chunks.
            Texture2D fullMapTexture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
            Color[] fullMapData = new Color[textureWidth * textureHeight];

            int worldWidthInTiles = totalChunksX * chunkSize;
            int worldHeightInTiles = totalChunksY * chunkSize;

            for (int chunkY = 0; chunkY < totalChunksY; chunkY++)
            {
                for (int chunkX = 0; chunkX < totalChunksX; chunkX++)
                {
                    Point key = new Point(chunkX, chunkY);
                    if (!chunks.ContainsKey(key))
                        continue;

                    byte[,] chunk = chunks[key];

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
                            fullMapData[pixelY * textureWidth + pixelX] = tileColor;
                        }
                    }
                }
            }

            fullMapTexture.SetData(fullMapData);
            Console.WriteLine("Map generated from scratch");
            return fullMapTexture;
        }
    }
}
