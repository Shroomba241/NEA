using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CompSci_NEA.Tilemap;
using CompSci_NEA.Core;
using System.Collections.Generic;
using System;
using CompSci_NEA.DEBUG;

namespace CompSci_NEA.Entities
{
    public enum Direction { Right, Left, Down, Up }

    public class Player
    {
        public Vector2 Position;
        private Vector2 _velocity;
        private float _acceleration = 8f;
        private float _deceleration = 12f;
        private float _maxSpeed = 10f;
        private int _width = 50;
        private int _height = 100;
        private Texture2D _moveAtlas;
        private Dictionary<Direction, Animation> _idleAnimations;
        private Dictionary<Direction, Animation> _walkAnimations;
        private Direction _currentDirection;
        private bool _isWalking;
        private Texture2D _debugTexture;

        public Player(GraphicsDevice graphicsDevice, Vector2 startPosition, Texture2D moveAtlas)
        {
            Position = startPosition;
            _moveAtlas = moveAtlas;
            CreateDebugTexture(graphicsDevice);
            InitializeAnimations();
            _currentDirection = Direction.Down;
            _isWalking = false;
        }

        private void CreateDebugTexture(GraphicsDevice graphicsDevice)
        {
            _debugTexture = new Texture2D(graphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.Red });
        }

        private void InitializeAnimations()
        {
            float walkFrameTime = 0.08f;
            _idleAnimations = new Dictionary<Direction, Animation>
            {
                { Direction.Right, new Animation(_moveAtlas, 32, 32, 1, 1, 0, 0, false) },
                { Direction.Left, new Animation(_moveAtlas, 32, 32, 1, 1, 0, 1, false) },
                { Direction.Down, new Animation(_moveAtlas, 32, 32, 1, 1, 0, 2, false) },
                { Direction.Up, new Animation(_moveAtlas, 32, 32, 1, 1, 0, 3, false) }
            };
            _walkAnimations = new Dictionary<Direction, Animation>
            {
                { Direction.Right, new Animation(_moveAtlas, 32, 32, 12, walkFrameTime, 1, 0, true) },
                { Direction.Left, new Animation(_moveAtlas, 32, 32, 12, walkFrameTime, 1, 1, true) },
                { Direction.Down, new Animation(_moveAtlas, 32, 32, 12, walkFrameTime, 1, 2, true) },
                { Direction.Up, new Animation(_moveAtlas, 32, 32, 12, walkFrameTime, 1, 3, true) }
            };
        }

        public void Update(CollisionTileMap tileMap, GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            Vector2 input = Vector2.Zero;
            if (ks.IsKeyDown(Keys.W)) input.Y -= 1;
            if (ks.IsKeyDown(Keys.S)) input.Y += 1;
            if (ks.IsKeyDown(Keys.A)) input.X -= 1;
            if (ks.IsKeyDown(Keys.D)) input.X += 1;
            _isWalking = input != Vector2.Zero;
            if (_isWalking)
            {
                input.Normalize();
                if (Math.Abs(input.X) > Math.Abs(input.Y))
                    _currentDirection = input.X > 0 ? Direction.Right : Direction.Left;
                else
                    _currentDirection = input.Y > 0 ? Direction.Down : Direction.Up;
            }
            Vector2 newVelocity = _velocity;
            if (ks.IsKeyDown(Keys.W)) newVelocity.Y -= _acceleration;
            if (ks.IsKeyDown(Keys.S)) newVelocity.Y += _acceleration;
            if (ks.IsKeyDown(Keys.A)) newVelocity.X -= _acceleration;
            if (ks.IsKeyDown(Keys.D)) newVelocity.X += _acceleration;
            if (!ks.IsKeyDown(Keys.W) && !ks.IsKeyDown(Keys.S))
                newVelocity.Y = ApproachZero(newVelocity.Y, _deceleration);
            if (!ks.IsKeyDown(Keys.A) && !ks.IsKeyDown(Keys.D))
                newVelocity.X = ApproachZero(newVelocity.X, _deceleration);
            if (newVelocity.LengthSquared() > _maxSpeed * _maxSpeed)
                newVelocity = Vector2.Normalize(newVelocity) * _maxSpeed;
            if (!ks.IsKeyDown(Keys.LeftShift))
            {
                Vector2 newPosition = Position;
                newPosition.X += newVelocity.X;
                Rectangle horizontalRect = new Rectangle((int)newPosition.X, (int)Position.Y + _height - (_height / 3), _width, _height / 3);
                if (!tileMap.IsColliding(horizontalRect))
                    Position = new Vector2(newPosition.X, Position.Y);
                else
                    newVelocity.X = 0;
                newPosition = Position;
                newPosition.Y += newVelocity.Y;
                Rectangle verticalRect = new Rectangle((int)Position.X, (int)newPosition.Y + _height - (_height / 3), _width, _height / 3);
                if (!tileMap.IsColliding(verticalRect))
                    Position = new Vector2(Position.X, newPosition.Y);
                else
                    newVelocity.Y = 0;
            }
            else
            {
                Position += newVelocity * 10;
            }
            _velocity = newVelocity;
            if (_isWalking)
                _walkAnimations[_currentDirection].Update(gameTime);
            else
                _idleAnimations[_currentDirection].Update(gameTime);
        }

        private float ApproachZero(float value, float amount)
        {
            if (Math.Abs(value) < amount)
                return 0;
            return value > 0 ? value - amount : value + amount;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Animation currentAnim = _isWalking ? _walkAnimations[_currentDirection] : _idleAnimations[_currentDirection];
            Rectangle sourceRect = currentAnim.GetCurrentFrameRectangle();
            spriteBatch.Draw(_moveAtlas, new Vector2(Position.X - 24, Position.Y), sourceRect, Color.White, 0f, Vector2.Zero, 3.0f, SpriteEffects.None, 0f);
            if (DebugOptions.PlayerDebugSquare)
            {
                Rectangle debugRect = new Rectangle((int)Position.X, (int)Position.Y, _width, _height);
                spriteBatch.Draw(_debugTexture, debugRect, Color.Red * 0.5f);
            }
        }
    }
}
