using CompSci_NEA.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CompSci_NEA.Tilemap.TileMapVisual;

namespace CompSci_NEA.Tilemap
{
    public class StructureTileMap : BaseTileMap
    {
        public override byte GenerateTile(int x, int y)
        {
            if (x%8 == 0  && y == 0) return 3;
            return 0;
        }

        // Dictionary to store structures with their positions as keys
        private Dictionary<Point, byte> structures;

        // Constructor
        public StructureTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY)
            : base(graphicsDevice, totalChunksX, totalChunksY)
        {
            tileAtlas = TextureManager.ATLAS;
            
            //InitializeStructures(); // Populate with test structures
        }

        // Method to initialize test structures
        private void InitializeStructures()
        {
            // Adding a test structure at position (0, 0)
            //structures.Add(, 3); // Example structure name
        }

        // Draw method to render structures
        public void DrawStructures(SpriteBatch spriteBatch, Entities.Player player)
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

                    // Draw structures if they are within the current chunk
                    DrawStructuresInChunk(spriteBatch, currentChunkX, currentChunkY);
                }
            }
        }

        // Helper method to draw structures within a specific chunk
        private void DrawStructuresInChunk(SpriteBatch spriteBatch, int chunkX, int chunkY)
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
                    //sourceRect.Inflate(-1, -1);
                    //Vector2 destinationPosition = new Vector2((int)position.X, (int)position.Y);
                    float scale = 3.0f; // Scale factor of 3x

                    spriteBatch.Draw(tileAtlas, position, sourceRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }


            /*// Check if the chunk exists
            if (!chunks.ContainsKey(new Point(chunkX, chunkY)))
            {
                return; // Skip if the chunk does not exist
            }

            // Loop through all the structures and draw them if they are in the current chunk
            foreach (var structure in structures)
            {
                Point structurePosition = structure.Key;

                // Calculate local chunk coordinates
                int localChunkX = structurePosition.X / (48 * chunkSize);
                int localChunkY = structurePosition.Y / (48 * chunkSize);

                if (localChunkX == chunkX && localChunkY == chunkY)
                {
                    // Draw the structure at the calculated position
                    DrawStructure(spriteBatch, structurePosition, structure.Value, chunkX, chunkY);
                }
            }*/
        }

        // Method to draw an individual structure
        private void DrawStructure(SpriteBatch spriteBatch, Point pointPosition, int structureType, int chunkX, int chunkY)
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
                    Rectangle destinationRect = new Rectangle((int)pointPosition.X, (int)pointPosition.Y, 48, 48);

                    spriteBatch.Draw(tileAtlas, destinationRect, sourceRect, Color.White);

                    spriteBatch.Draw(tileAtlas, new Vector2(pointPosition.X, pointPosition.Y), null, Color.White, 0f, Vector2.Zero, 3, SpriteEffects.None, 0f);
                    spriteBatch.Draw(tileAtlas, new Vector2(48 * 8, pointPosition.Y), null, Color.White, 0f, Vector2.Zero, 3, SpriteEffects.None, 0f); 
                }
            }
        }
        /*// You can customize the appearance of the structure here based on type
        Rectangle destinationRect = new Rectangle(position.X, position.Y, 48, 48);
        Color structureColor = Color.Red; // Example color for the structure

        // Optional: Use different textures or colors based on the structure type
        // For now, we just draw a simple rectangle or use a texture from the tileAtlas
        //spriteBatch.Draw(tileAtlas, Vector2.Zero, structureColor, 3f);
        TileType tileType = tileTypes[tileID];
        Rectangle sourceRect = tileType.TextureRegion;
        sourceRect.Inflate(-1, -1);
        Rectangle destinationRect = new Rectangle((int)position.X, (int)position.Y, 48, 48);

        spriteBatch.Draw(tileAtlas, destinationRect, sourceRect, Color.White);

        spriteBatch.Draw(tileAtlas, new Vector2(position.X, position.Y), null, Color.White, 0f, Vector2.Zero, 3, SpriteEffects.None, 0f);
        spriteBatch.Draw(tileAtlas, new Vector2(48*8, position.Y), null, Color.White, 0f, Vector2.Zero, 3, SpriteEffects.None, 0f);*/
    }
    
}

