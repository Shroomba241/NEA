using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.GUI
{
    public class InputBox
    {
        private SpriteFont font;
        private string text;
        private Text inputText;
        private Vector2 position;
        private Rectangle bounds;
        private Color color;
        public bool isActive;
        private Texture2D backgroundTexture;

        private Dictionary<Keys, double> keyTimers;
        private const double KeyRepeatDelay = 250;

        private bool wasMousePressed = false;
        private bool obscured;
        private int limit;

        public Action OnClickAction;


        public InputBox(GraphicsDevice graphicsDevice, SpriteFont font, Vector2 position, int width, int height, bool obscured, int limit)
        {
            this.font = font;
            this.position = position;
            this.bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            this.color = Color.Red;
            this.text = string.Empty;
            this.isActive = false;
            this.obscured = obscured;
            this.limit = limit;

            keyTimers = new Dictionary<Keys, double>();

            backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            backgroundTexture.SetData(new[] { Color.White });

            inputText = new Text(font, text, position + new Vector2(20, 20), Color.White, 3.0f);
            this.limit = limit;
        }

        public void Update(GameTime gameTime)
        {
            double elapsedTime = gameTime.ElapsedGameTime.TotalMilliseconds;
            if (isActive)
            {
                KeyboardState state = Keyboard.GetState();
                foreach (Keys key in state.GetPressedKeys())
                {
                    if (!keyTimers.ContainsKey(key))
                    {
                        keyTimers[key] = 0; 
                    }

                    if (keyTimers[key] <= 0)
                    {
                        HandleKeyPress(key);
                        keyTimers[key] = KeyRepeatDelay;
                    }
                }

                foreach (Keys key in keyTimers.Keys.ToList())
                {
                    if (state.IsKeyDown(key))
                    {
                        keyTimers[key] -= elapsedTime;
                    }
                    else
                    {
                        keyTimers.Remove(key); 
                    }
                }

                if (obscured)
                {
                    inputText.UpdateContent(new string('*', text?.Length ?? 0));
                }
                else { inputText.UpdateContent(text); }

                color = Color.DarkGray;
            }
            else color = Color.Gray;

            MouseState mouseState = Mouse.GetState();
            bool isMousePressed = mouseState.LeftButton == ButtonState.Pressed;

            if (bounds.Contains(mouseState.X, mouseState.Y) && isMousePressed && !wasMousePressed)
            {
                isActive = !isActive;
                OnClick();
            }

            wasMousePressed = isMousePressed;

            
        }

        private void HandleKeyPress(Keys key)
        {
            KeyboardState state = Keyboard.GetState();
            bool isShiftPressed = state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);
            if (key == Keys.Back && text.Length > 0)
            {
                text = text.Remove(text.Length - 1);
            }
            else if (key >= Keys.A && key <= Keys.Z && text.Length <= limit)
            {
                text += isShiftPressed ? key.ToString() : key.ToString().ToLower();
            }
            else if (key >= Keys.D0 && key <= Keys.D9 && text.Length <= limit)
            {
                text += (key - Keys.D0).ToString();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, bounds, color);
            inputText.Draw(spriteBatch);
        }

        public void Move(Vector2 newPosition)
        {
            bounds = new Rectangle((int)newPosition.X, (int)newPosition.Y, bounds.Width, bounds.Height);
            inputText.position = newPosition + new Vector2(20, 20);
        }

        public string GetText()
        {
            return text;
        }

        private void OnClick()
        {
            OnClickAction?.Invoke();
        }
    }
}
