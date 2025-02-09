using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace CompSci_NEA.Tilemap
{
    public abstract class BaseTileMap
    {
        protected Dictionary<Point, byte[,]> chunks; // Dictionary to store all chunks
        protected Dictionary<byte, TileType> tileTypes;
        protected Texture2D tileAtlas;
        protected int chunkSize = 64; // Size of each chunk in tiles
        protected int totalChunksX; // Total chunks in X direction
        protected int totalChunksY; // Total chunks in Y direction

        // Constructor with total chunks parameters
        protected BaseTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY)
        {
            this.totalChunksX = totalChunksX;
            this.totalChunksY = totalChunksY;
            chunks = new Dictionary<Point, byte[,]>();
            tileTypes = new Dictionary<byte, TileType>
            {
                { 1, new TileType("Grass", false, new Rectangle(0, 0, 16, 16), Color.Green) },
                { 2, new TileType("Water", true, new Rectangle(0, 16, 16, 16), Color.DodgerBlue) },
                { 3, new TileType("Bridge", false, new Rectangle(0, 32, 128, 96), Color.Gray) }
            };

            GenerateWorld(); // Generate the entire world [NEW] 
        }

        // Method to generate the entire world in chunks [NEW]
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
            int chunkX = worldX / chunkSize;
            int chunkY = worldY / chunkSize;

            if (!chunks.ContainsKey(new Point(chunkX, chunkY)))
            {
                return 0; // Return an empty tile ID or handle appropriately
            }

            int localX = worldX % chunkSize;
            int localY = worldY % chunkSize;
            return chunks[new Point(chunkX, chunkY)][localY, localX]; // Return the tile from the chunk
        }

        public Point GetChunkCoordinates(int worldX, int worldY)
        {
            int chunkX = worldX / (chunkSize * 48); // Adjust for chunk size
            int chunkY = worldY / (chunkSize * 48);
            return new Point(chunkX, chunkY);
        }
    }

}


