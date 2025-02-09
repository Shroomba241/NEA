using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CompSci_NEA.Tilemap;

namespace CompSci_NEA.Entities
{
    public class Player
    {
        public Vector2 Position { get; private set; }
        private Vector2 velocity;

        private float acceleration = 8f;
        private float deceleration = 12f;
        private float maxSpeed = 15f;

        private Texture2D texture;
        private Rectangle destinationRect;

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
            Vector2 nextVelocity = velocity;

            // Apply acceleration
            if (keyState.IsKeyDown(Keys.W)) nextVelocity.Y -= acceleration;
            if (keyState.IsKeyDown(Keys.S)) nextVelocity.Y += acceleration;
            if (keyState.IsKeyDown(Keys.A)) nextVelocity.X -= acceleration;
            if (keyState.IsKeyDown(Keys.D)) nextVelocity.X += acceleration;

            // Apply deceleration
            if (!keyState.IsKeyDown(Keys.W) && !keyState.IsKeyDown(Keys.S))
                nextVelocity.Y = ApproachZero(nextVelocity.Y, deceleration);
            if (!keyState.IsKeyDown(Keys.A) && !keyState.IsKeyDown(Keys.D))
                nextVelocity.X = ApproachZero(nextVelocity.X, deceleration);

            // Clamp velocity to max speed
            nextVelocity = Vector2.Clamp(nextVelocity, new Vector2(-maxSpeed, -maxSpeed), new Vector2(maxSpeed, maxSpeed));

            // Predict next position
            Vector2 nextPosition = Position + nextVelocity;
            Rectangle nextRect = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, 50, 100);

            // Check collisions only for relevant tiles
            if (!IsColliding(tileMap, nextRect))
            {
                Position = nextPosition;  // Move if no collision
                velocity = nextVelocity;
            }
            else
            {
                velocity = Vector2.Zero; // Stop movement if colliding
            }
        }

        private float ApproachZero(float value, float amount)
        {
            if (Math.Abs(value) < amount) return 0;
            return value > 0 ? value - amount : value + amount;
        }

        private bool IsColliding(TileMapCollisions tileMap, Rectangle playerRect)
        {
            int tileSize = 48;

            // Get the range of tiles the player occupies
            int left = playerRect.Left / tileSize;
            int right = playerRect.Right / tileSize;
            int top = playerRect.Top / tileSize;
            int bottom = playerRect.Bottom / tileSize;

            /*for (int y = top; y <= bottom; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    if (tileMap.IsSolidTile(x, y))
                    {
                        return true; // Collision detected
                    }
                }
            }*/
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            destinationRect = new Rectangle((int)Position.X, (int)Position.Y, 50, 100);
            spriteBatch.Draw(texture, destinationRect, Color.White);
        }
    }
}
