using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Action OnClickAction; 

        public Button(GraphicsDevice graphicsDevice, SpriteFont font, string text, Vector2 position, int width, int height)
        {
            bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            normalColor = Color.White;
            hoverColor = Color.Gray;
            currentColor = normalColor;

            backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            backgroundTexture.SetData(new[] { Color.DarkSlateGray });

            buttonText = new Text(font, text, position + new Vector2(20, 20), Color.White, 3.0f);
        }

        public void Update()
        {
            MouseState mouseState = Mouse.GetState();
            if (bounds.Contains(mouseState.X, mouseState.Y))
            {
                currentColor = hoverColor;
                IsHovered = true;
                if(mouseState.LeftButton == ButtonState.Pressed) { isHeld = true; }
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
            buttonText.position = newPosition + new Vector2(20, 20);
        }

        private void OnClick()
        {
            OnClickAction?.Invoke();
        }
    }
}
