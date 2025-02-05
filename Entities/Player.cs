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
        public Vector2 Position { get; set; }
        private float acceleration = 0.8f;
        private float deceleration = 1.2f;
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

        public void Update(Tilemap.TileMapCollisions tileMap)
        {
            KeyboardState keyState = Keyboard.GetState();

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

            // Predict the next position
            Vector2 nextPosition = Position + velocity;

            // Define the player's rectangle based on the next position
            Rectangle nextDestinationRect = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, 50, 100);

            // Check for collisions with tiles
            foreach (var tile in tileMap.tiles)
            {
                if (tile.IsSolid && tile.GetBoundingBox().Intersects(nextDestinationRect))
                {
                    Rectangle tileBoundingBox = tile.GetBoundingBox();

                    // Determine which side of the player is colliding with the tile
                    float overlapLeft = tileBoundingBox.Right - nextDestinationRect.Left; // Distance from the left
                    float overlapRight = nextDestinationRect.Right - tileBoundingBox.Left; // Distance from the right
                    float overlapTop = tileBoundingBox.Bottom - nextDestinationRect.Top; // Distance from the top
                    float overlapBottom = nextDestinationRect.Bottom - tileBoundingBox.Top; // Distance from the bottom

                    // Find the smallest overlap to resolve the collision
                    float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

                    if (minOverlap == overlapLeft) // Colliding from the left
                    {
                        Console.WriteLine("LEFT");
                        nextPosition.X = tileBoundingBox.Right; // Move to the right of the tile
                        velocity.X = 0; // Reset horizontal velocity
                    }
                    else if (minOverlap == overlapRight) // Colliding from the right
                    {
                        Console.WriteLine("-RIGHT");
                        nextPosition.X = tileBoundingBox.Left - nextDestinationRect.Width; // Move to the left of the tile
                        velocity.X = 0; // Reset horizontal velocity
                    }
                    else if (minOverlap == overlapTop) // Colliding from above
                    {
                        Console.WriteLine("--TOP");
                        nextPosition.Y = tileBoundingBox.Bottom; // Move below the tile
                        velocity.Y = 0; // Reset vertical velocity
                    }
                    else if (minOverlap == overlapBottom) // Colliding from below
                    {
                        Console.WriteLine("---BOTTOM");
                        nextPosition.Y = tileBoundingBox.Top - nextDestinationRect.Height; // Move above the tile
                        velocity.Y = 0; // Reset vertical velocity
                    }
                }
            }

            // Update the player's position after all collision checks
            Position = nextPosition;
        }







        /*public void Update(Tilemap.TileMapCollisions tileMap)
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
            *//*if (tileMap.CheckCollision(destinationRect))
            {
                // Simple collision response: revert to previous position
                Position = previousPosition;
            }*//*
        }*/

        public void Draw(SpriteBatch spriteBatch)
        {
            destinationRect = new Rectangle((int)Position.X, (int)Position.Y, 50, 100);
            spriteBatch.Draw(texture, destinationRect, Color.White);
        }
    }

}
