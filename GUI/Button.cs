using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CompSci_NEA.GUI
{
    public class Button
    {
        private Rectangle bounds;
        private Color normalColour;
        private Color hoverColour;
        private Color _currentColour;
        private Texture2D _backgroundTexture;
        private Text _buttonText;
        public bool IsHovered = false;
        private bool _isHeld;
        private Vector2 positionOffset;
        public Action OnClickAction;

        
        public Button(GraphicsDevice graphicsDevice, SpriteFont font, string text, Vector2 position, int width, int height)
            : this(graphicsDevice, font, text, position, width, height, 3.0f, new Vector2(20, 20))
        {
        }

        public Button(GraphicsDevice graphicsDevice, SpriteFont font, string text, Vector2 position, int width, int height, float textScale, Vector2 positionOffset)
        {
            bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            normalColour = Color.White;
            hoverColour = Color.Gray;
            _currentColour = normalColour;
            this.positionOffset = positionOffset;

            _backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            _backgroundTexture.SetData(new[] { Color.DarkSlateGray });

            // Create the Text object with the specified text scale.
            _buttonText = new Text(font, text, position + positionOffset, Color.White, textScale);
        }

        public void Update()
        {
            MouseState mouseState = Mouse.GetState();
            if (bounds.Contains(mouseState.X, mouseState.Y))
            {
                _currentColour = hoverColour;
                IsHovered = true;
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    _isHeld = true;
                }
                if (mouseState.LeftButton == ButtonState.Released && _isHeld)
                {
                    OnClick();
                    _isHeld = false;
                }
            }
            else
            {
                _currentColour = normalColour;
                IsHovered = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_backgroundTexture, bounds, _currentColour);
            _buttonText.Draw(spriteBatch);
        }

        public void SetText(string newText)
        {
            _buttonText.UpdateContent(newText);
        }

        public void Move(Vector2 newPosition)
        {
            bounds = new Rectangle((int)newPosition.X, (int)newPosition.Y, bounds.Width, bounds.Height);
            _buttonText.position = newPosition + positionOffset;
        }

        private void OnClick()
        {
            OnClickAction?.Invoke();
        }
    }
}
