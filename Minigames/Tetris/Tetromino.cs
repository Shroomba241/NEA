using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using CompSci_NEA.Core;

namespace CompSci_NEA.Minigames.Tetris
{
    public class Tetromino
    {
        public Vector2 Position;
        public int[,] Shape;
        public Color TetrominoColour;
        public Texture2D Texture = TextureManager.Tetromino_texture;

        private static Random _rnd = new Random();

        public static Tetromino GenerateRandomTetromino()
        {
            int type = _rnd.Next(7);
            switch (type)
            {
                case 0: // Square
                    return new Tetromino
                    {
                        TetrominoColour = Color.Yellow,
                        Position = new Vector2(3, 0),
                        Shape = new int[2, 2] {
                        { 1, 1 },
                        { 1, 1 }
                    }
                    };
                case 1: // L
                    return new Tetromino
                    {
                        TetrominoColour = Color.Orange,
                        Position = new Vector2(3, 0),
                        Shape = new int[3, 3] {
                        { 0, 0, 1 },
                        { 1, 1, 1 },
                        { 0, 0, 0 }
                    }
                    };
                case 2: // J
                    return new Tetromino
                    {
                        TetrominoColour = Color.Blue,
                        Position = new Vector2(3, 0),
                        Shape = new int[3, 3] {
                        { 1, 0, 0 },
                        { 1, 1, 1 },
                        { 0, 0, 0 }
                        }
                    };
                case 3: // T
                    return new Tetromino
                    {
                        TetrominoColour = Color.BlueViolet,
                        Position = new Vector2(3, 0),
                        Shape = new int[3, 3] {
                        { 0, 1, 0 },
                        { 1, 1, 1 },
                        { 0, 0, 0 }
                        }
                    };
                case 4: // Z
                    return new Tetromino
                    {
                        TetrominoColour = Color.Red,
                        Position = new Vector2(3, 0),
                        Shape = new int[3, 3] {
                        { 1, 1, 0 },
                        { 0, 1, 1 },
                        { 0, 0, 0 }
                        }
                    };
                case 5: // S
                    return new Tetromino
                    {
                        TetrominoColour = Color.LimeGreen,
                        Position = new Vector2(3, 0),
                        Shape = new int[3, 3] {
                        { 0, 1, 1 },
                        { 1, 1, 0 },
                        { 0, 0, 0 }
                        }
                    };
                case 6: // I 
                    return new Tetromino
                    {
                        TetrominoColour = Color.Aqua,
                        Position = new Vector2(3, 0),
                        Shape = new int[4, 4] {
                        { 0, 0, 0, 0 },
                        { 1, 1, 1, 1 },
                        { 0, 0, 0, 0 },
                        { 0, 0, 0, 0 }
                        }
                    };
                default:
                    throw new Exception("Unknown Tetromino type.");
            }
        }

        public Tetromino Clone()
        {
            return new Tetromino
            {
                Shape = (int[,])Shape.Clone(),
                Position = this.Position,
                TetrominoColour = this.TetrominoColour
            };
        }

        public void Rotate()
        {
            int size = Shape.GetLength(0);
            int[,] newShape = new int[size, size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    newShape[x, size - 1 - y] = Shape[y, x];
                }
            }
            Shape = newShape;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < Shape.GetLength(0); y++)
            {
                for (int x = 0; x < Shape.GetLength(1); x++)
                {
                    if (Shape[y, x] == 1)
                    {
                        spriteBatch.Draw(TextureManager.Tetromino_texture,
                            new Rectangle((int)((Position.X + x + TBoard.X_OFFSET) * TBoard.TILESIZE),
                                          (int)((Position.Y + y + TBoard.Y_OFFSET) * TBoard.TILESIZE),
                                          TBoard.TILESIZE, TBoard.TILESIZE), TetrominoColour);
                    }
                }
            }
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 drawPosition)
        {
            for (int y = 0; y < Shape.GetLength(0); y++)
            {
                for (int x = 0; x < Shape.GetLength(1); x++)
                {
                    if (Shape[y, x] == 1)
                    {
                        spriteBatch.Draw(TextureManager.Tetromino_texture,
                            new Rectangle((int)(drawPosition.X + x * TBoard.TILESIZE),
                                          (int)(drawPosition.Y + y * TBoard.TILESIZE),
                                          TBoard.TILESIZE, TBoard.TILESIZE), TetrominoColour);
                    }
                }
            }
        }

        public void MoveDown() { Position = new Vector2(Position.X, Position.Y + 1); }
        public void MoveLeft() { Position = new Vector2(Position.X - 1, Position.Y); }
        public void MoveRight() { Position = new Vector2(Position.X + 1, Position.Y); }
        public void MoveUp() { Position = new Vector2(Position.X, Position.Y - 1); }
    }
}
