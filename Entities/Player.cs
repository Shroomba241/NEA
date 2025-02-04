using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompSci_NEA.Tilemap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace CompSci_NEA.Entities
{
    public class Player
    {
        public Vector2 Position { get; private set; }
        private float acceleration = 0.6f;
        private float deceleration = 1f;
        private float maxSpeed = 5f;
        private Vector2 velocity;
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

        public void Update(TileMap tileMap)
        {
            KeyboardState keyState = Keyboard.GetState();
            Vector2 previousPosition = Position;

            // Apply acceleration when keys are pressed
            if (keyState.IsKeyDown(Keys.W)) velocity.Y -= acceleration;
            if (keyState.IsKeyDown(Keys.S)) velocity.Y += acceleration;
            if (keyState.IsKeyDown(Keys.A)) velocity.X -= acceleration;
            if (keyState.IsKeyDown(Keys.D)) velocity.X += acceleration;

            // Apply deceleration when no input is given
            if (!keyState.IsKeyDown(Keys.W) && !keyState.IsKeyDown(Keys.S))
            {
                if (velocity.Y > 0) velocity.Y -= deceleration;
                if (velocity.Y < 0) velocity.Y += deceleration;
                if (Math.Abs(velocity.Y) < deceleration) velocity.Y = 0;
            }

            if (!keyState.IsKeyDown(Keys.A) && !keyState.IsKeyDown(Keys.D))
            {
                if (velocity.X > 0) velocity.X -= deceleration;
                if (velocity.X < 0) velocity.X += deceleration;
                if (Math.Abs(velocity.X) < deceleration) velocity.X = 0;
            }

            // Clamp velocity to maxSpeed
            velocity = Vector2.Clamp(velocity, new Vector2(-maxSpeed, -maxSpeed), new Vector2(maxSpeed, maxSpeed));

            // Update position
            Position += velocity;

            // Check for collision
            destinationRect = new Rectangle((int)Position.X, (int)Position.Y, 50, 100); // Define the player's rectangle
            if (tileMap.CheckCollision(destinationRect))
            {
                // Simple collision response: revert to previous position
                Position = previousPosition;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            destinationRect = new Rectangle((int)Position.X, (int)Position.Y, 50, 100);
            spriteBatch.Draw(texture, destinationRect, Color.White);
        }
    }

}
