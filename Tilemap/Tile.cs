using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.Tilemap
{
    public class Tile
    {
        public Vector2 Position { get; private set; }
        public Texture2D Texture { get; private set; }
        public static int TileSize = 48; // Size of each tile
        public bool IsSolid = false;

        public Tile(GraphicsDevice graphicsDevice, Vector2 position, Color color, bool isSolid)
        {
            Position = position;
            IsSolid = isSolid; // Set collision flag
            CreateTexture(graphicsDevice, color);
        }

        private void CreateTexture(GraphicsDevice graphicsDevice, Color color)
        {
            Texture = new Texture2D(graphicsDevice, 1, 1);
            Texture.SetData(new[] { color }); // Solid color texture
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, TileSize, TileSize), Color.White);
        }

        public Rectangle GetBoundingBox() // Add this method to get the bounding box for collision detection
        {
            return new Rectangle((int)Position.X, (int)Position.Y, TileSize, TileSize);
        }
    }
}
