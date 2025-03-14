using CompSci_NEA.Scenes;
using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CompSci_NEA.WorldGeneration.Structures
{
    public class Shop : Structure
    {
        private Texture2D _tileAtlas;
        private Vector2 _position;
        private Rectangle _textureRegion;
        private const float SCALE = 3.0f;

        public Shop(Texture2D tileAtlas, Vector2 position)
        {
            _tileAtlas = tileAtlas;
            _position = position;
            _textureRegion = new Rectangle(0, 272, 48, 32);
        }

        public void Interact(Main game, GameSave save)
        {
            game.StartMiniGame(Core.SubGameState.ShopMenu, save);
        }

        public Vector2 GetInteractionPoint()
        {
            return new Vector2(_position.X + (_textureRegion.Width * SCALE) / 2, _position.Y + (_textureRegion.Height * SCALE) / 2);
        }

        public void DrawBackgroundInRect(SpriteBatch spriteBatch, Rectangle chunkRect)
        {
            Rectangle shopRect = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                (int)(_textureRegion.Width * SCALE),
                (int)(_textureRegion.Height * SCALE));

            if (shopRect.Intersects(chunkRect))
            {
                spriteBatch.Draw(_tileAtlas, _position, _textureRegion, Color.White, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
            }
        }

        public void DrawForegroundInRect(SpriteBatch spriteBatch, Rectangle rect)
        {
           
        }

        public IEnumerable<Rectangle> GetColliders()
        {
            Rectangle shopRect = new Rectangle((int)_position.X, (int)(_position.Y + _textureRegion.Height/2 * SCALE),
                (int)(_textureRegion.Width * SCALE), (int)(_textureRegion.Height * SCALE/2));
            yield return shopRect;
        }
    }
}
