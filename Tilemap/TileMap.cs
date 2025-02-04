using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.Tilemap
{
    public class TileMap
    {
        private Tile[,] tiles; // 2D Array of tiles
        private int rows, cols;

        public TileMap(GraphicsDevice graphicsDevice, int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            tiles = new Tile[rows, cols];

            GenerateTiles(graphicsDevice);
        }

        private void GenerateTiles(GraphicsDevice graphicsDevice)
        {
            Random rand = new Random();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Vector2 position = new Vector2(x * Tile.TileSize, y * Tile.TileSize);
                    Color tileColor = new Color(150, 50 + rand.Next(50), 150);
                    bool isSolid = rand.Next(2) == 0;
                    tiles[y, x] = new Tile(graphicsDevice, position, tileColor, false);
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

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var tile in tiles)
            {
                tile.Draw(spriteBatch);
            }
        }
    }
}
