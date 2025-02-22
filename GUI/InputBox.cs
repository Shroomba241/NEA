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
        private Text _inputText;
        private Vector2 position;
        private Rectangle bounds;
        private Color colour;
        public bool isActive;
        private Texture2D backgroundTexture;

        private bool _wasMousePressed = false;
        private bool obscured;
        private int limit;

        public Action OnClickAction;

        private double _keyRepeatTimer = 0;
        private const double _initialKeyDelay = 400; 
        private const double _keyRepeatRate = 50;   
        private Keys _lastKeyPressed = Keys.None;


        public InputBox(GraphicsDevice graphicsDevice, SpriteFont font, Vector2 position, int width, int height, bool obscured, int limit)
        {
            this.font = font;
            this.position = position;
            this.bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            this.colour = Color.Red;
            this.text = string.Empty;
            this.isActive = false;
            this.obscured = obscured;

            backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            backgroundTexture.SetData(new[] { Color.White });

            _inputText = new Text(font, text, position + new Vector2(20, 20), Color.White, 3.0f);
            this.limit = limit;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            double elapsedTime = gameTime.ElapsedGameTime.TotalMilliseconds;

            bool isMouseClicked = mouseState.LeftButton == ButtonState.Pressed;
            if (bounds.Contains(mouseState.X, mouseState.Y) && isMouseClicked && !_wasMousePressed)
            {
                isActive = true;
                OnClick();
            }
            else if (isMouseClicked && !bounds.Contains(mouseState.X, mouseState.Y))
            {
                isActive = false;
            }
            _wasMousePressed = isMouseClicked;

            if (isActive)
            {
                Keys[] pressedKeys = keyboardState.GetPressedKeys();
                Keys? keyToProcess = null;

                if (pressedKeys.Length > 0)
                {
                    Keys key = pressedKeys[0];

                    if (key != _lastKeyPressed)
                    {
                        _keyRepeatTimer = _initialKeyDelay; 
                        keyToProcess = key;
                    }
                    else if (_keyRepeatTimer <= 0)
                    {
                        _keyRepeatTimer = _keyRepeatRate; 
                        keyToProcess = key;
                    }

                    _lastKeyPressed = key;
                }
                else
                {
                    _lastKeyPressed = Keys.None;
                    _keyRepeatTimer = 0; 
                }

                if (_lastKeyPressed != Keys.None)
                {
                    _keyRepeatTimer -= elapsedTime;
                }

                if (keyToProcess.HasValue)
                {
                    HandleKeyPress(keyToProcess.Value);
                }

                _inputText.UpdateContent(obscured ? new string('*', text.Length) : text);
            }

            colour = isActive ? Color.DarkGray : Color.Gray;
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
            spriteBatch.Draw(backgroundTexture, bounds, colour);
            _inputText.Draw(spriteBatch);
        }

        public void Move(Vector2 newPosition)
        {
            bounds = new Rectangle((int)newPosition.X, (int)newPosition.Y, bounds.Width, bounds.Height);
            _inputText.position = newPosition + new Vector2(20, 20);
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
