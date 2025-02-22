using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CompSci_NEA.Tilemap;
using System;

namespace CompSci_NEA.Entities
{
    public class Player
    {
        public Vector2 Position { get; private set; }
        private Vector2 _velocity;

        private float _acceleration = 8f;
        private float _deceleration = 12f;
        private float _maxSpeed = 10f;

        private int _width = 50;
        private int _height = 100;

        private Texture2D _texture;

        public Player(GraphicsDevice graphicsDevice, Vector2 startPosition)
        {
            Position = startPosition;
            CreateTexture(graphicsDevice);
        }

        private void CreateTexture(GraphicsDevice graphicsDevice)
        {
            _texture = new Texture2D(graphicsDevice, 1, 1);
            _texture.SetData(new[] { Color.Red });
        }

        public void Update(CollisionTileMap tileMap)
        {
            KeyboardState keyState = Keyboard.GetState();
            Vector2 newVelocity = _velocity;

            bool disableCollisions = keyState.IsKeyDown(Keys.LeftShift);
            //disableCollisions = true; //debug
            _maxSpeed = disableCollisions ? 150f : 15f;

            if (keyState.IsKeyDown(Keys.W)) newVelocity.Y -= _acceleration;
            if (keyState.IsKeyDown(Keys.S)) newVelocity.Y += _acceleration;
            if (keyState.IsKeyDown(Keys.A)) newVelocity.X -= _acceleration;
            if (keyState.IsKeyDown(Keys.D)) newVelocity.X += _acceleration;

            if (!keyState.IsKeyDown(Keys.W) && !keyState.IsKeyDown(Keys.S))
                newVelocity.Y = ApproachZero(newVelocity.Y, _deceleration);
            if (!keyState.IsKeyDown(Keys.A) && !keyState.IsKeyDown(Keys.D))
                newVelocity.X = ApproachZero(newVelocity.X, _deceleration);

            if (newVelocity.LengthSquared() > _maxSpeed * _maxSpeed)
                newVelocity = Vector2.Normalize(newVelocity) * _maxSpeed;

            if (disableCollisions)
            {
                Position += newVelocity;
            }
            else
            {
                // Horizontal movement
                Vector2 newPosition = Position;
                newPosition.X += newVelocity.X;
                Rectangle horizontalRect = new Rectangle((int)newPosition.X, (int)Position.Y, _width, _height);
                if (!tileMap.IsColliding(horizontalRect))
                {
                    Position = new Vector2(newPosition.X, Position.Y);
                }
                else
                {
                    newVelocity.X = 0;
                }

                // Vertical movement
                newPosition = Position;
                newPosition.Y += newVelocity.Y;
                Rectangle verticalRect = new Rectangle((int)Position.X, (int)newPosition.Y, _width, _height);
                if (!tileMap.IsColliding(verticalRect))
                {
                    Position = new Vector2(Position.X, newPosition.Y);
                }
                else
                {
                    newVelocity.Y = 0;
                }
            }

            _velocity = newVelocity;
        }

        private float ApproachZero(float value, float amount)
        {
            if (Math.Abs(value) < amount)
                return 0;
            return value > 0 ? value - amount : value + amount;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRect = new Rectangle((int)Position.X, (int)Position.Y, _width, _height);
            spriteBatch.Draw(_texture, destinationRect, Color.White);
        }
    }
}
