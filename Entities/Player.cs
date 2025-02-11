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
        private Vector2 velocity;

        private float acceleration = 8f;
        private float deceleration = 12f;
        private float maxSpeed = 10f;

        private int width = 50;
        private int height = 100;

        private Texture2D texture;

        public Player(GraphicsDevice graphicsDevice, Vector2 startPosition)
        {
            Position = startPosition;
            CreateTexture(graphicsDevice);
        }

        private void CreateTexture(GraphicsDevice graphicsDevice)
        {
            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.Red });
        }

        public void Update(TileMapCollisions tileMap)
        {
            KeyboardState keyState = Keyboard.GetState();
            Vector2 newVelocity = velocity;

            bool disableCollisions = keyState.IsKeyDown(Keys.LeftShift);
            maxSpeed = disableCollisions ? 150f : 15f;

            if (keyState.IsKeyDown(Keys.W)) newVelocity.Y -= acceleration;
            if (keyState.IsKeyDown(Keys.S)) newVelocity.Y += acceleration;
            if (keyState.IsKeyDown(Keys.A)) newVelocity.X -= acceleration;
            if (keyState.IsKeyDown(Keys.D)) newVelocity.X += acceleration;

            if (!keyState.IsKeyDown(Keys.W) && !keyState.IsKeyDown(Keys.S))
                newVelocity.Y = ApproachZero(newVelocity.Y, deceleration);
            if (!keyState.IsKeyDown(Keys.A) && !keyState.IsKeyDown(Keys.D))
                newVelocity.X = ApproachZero(newVelocity.X, deceleration);

            newVelocity = Vector2.Clamp(newVelocity,
                new Vector2(-maxSpeed, -maxSpeed),
                new Vector2(maxSpeed, maxSpeed));

            if (disableCollisions)
            {
                Position += newVelocity;
            }
            else
            {
                // Horizontal movement
                Vector2 newPosition = Position;
                newPosition.X += newVelocity.X;
                Rectangle horizontalRect = new Rectangle((int)newPosition.X, (int)Position.Y, width, height);
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
                Rectangle verticalRect = new Rectangle((int)Position.X, (int)newPosition.Y, width, height);
                if (!tileMap.IsColliding(verticalRect))
                {
                    Position = new Vector2(Position.X, newPosition.Y);
                }
                else
                {
                    newVelocity.Y = 0;
                }
            }

            velocity = newVelocity;
        }

        private float ApproachZero(float value, float amount)
        {
            if (Math.Abs(value) < amount)
                return 0;
            return value > 0 ? value - amount : value + amount;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRect = new Rectangle((int)Position.X, (int)Position.Y, width, height);
            spriteBatch.Draw(texture, destinationRect, Color.White);
        }
    }
}
