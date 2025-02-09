

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using CompSci_NEA.WorldGeneration;
using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using System.Collections.Generic;

namespace CompSci_NEA.Tilemap
{
    public class TileMapVisual : BaseTileMap
    {
        public TileMapVisual(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY)
            : base(graphicsDevice, totalChunksX, totalChunksY)
        {
            tileAtlas = TextureManager.ATLAS;
        }

        public override byte GenerateTile(int x, int y)
        {

            float centerX = (totalChunksX * chunkSize) * 0.5f;
            float centerY = (totalChunksY * chunkSize) * 0.5f;

            // Commented out oval shape calculations

            float ovalWidthScale = 0.35f;
            float ovalHeightScale = 0.25f;
            float normX = (x - centerX) / (totalChunksX * chunkSize * ovalWidthScale);
            float normY = (y - centerY) / (totalChunksY * chunkSize * ovalHeightScale);
            float baseShape = (normX * normX) + (normY * normY);
            //baseShape = 0;


            /*float triangleXOffset = 0.65f;
            float triangleBaseWidth = 0.15f;
            float triangleHeightScale = 0.25f;

            float triangleHeightFactor = 0.35f;
            float noiseInfluence = NoiseGenerator.Generate(x / 50.0f, y / 50.0f) * 0.1f; // Adjust frequency and amplitude as needed

            float triangleX = (x - (centerX + totalChunksX * chunkSize * triangleXOffset)) / (totalChunksX * chunkSize * triangleBaseWidth);
            float edgeNoise = NoiseGenerator.Generate(x / 50.0f, y / 50.0f) * 0.2f; // Adjust noise frequency and amplitude
            float triangleY = (y - centerY + edgeNoise) / (totalChunksY * chunkSize * triangleHeightScale);

            bool inTriangle = (triangleX < 0) &&
                              (triangleY > triangleHeightFactor * triangleX + edgeNoise) &&
                              (triangleY < -triangleHeightFactor * triangleX + edgeNoise);

            if (inTriangle)
            {
                baseShape -= (0.3f + edgeNoise); // Modify based on the triangle effect and noise
            }*/
            // Commented out noise calculations

            float baseNoise = NoiseGenerator.Generate(x / 200f, y / 200f) * 0.3f;
            baseShape -= baseNoise;

            // Coastline Noise Layers
            float largeScaleNoise = NoiseGenerator.Generate(x / 150f, y / 150f, 3, 50f, 0.5f, 2.5f); // Defines major land chunks
            float mediumNoise = NoiseGenerator.Generate(x / 40f, y / 40f, 4, 20f, 0.6f, 2.2f); // Bays, peninsulas
            float smallNoise = NoiseGenerator.Generate(x / 10f, y / 10f, 5, 10f, 0.8f, 2.0f); // Tiny irregularities

            float coastlineDistortion = (largeScaleNoise * 0.5f) + (mediumNoise * 0.4f) + (smallNoise * 0.4f);

            float finalShape = baseShape - coastlineDistortion;

            float landThreshold = 0.9f - (largeScaleNoise * 0.1f) + (mediumNoise * 0.05f);

            if (finalShape < landThreshold)
            {
                return 1; // Land
            }
            else
            {
                return 2; // Water
            }

        }



        public void Draw(SpriteBatch spriteBatch, Entities.Player player)
        {
            // Determine the chunk coordinates that the player is in
            int chunkX = (int)(player.Position.X / (48 * chunkSize)); // Calculate chunk X based on player's position
            int chunkY = (int)(player.Position.Y / (48 * chunkSize)); // Calculate chunk Y based on player's position

            // Loop through the 3x3 grid of chunks centered around the player's chunk
            for (int offsetY = -2; offsetY <= 2; offsetY++)
            {
                for (int offsetX = -2; offsetX <= 2; offsetX++)
                {
                    int currentChunkX = chunkX + offsetX; // Calculate current chunk X
                    int currentChunkY = chunkY + offsetY; // Calculate current chunk Y
                    DrawChunk(spriteBatch, currentChunkX, currentChunkY); // Draw the chunk
                }
            }
        }

        private void DrawChunk(SpriteBatch spriteBatch, int chunkX, int chunkY)
        {
            if (!chunks.ContainsKey(new Point(chunkX, chunkY)))
            {
                return; // Skip if the chunk does not exist
            }

            byte[,] chunk = chunks[new Point(chunkX, chunkY)];

            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    byte tileID = chunk[y, x];
                    if (!tileTypes.ContainsKey(tileID)) continue;

                    TileType tileType = tileTypes[tileID];
                    Vector2 position = new Vector2(
                        (chunkX * chunkSize + x) * 48,
                        (chunkY * chunkSize + y) * 48
                    );

                    Rectangle sourceRect = tileType.TextureRegion;
                    sourceRect.Inflate(-1, -1);
                    Rectangle destinationRect = new Rectangle((int)position.X, (int)position.Y, 48, 48);

                    spriteBatch.Draw(tileAtlas, destinationRect, sourceRect, Color.White);
                }
            }
        }

        public Texture2D GenerateMapTexture(GraphicsDevice graphicsDevice, int width, int height)
        {
            Texture2D mapTexture = new Texture2D(graphicsDevice, width, height);
            Color[] mapData = new Color[width * height];

            // Fill mapData with colors based on the tile types
            for (int chunkY = 0; chunkY < chunks.Count; chunkY++)
            {
                for (int chunkX = 0; chunkX < chunks.Count; chunkX++)
                {
                    if (!chunks.ContainsKey(new Point(chunkX, chunkY))) continue;

                    byte[,] chunk = chunks[new Point(chunkX, chunkY)];

                    for (int y = 0; y < chunkSize; y++)
                    {
                        for (int x = 0; x < chunkSize; x++)
                        {
                            byte tileID = chunk[y, x];
                            Color tileColor;

                            if (tileTypes.ContainsKey(tileID))
                            {
                                tileColor = tileTypes[tileID].Color;
                            }
                            else
                            {
                                tileColor = Color.Red;
                            }

                            // Calculate position in the map texture
                            int mapX = (chunkX * chunkSize + x);
                            int mapY = (chunkY * chunkSize + y);

                            if (mapX < width && mapY < height)
                            {
                                mapData[mapY * width + mapX] = tileColor; // Set the color
                            }
                        }
                    }
                }
            }
            Console.WriteLine("map generated");
            mapTexture.SetData(mapData);
            return mapTexture; // Return the generated texture
        }

        /*public class MultiTileStructure
        {
            public int Width, Height;  // Structure size in tiles
            public TileTemplate[,] Layout;  // 2D array storing tile templates
        }

        public class TileTemplate
        {
            public Point AtlasCoords;  // (X, Y) position in tile atlas
            public List<Point> Variants; // Possible random alternative textures
        }

        // Example: Bridge Structure
        public static Dictionary<string, MultiTileStructure> StructureTemplates = new Dictionary<string, MultiTileStructure>()
        {
            {
                "Bridge", new MultiTileStructure
                {
                    Width = 8,
                    Height = 6,
                    Layout = new TileTemplate[,]
                    {
                        { new TileTemplate { AtlasCoords = new Point(0, 32), Variants = new List<Point>{ new Point(16, 32), new Point(32, 32) } }, new TileTemplate { AtlasCoords = new Point(16, 32) }, new TileTemplate { AtlasCoords = new Point(32, 32) }, new TileTemplate { AtlasCoords = new Point(48, 32) }, new TileTemplate { AtlasCoords = new Point(64, 32) }, new TileTemplate { AtlasCoords = new Point(80, 32) }, new TileTemplate { AtlasCoords = new Point(96, 32) }, new TileTemplate { AtlasCoords = new Point(112, 32) } },
                        { new TileTemplate { AtlasCoords = new Point(0, 48) }, new TileTemplate { AtlasCoords = new Point(16, 48), Variants = new List<Point>{ new Point(32, 48) } }, new TileTemplate { AtlasCoords = new Point(32, 48) }, new TileTemplate { AtlasCoords = new Point(48, 48) }, new TileTemplate { AtlasCoords = new Point(64, 48) }, new TileTemplate { AtlasCoords = new Point(80, 48) }, new TileTemplate { AtlasCoords = new Point(96, 48) }, new TileTemplate { AtlasCoords = new Point(112, 48) } },
                        { new TileTemplate { AtlasCoords = new Point(0, 64) }, new TileTemplate { AtlasCoords = new Point(16, 64) }, new TileTemplate { AtlasCoords = new Point(32, 64) }, new TileTemplate { AtlasCoords = new Point(48, 64) }, new TileTemplate { AtlasCoords = new Point(64, 64) }, new TileTemplate { AtlasCoords = new Point(80, 64) }, new TileTemplate { AtlasCoords = new Point(96, 64) }, new TileTemplate { AtlasCoords = new Point(112, 64) } },
                        { new TileTemplate { AtlasCoords = new Point(0, 80) }, new TileTemplate { AtlasCoords = new Point(16, 80) }, new TileTemplate { AtlasCoords = new Point(32, 80) }, new TileTemplate { AtlasCoords = new Point(48, 80) }, new TileTemplate { AtlasCoords = new Point(64, 80) }, new TileTemplate { AtlasCoords = new Point(80, 80) }, new TileTemplate { AtlasCoords = new Point(96, 80) }, new TileTemplate { AtlasCoords = new Point(112, 80) } },
                        { new TileTemplate { AtlasCoords = new Point(0, 96) }, new TileTemplate { AtlasCoords = new Point(16, 96) }, new TileTemplate { AtlasCoords = new Point(32, 96) }, null, null, new TileTemplate { AtlasCoords = new Point(80, 96) }, new TileTemplate { AtlasCoords = new Point(96, 96) }, new TileTemplate { AtlasCoords = new Point(112, 96) } },
                        { new TileTemplate { AtlasCoords = new Point(0, 112) }, null, null, null, null, null, null, new TileTemplate { AtlasCoords = new Point(112, 112) } },
                    }
                }
            }
        };*/





    }
}
