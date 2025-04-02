using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

//in this file connect 4 game logic is held.
//a bitboard is used for optimisation and opperation efficiency over something like parsing a string.

namespace CompSci_NEA.Minigames.Connect4
{
    public class C4Board
    {
        public int Cols;
        public int Rows;
        private ulong[] _bitboards = new ulong[2];
        private ulong _mask;
        public int[] heights; //stores the number of bits in each column, which can simplify things like  win detection

        public C4Board(int columns, int rows)
        {
            Cols = columns;
            Rows = rows;
            _bitboards[0] = 0;
            _bitboards[1] = 0;
            _mask = 0;
            heights = new int[columns];
        }

        public bool IsValidCol(int col)
        {
            return col >= 0 && col < Cols && heights[col] < Rows;
        }

        //uses bit shifting to calculate the position for the correct corresponding cell
        public bool Play(int col, int player)
        {
            if (!IsValidCol(col))
                return false;
            int index = col * 7 + heights[col];
            ulong moveBit = 1UL << index;
            _mask |= moveBit;
            int boardIndex = (player == 1) ? 0 : 1;
            _bitboards[boardIndex] |= moveBit;
            heights[col]++;
            return true;
        }

        public void Rewind(int col, int player) //undo sort of action used for the AI's foresight
        {
            if (heights[col] <= 0)
                return;
            heights[col]--;
            int index = col * 7 + heights[col];
            ulong moveBit = 1UL << index;
            _mask &= ~moveBit; //clears the move from the mask
            int boardIndex = (player == 1) ? 0 : 1;
            _bitboards[boardIndex] &= ~moveBit; //and the players bitboard
        }

        public List<int> GetValidCols()
        {
            List<int> moves = new List<int>();
            for (int col = 0; col < Cols; col++)
            {
                if (IsValidCol(col))
                    moves.Add(col);
            }
            return moves;
        }

        public bool IsFull()
        {
            for (int col = 0; col < Cols; col++)
            {
                if (IsValidCol(col))
                    return false;
            }
            return true;
        }

        public bool CheckWin(int player)
        {
            ulong board = (player == 1) ? _bitboards[0] : _bitboards[1];
            return IsWinningBoard(board);
        }

        //performs for directional shifts so that is can check all winning directions
        //the shifts align the bits
        private bool IsWinningBoard(ulong pos)
        {
            ulong m;
            //vertical check (shift by 1)
            m = pos & (pos >> 1);
            if ((m & (m >> 2)) != 0)
                return true;
            //horizontal check (shift by 7)
            m = pos & (pos >> 7);
            if ((m & (m >> 14)) != 0)
                return true;
            //diagonal up-left check (shift by 6)
            m = pos & (pos >> 6);
            if ((m & (m >> 12)) != 0)
                return true;
            //diagonal up-right check (shift by 8)
            m = pos & (pos >> 8);
            if ((m & (m >> 16)) != 0)
                return true;
            return false;
        }

        public int GetCell(int col, int row)
        {
            int index = col * 7 + row;
            ulong bit = 1UL << index;
            if ((_bitboards[0] & bit) != 0)
                return 1;
            else if ((_bitboards[1] & bit) != 0)
                return -1;
            return 0;
        }

        public int EvaluateBoard(int player)
        {
            int score = 0;
            int centerColumn = Cols / 2;
            int centerCount = 0;
            for (int row = 0; row < Rows; row++)
            {
                if (GetCell(centerColumn, row) == player)
                    centerCount++;
            }
            score += centerCount * 3;

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    //horizontal area
                    if (col <= Cols - 4)
                    {
                        int[] area = new int[4];
                        for (int i = 0; i < 4; i++)
                            area[i] = GetCell(col + i, row);
                        score += EvaluateArea(area, player);
                    }
                    //vertical area
                    if (row <= Rows - 4)
                    {
                        int[] area = new int[4];
                        for (int i = 0; i < 4; i++)
                            area[i] = GetCell(col, row + i);
                        score += EvaluateArea(area, player);
                    }
                    //diagonal UR area
                    if (col <= Cols - 4 && row <= Rows - 4)
                    {
                        int[] area = new int[4];
                        for (int i = 0; i < 4; i++)
                            area[i] = GetCell(col + i, row + i);
                        score += EvaluateArea(area, player);
                    }
                    //diagonal UL area
                    if (col >= 3 && row <= Rows - 4)
                    {
                        int[] area = new int[4];
                        for (int i = 0; i < 4; i++)
                            area[i] = GetCell(col - i, row + i);
                        score += EvaluateArea(area, player);
                    }
                }
            }
            return score;
        }

        private int EvaluateArea(int[] area, int player) //evaluates a specific 4 cells (to check for wins)
        {
            int score = 0;
            int opponent = -player;
            int playerCount = 0, emptyCount = 0, opponentCount = 0;
            foreach (int cell in area)
            {
                if (cell == player)
                    playerCount++;
                else if (cell == opponent)
                    opponentCount++;
                else
                    emptyCount++;
            }
            if (playerCount == 4)
                score += 100;
            else if (playerCount == 3 && emptyCount == 1)
                score += 5;
            else if (playerCount == 2 && emptyCount == 2)
                score += 2;
            if (opponentCount == 3 && emptyCount == 1)
                score -= 4;
            return score;
        }

        public string TTHash() //for the TT in solver, generates a unique hash for the board state in X16
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_bitboards[0].ToString("X16"));
            sb.Append(_bitboards[1].ToString("X16"));
            sb.Append(string.Join(",", heights));
            return sb.ToString();
        }

        public void Draw(SpriteBatch spriteBatch, int cellSize)
        {
            //spriteBatch.Draw(boardTexture, new Rectangle(0, 0, Cols * cellSize, Rows * cellSize), Color.White);

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    int cell = GetCell(col, row);
                    Rectangle destRect = new Rectangle((col + 1) * cellSize, (Rows - row + 1) * cellSize, cellSize, cellSize);
                    if (cell == 1)
                    {
                        spriteBatch.Draw(TextureManager.Disc, destRect, Color.Red);
                    }
                    else if (cell == -1)
                    {
                        spriteBatch.Draw(TextureManager.Disc, destRect, Color.Yellow);
                    }
                }
            }
        }

        public C4Board Clone()
        {
            C4Board clone = new C4Board(this.Cols, this.Rows);
            clone._bitboards[0] = this._bitboards[0];
            clone._bitboards[1] = this._bitboards[1];
            clone._mask = this._mask;
            clone.heights = (int[])this.heights.Clone();
            return clone;
        }
    }
}
