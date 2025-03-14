using System;
using System.Collections.Generic;

namespace CompSci_NEA.Minigames.Connect4
{
    public enum BoundType
    {
        E, //exact score
        LB, //lowerbound
        UB //upperbound
    }

    //key optimisation - stores a circumstance which to improve search efficiency by reducing redundant processing.
    public struct TTEntry
    {
        public int Depth;
        public int Score;
        public BoundType Flag;
    }

    public class C4Solver
    {
        private int maxSearchDepth;
        private Dictionary<string, TTEntry> _transpositionTable;

        public C4Solver(int maxSearchDepth)
        {
            this.maxSearchDepth = maxSearchDepth;
            _transpositionTable = new Dictionary<string, TTEntry>();
        }

        public int GetBestMove(C4Board board, int currentPlayer)
        {
            int bestMove = -1;
            _transpositionTable.Clear();

            for (int depth = 1; depth <= maxSearchDepth; depth++) //if my evaluation is gppd, then this should return perfect move given enough depth
            {
                int currentBestMove = -1;
                int currentBestScore = int.MinValue;
                List<int> moves = GetSortedMoves(board);
                foreach (int move in moves)
                {
                    board.Play(move, currentPlayer);
                    //negamax with alpha beta pruning is ran here.
                    int score = -Negamax(board, depth - 1, -currentPlayer, int.MinValue, int.MaxValue, -currentPlayer);
                    board.Rewind(move, currentPlayer);
                    if (score > currentBestScore)
                    {
                        currentBestScore = score;
                        currentBestMove = move;
                    }
                }
                bestMove = currentBestMove;
            }
            return bestMove;
        }

        public List<(int move, int score)> EvaluateMoves(C4Board board, int currentPlayer)
        {
            List<(int move, int score)> moves = new List<(int, int)>();
            List<int> validMoves = board.GetValidCols();
            foreach (int move in validMoves)
            {
                board.Play(move, currentPlayer);
                int score = -Negamax(board, maxSearchDepth - 1, -currentPlayer, int.MinValue, int.MaxValue, currentPlayer);
                board.Rewind(move, currentPlayer);
                moves.Add((move, score));
            }
            return moves;
        }

        private List<int> GetSortedMoves(C4Board board)
        {
            List<int> moves = board.GetValidCols();
            int center = board.Cols / 2;
            moves.Sort((a, b) => Math.Abs(a - center).CompareTo(Math.Abs(b - center)));
            return moves;
        }

        //Negamax recursively evaluates moves in order to find the best possible move for the ai to make.
        private int Negamax(C4Board board, int depth, int player, int alpha, int beta, int perspective)
        {
            string hash = player.ToString() + board.TTHash();
            if (_transpositionTable.TryGetValue(hash, out TTEntry entry) && entry.Depth >= depth)
            {
                if (entry.Flag == BoundType.E)
                    return entry.Score;
                else if (entry.Flag == BoundType.LB)
                    alpha = Math.Max(alpha, entry.Score);
                else if (entry.Flag == BoundType.UB)
                    beta = Math.Min(beta, entry.Score);
                if (alpha >= beta)
                    return entry.Score;
            }

            if (board.CheckWin(-player))
                return -100000 - depth;
            if (depth == 0 || board.IsFull())
                return board.EvaluateBoard(perspective);

            int originalAlpha = alpha;
            int maxScore = int.MinValue;
            foreach (int move in GetSortedMoves(board))
            {
                board.Play(move, player);
                int score = -Negamax(board, depth - 1, -player, -beta, -alpha, perspective);
                board.Rewind(move, player);
                maxScore = Math.Max(maxScore, score);
                alpha = Math.Max(alpha, maxScore);
                if (alpha >= beta)
                    break;
            }

            BoundType flag;
            if (maxScore <= originalAlpha)
                flag = BoundType.UB;
            else if (maxScore >= beta)
                flag = BoundType.LB;
            else
                flag = BoundType.E;
            _transpositionTable[hash] = new TTEntry { Depth = depth, Score = maxScore, Flag = flag };

            return maxScore;
        }
    }
}
