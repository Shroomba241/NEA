using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace CompSci_NEA.Tilemap
{
    public class TileMapVisual : BaseTileMap
    {
        public TileMapVisual(GraphicsDevice graphicsDevice, int rows, int cols)
            : base(graphicsDevice, rows, cols) { }

        public override void GenerateTiles(GraphicsDevice graphicsDevice)
        {
            Random rand = new Random();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Vector2 position = new Vector2(x * Tile.TileSize, y * Tile.TileSize);
                    Color tileColor = new Color(rand.Next(255), rand.Next(255), rand.Next(255));
                    tiles[y, x] = new Tile(graphicsDevice, position, tileColor, false); // Non-solid for visual map
                }
            }
        }
    }
}