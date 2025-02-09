

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace CompSci_NEA.Tilemap
{
    public class TileMapCollisions : BaseTileMap
    {
        // Constructor with total chunks parameters [NEW]
        public TileMapCollisions(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY)
            : base(graphicsDevice, totalChunksX, totalChunksY) { }

        public override byte GenerateTile(int x, int y)
        {
            Random rand = new Random();
            return (rand.NextDouble() > 0.8) ? (byte)2 : (byte)1; // 2 = Water, 1 = Grass
        }

        public bool CheckCollision(Rectangle playerRect)
        {
            int tileX = playerRect.X / 48;
            int tileY = playerRect.Y / 48;
            return GetTile(tileX, tileY) == 2; // Check for water tile
        }
    }
}
