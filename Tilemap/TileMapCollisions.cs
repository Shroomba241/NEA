using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace CompSci_NEA.Tilemap
{
    public class TileMapCollisions : BaseTileMap
    {
        public TileMapCollisions(GraphicsDevice graphicsDevice, int rows, int cols)
            : base(graphicsDevice, rows, cols) { }

        public override void GenerateTiles(GraphicsDevice graphicsDevice)
        {
            Random rand = new Random();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Vector2 position = new Vector2(x * Tile.TileSize, y * Tile.TileSize);
                    tiles[y, x] = new Tile(graphicsDevice, position, Color.Transparent, true); // Use transparent color
                }
            }
        }

        public bool CheckCollision(Rectangle playerRect)
        {
            foreach (var tile in tiles)
            {
                if (tile.IsSolid && tile.GetBoundingBox().Intersects(playerRect))
                {
                    return true; // Collision detected
                }
            }
            return false; // No collision
        }
    }
}
