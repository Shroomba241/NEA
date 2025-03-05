﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CompSci_NEA.Core;

namespace CompSci_NEA.Minigames.Tetris
{
    public class TBoard
    {
        public const int X_OFFSET = 4;
        public const int Y_OFFSET = 2;
        public const int TILESIZE = 32;
        public const int WIDTH = 10;
        public const int HEIGHT = 20;
        private GridCell[,] _grid = new GridCell[HEIGHT, WIDTH];

        public bool IsValidPosition(Tetromino piece, int offsetX, int offsetY)
        {
            for (int y = 0; y < piece.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < piece.Shape.GetLength(1); x++)
                {
                    if (piece.Shape[y, x] != 0)
                    {
                        int boardX = (int)piece.Position.X + x + offsetX;
                        int boardY = (int)piece.Position.Y + y + offsetY;

                        if (boardX < 0 || boardX >= WIDTH || boardY < 0 || boardY >= HEIGHT)
                            return false;

                        if (_grid[boardY, boardX].Type != 0)
                            return false;
                    }
                }
            }
            return true;
        }

        public void LockPiece(Tetromino piece)
        {
            for (int y = 0; y < piece.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < piece.Shape.GetLength(1); x++)
                {
                    if (piece.Shape[y, x] != 0)
                    {
                        int gridX = (int)piece.Position.X + x;
                        int gridY = (int)piece.Position.Y + y;

                        if (gridX >= 0 && gridX < WIDTH && gridY >= 0 && gridY < HEIGHT)
                            _grid[gridY, gridX] = new GridCell(piece.Shape[y, x], piece.TetrominoColour);
                        else
                            Console.WriteLine($"attempted OOB placement at ({gridX}, {gridY})");
                    }
                }
            }
        }

        public int ClearLines()
        {
            int n = 0;
            for (int y = HEIGHT - 1; y >= 0; y--)
            {
                bool isLineFull = true;
                for (int x = 0; x < WIDTH; x++)
                {
                    if (_grid[y, x].Type == 0)
                    {
                        isLineFull = false;
                        break;
                    }
                }

                if (isLineFull)
                {
                    n++;
                    for (int newY = y; newY > 0; newY--)
                    {
                        for (int x = 0; x < WIDTH; x++)
                        {
                            _grid[newY, x] = _grid[newY - 1, x];
                        }
                    }
                    for (int x = 0; x < WIDTH; x++)
                        _grid[0, x] = new GridCell(0, Color.Transparent);

                    y++;
                }
            }
            return n;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    GridCell cell = _grid[y, x];
                    if (cell.Type != 0)
                    {
                        spriteBatch.Draw(TextureManager.Tetromino_texture, new Vector2((x + X_OFFSET) * TILESIZE, (y + Y_OFFSET) * TILESIZE), cell.Colour);
                    }
                }
            }
        }
    }

    public struct GridCell
    {
        public int Type;
        public Color Colour;

        public GridCell(int type, Color colour)
        {
            Type = type;
            Colour = colour;
        }
    }
}
