using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace CompSci_NEA.Tilemap
{
    public abstract class BaseTileMap
    {
        public Tile[,] tiles; // 2D Array of tiles
        protected int rows, cols;

        protected BaseTileMap(GraphicsDevice graphicsDevice, int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            tiles = new Tile[rows, cols];


        }

        public abstract void GenerateTiles(GraphicsDevice graphicsDevice);

        public Tile GetTile(int x, int y)
        {
            return tiles[y, x];
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach (var tile in tiles)
            {
                tile.Draw(spriteBatch);
            }
        }

        public void FillSolidArea(GraphicsDevice graphicsDevice, int startX, int startY, int endX, int endY, Color color)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (x >= 0 && x < cols && y >= 0 && y < rows) // Ensure within bounds
                    {
                        if (tiles[y, x] == null) // Initialize tile if missing
                        {
                            Vector2 position = new Vector2(x * Tile.TileSize, y * Tile.TileSize);
                            tiles[y, x] = new Tile(graphicsDevice, position, Color.Transparent, false);
                        }

                        tiles[y, x] = new Tile(graphicsDevice, tiles[y, x].Position, color, true); // Set solid tile
                    }
                }
            }
        }

    }
}
