using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace CompSci_NEA.Tilemap
{
    public abstract class BaseTileMap
    {
        protected Dictionary<Point, byte[,]> chunks; 
        protected Dictionary<byte, TileType> tileTypes;
        protected Texture2D tileAtlas;
        protected int chunkSize = 64; 
        protected int totalChunksX; 
        protected int totalChunksY;
        protected Random random;

        protected BaseTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY, int seed)
        {
            this.totalChunksX = totalChunksX;
            this.totalChunksY = totalChunksY;
            chunks = new Dictionary<Point, byte[,]>();
            tileTypes = new Dictionary<byte, TileType>
            {
                { 1, new TileType("GrassTEST", false, new Rectangle(0, 0, 16, 16), Color.Green) },
                { 2, new TileType("WaterTEST", true, new Rectangle(0, 16, 16, 16), Color.DodgerBlue) },
                { 3, new TileType("Grass", false, new Rectangle(16, 0, 16, 16), Color.Green,
                      new List<Rectangle>
                      {
                          new Rectangle(32, 0, 16, 16),
                          new Rectangle(48, 0, 16, 16),
                          new Rectangle(64, 0, 16, 16)
                      }) },
                { 4, new TileType("Dirt", false, new Rectangle(80, 0, 16, 16), Color.SaddleBrown,
                      new List<Rectangle>
                      {
                          new Rectangle(96, 0, 16, 16),
                          new Rectangle(112, 0, 16, 16),
                          new Rectangle(128, 0, 16, 16)
                      }) },
                { 5, new TileType("Snow", false, new Rectangle(16, 16, 16, 16), Color.LightBlue,
                      new List<Rectangle>
                      {
                          new Rectangle(32, 16, 16, 16),
                          new Rectangle(48, 16, 16, 16),
                          new Rectangle(64, 16, 16, 16)
                      }) }
            };


            random = new Random(seed);
            GenerateWorld(); 
        }

        private void GenerateWorld()
        {
            for (int chunkY = 0; chunkY < totalChunksY; chunkY++)
            {
                for (int chunkX = 0; chunkX < totalChunksX; chunkX++)
                {
                    chunks[new Point(chunkX, chunkY)] = CreateChunk(chunkX, chunkY);
                }
            }
        }

        protected byte[,] CreateChunk(int chunkX, int chunkY)
        {
            byte[,] chunk = new byte[chunkSize, chunkSize];

            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    int worldX = chunkX * chunkSize + x;
                    int worldY = chunkY * chunkSize + y;
                    chunk[y, x] = GenerateTile(worldX, worldY);
                }
            }

            return chunk;
        }

        public abstract byte GenerateTile(int x, int y);

        public byte GetTile(int worldX, int worldY)
        {
            if (worldX < 0 || worldY < 0)
                return 0;

            int chunkX = worldX / chunkSize;
            int chunkY = worldY / chunkSize;

            if (!chunks.ContainsKey(new Point(chunkX, chunkY)))
            {
                return 0;
            }

            int localX = worldX % chunkSize;
            int localY = worldY % chunkSize;
            return chunks[new Point(chunkX, chunkY)][localY, localX];
        }

        public Point GetChunkCoordinates(int worldX, int worldY)
        {
            int chunkX = worldX / (chunkSize * 48); 
            int chunkY = worldY / (chunkSize * 48);
            return new Point(chunkX, chunkY);
        }
    }

}


