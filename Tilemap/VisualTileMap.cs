using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using CompSci_NEA.WorldGeneration;
using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using CompSci_NEA.Scenes;
using System.Collections.Generic;

namespace CompSci_NEA.Tilemap
{
    public class VisualTileMap : BaseTileMap
    {
        private int _seed;
        private Texture2D _mapTexture;
        private Color[] _mapData;
        private int _textureWidth, _textureHeight;
        private HashSet<Point> _processedChunks = new HashSet<Point>();

        public VisualTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY, int seed)
            : base(graphicsDevice, totalChunksX, totalChunksY, seed)
        {
            _seed = seed;
            tileAtlas = TextureManager.ATLAS;
            NoiseGenerator.SetSeed(seed);
        }

        private GenerateBaseWorld _baseWorldGenerator;
        private GenerateBaseWorld _baseWorldGeneratorProp
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
            return _baseWorldGeneratorProp.GenerateFinalTile(x, y);
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

        public void InitializeMapTexture(GraphicsDevice graphicsDevice, int texWidth, int texHeight, StructureTileMap structures)
        {
            _textureWidth = texWidth;
            _textureHeight = texHeight;
            _mapTexture = new Texture2D(graphicsDevice, _textureWidth, _textureHeight);
            _mapData = new Color[_textureWidth * _textureHeight];
            for (int i = 0; i < _mapData.Length; i++)
                _mapData[i] = Color.Black;
            _processedChunks.Clear();
        }

        public Texture2D UpdateMapTexture(GraphicsDevice graphicsDevice)
        {
            int worldWidthInTiles = totalChunksX * chunkSize;
            int worldHeightInTiles = totalChunksY * chunkSize;

            foreach (var kvp in chunks)
            {
                Point chunkCoords = kvp.Key;
                if (_processedChunks.Contains(chunkCoords))
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

                        int pixelX = (int)((worldTileX / (float)worldWidthInTiles) * _textureWidth);
                        int pixelY = (int)((worldTileY / (float)worldHeightInTiles) * _textureHeight);

                        pixelX = Math.Clamp(pixelX, 0, _textureWidth - 1);
                        pixelY = Math.Clamp(pixelY, 0, _textureHeight - 1);

                        _mapData[pixelY * _textureWidth + pixelX] = tileColor;
                    }
                }
                _processedChunks.Add(chunkCoords);
            }
            _mapTexture.SetData(_mapData);
            return _mapTexture;
        }
    }
}
