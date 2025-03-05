using CompSci_NEA.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using CompSci_NEA.WorldGeneration.Structures;

namespace CompSci_NEA.Tilemap
{
    public class StructureTileMap : BaseTileMap
    {
        private List<Structure> _structures;
        public Shop Shop;

        public StructureTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY, int seed)
            : base(graphicsDevice, totalChunksX, totalChunksY, seed)
        {
            tileAtlas = TextureManager.ATLAS;
            _structures = new List<Structure>();

            // Create your structures:
            StoneBridge stoneBridge = new StoneBridge(tileAtlas, seed);
            stoneBridge.Generate(400 * 48, 400 * 48, 100);
            _structures.Add(stoneBridge);

            Shop = new Shop(tileAtlas, new Vector2(390 * 48, 400 * 48));
            _structures.Add(Shop);
        }

        public override byte GenerateTile(int x, int y)
        {
            return 0;
        }

        // Draw background layer (behind the player)
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
                    Rectangle chunkRect = new Rectangle(
                        currentChunkX * chunkSize * 48,
                        currentChunkY * chunkSize * 48,
                        chunkSize * 48,
                        chunkSize * 48);

                    // Draw the tilemap as before...

                    // Let each structure draw its background part if it intersects this chunk.
                    foreach (var structure in _structures)
                    {
                        structure.DrawBackgroundInRect(spriteBatch, chunkRect);
                    }
                }
            }
        }

        // Draw foreground layer (in front of the player)
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
                    Rectangle chunkRect = new Rectangle(
                        currentChunkX * chunkSize * 48,
                        currentChunkY * chunkSize * 48,
                        chunkSize * 48,
                        chunkSize * 48);

                    foreach (var structure in _structures)
                    {
                        structure.DrawForegroundInRect(spriteBatch, chunkRect);
                    }
                }
            }
        }

        // Gather all collision rectangles from all structures.
        public List<Rectangle> GetAllColliders()
        {
            List<Rectangle> colliders = new List<Rectangle>();
            foreach (var structure in _structures)
            {
                colliders.AddRange(structure.GetColliders());
            }
            return colliders;
        }
    }
}
