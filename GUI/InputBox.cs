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

        private bool wasMousePressed = false;
        private bool obscured;
        private int limit;

        public Action OnClickAction;

        private double keyRepeatTimer = 0;
        private const double InitialKeyDelay = 400; 
        private const double KeyRepeatRate = 50;   
        private Keys lastKeyPressed = Keys.None;


        public InputBox(GraphicsDevice graphicsDevice, SpriteFont font, Vector2 position, int width, int height, bool obscured, int limit)
        {
            this.font = font;
            this.position = position;
            this.bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            this.color = Color.Red;
            this.text = string.Empty;
            this.isActive = false;
            this.obscured = obscured;

            backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            backgroundTexture.SetData(new[] { Color.White });

            inputText = new Text(font, text, position + new Vector2(20, 20), Color.White, 3.0f);
            this.limit = limit;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            double elapsedTime = gameTime.ElapsedGameTime.TotalMilliseconds;

            bool isMouseClicked = mouseState.LeftButton == ButtonState.Pressed;
            if (bounds.Contains(mouseState.X, mouseState.Y) && isMouseClicked && !wasMousePressed)
            {
                isActive = true;
                OnClick();
            }
            else if (isMouseClicked && !bounds.Contains(mouseState.X, mouseState.Y))
            {
                isActive = false;
            }
            wasMousePressed = isMouseClicked;

            if (isActive)
            {
                Keys[] pressedKeys = keyboardState.GetPressedKeys();
                Keys? keyToProcess = null;

                if (pressedKeys.Length > 0)
                {
                    Keys key = pressedKeys[0];

                    if (key != lastKeyPressed)
                    {
                        keyRepeatTimer = InitialKeyDelay; 
                        keyToProcess = key;
                    }
                    else if (keyRepeatTimer <= 0)
                    {
                        keyRepeatTimer = KeyRepeatRate; 
                        keyToProcess = key;
                    }

                    lastKeyPressed = key;
                }
                else
                {
                    lastKeyPressed = Keys.None;
                    keyRepeatTimer = 0; 
                }

                if (lastKeyPressed != Keys.None)
                {
                    keyRepeatTimer -= elapsedTime;
                }

                if (keyToProcess.HasValue)
                {
                    HandleKeyPress(keyToProcess.Value);
                }

                inputText.UpdateContent(obscured ? new string('*', text.Length) : text);
            }

            color = isActive ? Color.DarkGray : Color.Gray;
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

        public void SetText(string newText)
        {
            /*if (newText.Length > limit)
            {
                newText = newText.Substring(0, limit);
            }*/
            Console.WriteLine($"{text}  {newText}");
            text = newText;
        }

        private void OnClick()
        {
            OnClickAction?.Invoke();
        }
    }
}
