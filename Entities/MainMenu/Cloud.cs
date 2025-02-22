using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CompSci_NEA
{
    internal class Cloud
    {
        private Texture2D textureAtlas;
        private Rectangle _sourceRect;
        private Vector2 _position;
        private Vector2 _velocity;
        private float _opacity;
        private Random _random = new Random();
        private int screenWidth;

        public bool IsOffscreen => _position.X < -(_sourceRect.Width + 510);

        public Cloud(Texture2D textureAtlas, int screenWidth)
        {
            this.textureAtlas = textureAtlas;
            this.screenWidth = screenWidth;

            int cloudIndex = _random.Next(0, 6);
            int col = cloudIndex % 3; 
            int row = cloudIndex / 3; 
            _sourceRect = new Rectangle(col * 170, row * 64, 170, 64);
            _position = new Vector2(screenWidth, _random.Next(0, 450));
            _velocity = new Vector2(-(float)_random.NextDouble()- 0.5f, 0);
            _opacity = (float)(_random.NextDouble() *0.5 + 0.4);
        }

        public void Update(GameTime gameTime)
        {
            _position += _velocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textureAtlas, _position, _sourceRect, Color.White *_opacity,
                 0f, Vector2.Zero, 3.0f, SpriteEffects.None, 0f);
        }
    }
}
