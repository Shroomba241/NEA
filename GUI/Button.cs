using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CompSci_NEA.GUI
{
    public class Button
    {
        private Rectangle bounds;
        private Color normalColor;
        private Color hoverColor;
        private Color currentColor;
        private Texture2D backgroundTexture;
        private Text buttonText;
        public bool IsHovered = false;
        private bool isHeld;
        private Vector2 positionOffset;
        public Action OnClickAction;

        // Constructor #1: Uses a default text scale of 3.0f.
        public Button(GraphicsDevice graphicsDevice, SpriteFont font, string text, Vector2 position, int width, int height)
            : this(graphicsDevice, font, text, position, width, height, 3.0f, new Vector2(20, 20))
        {
        }

        // Constructor #2: Accepts a custom text scale.
        public Button(GraphicsDevice graphicsDevice, SpriteFont font, string text, Vector2 position, int width, int height, float textScale, Vector2 positionOffset)
        {
            bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            normalColor = Color.White;
            hoverColor = Color.Gray;
            currentColor = normalColor;
            this.positionOffset = positionOffset;

            backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            backgroundTexture.SetData(new[] { Color.DarkSlateGray });

            // Create the Text object with the specified text scale.
            buttonText = new Text(font, text, position + positionOffset, Color.White, textScale);
        }

        public void Update()
        {
            MouseState mouseState = Mouse.GetState();
            if (bounds.Contains(mouseState.X, mouseState.Y))
            {
                currentColor = hoverColor;
                IsHovered = true;
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    isHeld = true;
                }
                if (mouseState.LeftButton == ButtonState.Released && isHeld)
                {
                    OnClick();
                    isHeld = false;
                }
            }
            else
            {
                currentColor = normalColor;
                IsHovered = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, bounds, currentColor);
            buttonText.Draw(spriteBatch);
        }

        public void Move(Vector2 newPosition)
        {
            bounds = new Rectangle((int)newPosition.X, (int)newPosition.Y, bounds.Width, bounds.Height);
            buttonText.position = newPosition + positionOffset;
        }

        private void OnClick()
        {
            OnClickAction?.Invoke();
        }
    }
}
