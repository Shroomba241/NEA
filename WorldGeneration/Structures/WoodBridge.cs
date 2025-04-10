using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CompSci_NEA.WorldGeneration.Structures
{
    public class WoodBridge : Structure
    {
        private Texture2D _tileAtlas;
        public Vector2 Position;
        private readonly Rectangle _textureRegion = new Rectangle(64, 272, 192, 48);
        private const float SCALE = 3.0f;

        public WoodBridge(Texture2D tileAtlas, Vector2 position)
        {
            _tileAtlas = tileAtlas;
            Position = position;
        }

        public void DrawBackgroundInRect(SpriteBatch spriteBatch, Rectangle visibleRect)
        {
            Rectangle bridgeRect = new Rectangle((int)Position.X, (int)Position.Y,  (int)(_textureRegion.Width * SCALE), (int)(_textureRegion.Height * SCALE));

            if (bridgeRect.Intersects(visibleRect))
            {
                spriteBatch.Draw(_tileAtlas, Position, _textureRegion, Color.White, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
            }
        }

        public void DrawForegroundInRect(SpriteBatch spriteBatch, Rectangle visibleRect)
        {
            
        }

        public IEnumerable<Rectangle> GetColliders()
        {
            yield break;
        }
    }
}
