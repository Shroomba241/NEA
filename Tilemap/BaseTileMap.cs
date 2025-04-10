using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;


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
                { 1, new TileType("GrassTEST", false, Color.DarkGreen,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(0, 0, 16, 16), 1)
                      }) },
                { 2, new TileType("WaterTEST", true, Color.DodgerBlue,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(0, 16, 16, 16), 1)
                      }) },
                { 3, new TileType("Grass", false, Color.Green,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(32, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(48, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(64, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(0, 0, 16, 16), 1)
                      }) },
                { 4, new TileType("Dirt", false, Color.SaddleBrown,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(96, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(112, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(128, 0, 16, 16), 1)
                      }) },
                { 5, new TileType("Snow", false, Color.LightBlue,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(32, 16, 16, 16), 1),
                          new WeightedVariant(new Rectangle(48, 16, 16, 16), 1),
                          new WeightedVariant(new Rectangle(64, 16, 16, 16), 1)
                      }) },
                { 6, new TileType("Beach", false, Color.Yellow,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(32, 32, 16, 16), 1)
                      }) },
                { 7, new TileType("Plains", false, Color.Green,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(32, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(48, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(64, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(0, 0, 16, 16), 3)
                      }) },
                { 8, new TileType("ColourfulPlains", false, Color.LimeGreen,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(144, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(160, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(176, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(192, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(80, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(96, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(112, 0, 16, 16), 1),
                          new WeightedVariant(new Rectangle(128, 0, 16, 16), 1)
                      }) },
                { 9, new TileType("Snow", false, Color.LightBlue,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(32, 16, 16, 16), 1),
                          new WeightedVariant(new Rectangle(48, 16, 16, 16), 1),
                          new WeightedVariant(new Rectangle(64, 16, 16, 16), 1)
                      }) },
                { 10, new TileType("Wetlands", false, Color.Brown,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(16, 16, 16, 16), 1)
                      }) },
                { 11, new TileType("Savanna", false, Color.YellowGreen,
                      new List<WeightedVariant>
                      {
                          new WeightedVariant(new Rectangle(48, 32, 16, 16), 16),
                          new WeightedVariant(new Rectangle(80, 32, 16, 16), 3),
                          new WeightedVariant(new Rectangle(96, 32, 16, 16), 16),
                          new WeightedVariant(new Rectangle(112, 32, 16, 16), 16),
                          new WeightedVariant(new Rectangle(64, 32, 16, 16), 2),
                          new WeightedVariant(new Rectangle(128, 32, 16, 16), 1)
                      }) },
                { 12, new TileType("HauntedSands", false, Color.Lavender,
                    new List<WeightedVariant>
                    {
                        new WeightedVariant(new Rectangle(144, 16, 16, 16), 1),
                        new WeightedVariant(new Rectangle(160, 16, 16, 16), 1),
                        new WeightedVariant(new Rectangle(176, 16, 16, 16), 1),
                        new WeightedVariant(new Rectangle(192, 16, 16, 16), 1)
                    })},
                { 13, new TileType("DarkerPlains", false, Color.DarkGreen,
                    new List<WeightedVariant>
                    {
                        new WeightedVariant(new Rectangle(144, 32, 16, 16), 1),
                        new WeightedVariant(new Rectangle(160, 32, 16, 16), 1),
                        new WeightedVariant(new Rectangle(176, 32, 16, 16), 1),
                        new WeightedVariant(new Rectangle(192, 32, 16, 16), 1)
                    }) },
                { 14, new TileType("SpookyForest", false, Color.Gray, 
                    new List<WeightedVariant>
                    {
                        new WeightedVariant(new Rectangle(32, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(48, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(64, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(16, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(80, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(96, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(112, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(128, 48, 16, 16), 1)
                    }) },
                { 15, new TileType("ScarySnow", false, Color.LightSteelBlue, 
                    new List<WeightedVariant> 
                    {
                        new WeightedVariant(new Rectangle(144, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(160, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(176, 48, 16, 16), 1),
                        new WeightedVariant(new Rectangle(192, 48, 16, 16), 1)
                    })},
                { 16, new TileType("ScorchedLands", false, Color.DarkRed,
                    new List<WeightedVariant>
                    {
                        new WeightedVariant(new Rectangle(32, 64, 16, 16), 1),
                        new WeightedVariant(new Rectangle(48, 64, 16, 16), 1),
                        new WeightedVariant(new Rectangle(64, 64, 16, 16), 1),
                        new WeightedVariant(new Rectangle(16, 64, 16, 16), 1),
                        new WeightedVariant(new Rectangle(80, 64, 16, 16), 1),
                        new WeightedVariant(new Rectangle(96, 64, 16, 16), 1)
                    }) }
            };




            random = new Random(seed);
            //GenerateWorld(); 
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

        public void SetTile(int worldX, int worldY, byte newTile)
        {
            if (worldX < 0 || worldY < 0)
                return;

            int chunkX = worldX / chunkSize;
            int chunkY = worldY / chunkSize;
            Point key = new Point(chunkX, chunkY);

            if (!chunks.ContainsKey(key))
                chunks[key] = CreateChunk(chunkX, chunkY);

            int localX = worldX % chunkSize;
            int localY = worldY % chunkSize;
            chunks[key][localY, localX] = newTile;
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
            Point key = new Point(chunkX, chunkY);

            if (!chunks.ContainsKey(key))
            {
                //lazily generate this chunk as needed
                chunks[key] = CreateChunk(chunkX, chunkY);
            }

            int localX = worldX % chunkSize;
            int localY = worldY % chunkSize;
            return chunks[key][localY, localX];
        }

        public Point GetChunkCoordinates(int worldX, int worldY)
        {
            int chunkX = worldX / (chunkSize * 48); 
            int chunkY = worldY / (chunkSize * 48);
            return new Point(chunkX, chunkY);
        }
    }

}


