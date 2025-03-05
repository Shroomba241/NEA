using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompSci_NEA.Minigames.Maze
{
    public class MGrid
    {
        public int Cols;
        public int Rows;
        public int CellSize;
        public byte[,] MazeData;
        private Texture2D _pixel;
        public int OffsetX;
        public int OffsetY;
        private Random _rnd = new Random();

        //private List<List<Point>> _kPaths;

        private struct WallInfo
        {
            public int WallRow;
            public int WallCol;
            public int CellRow;
            public int CellCol;
        }


        public MGrid(GraphicsDevice graphicsDevice, int cols, int rows)
        {
            Cols = cols;
            Rows = rows;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new Color[] { Color.White });
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;
            CellSize = Math.Min(screenWidth / Cols, screenHeight / Rows);
            OffsetX = 10;
            OffsetY = (screenHeight - (Rows * CellSize)) / 2;
            MazeData = new byte[Rows, Cols];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    MazeData[r, c] = 1;
            GenerateMaze();
            PunchHoles(0.1);
            MazeData[Rows - 1, 1] = 0;
            MazeData[0, Cols - 2] = 0;
        }

        private bool InBounds(int r, int c)
        {
            return r > 0 && r < Rows - 1 && c > 0 && c < Cols - 1;
        }

        private void GenerateMaze()
        {
            int startRow = Rows - 2;
            int startCol = 1;
            MazeData[startRow, startCol] = 0;
            List<WallInfo> walls = new List<WallInfo>();
            int[,] directions = new int[,] { { -2, 0 }, { 0, 2 }, { 2, 0 }, { 0, -2 } };
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int newRow = startRow + directions[i, 0];
                int newCol = startCol + directions[i, 1];
                if (InBounds(newRow, newCol))
                {
                    int wallRow = startRow + directions[i, 0] / 2;
                    int wallCol = startCol + directions[i, 1] / 2;
                    walls.Add(new WallInfo { WallRow = wallRow, WallCol = wallCol, CellRow = newRow, CellCol = newCol });
                }
            }
            while (walls.Count > 0)
            {
                int index = _rnd.Next(walls.Count);
                WallInfo currentWall = walls[index];
                walls.RemoveAt(index);
                if (InBounds(currentWall.CellRow, currentWall.CellCol) && MazeData[currentWall.CellRow, currentWall.CellCol] == 1)
                {
                    MazeData[currentWall.WallRow, currentWall.WallCol] = 0;
                    MazeData[currentWall.CellRow, currentWall.CellCol] = 0;
                    for (int i = 0; i < directions.GetLength(0); i++)
                    {
                        int nextRow = currentWall.CellRow + directions[i, 0];
                        int nextCol = currentWall.CellCol + directions[i, 1];
                        if (InBounds(nextRow, nextCol) && MazeData[nextRow, nextCol] == 1)
                        {
                            int betweenRow = currentWall.CellRow + directions[i, 0] / 2;
                            int betweenCol = currentWall.CellCol + directions[i, 1] / 2;
                            walls.Add(new WallInfo { WallRow = betweenRow, WallCol = betweenCol, CellRow = nextRow, CellCol = nextCol });
                        }
                    }
                }
            }
        }

        private void PunchHoles(double braidProbability)
        {
            for (int r = 1; r < Rows - 1; r++)
            {
                for (int c = 1; c < Cols - 1; c++)
                {
                    if (MazeData[r, c] == 0)
                    {
                        int openNeighbors = 0;
                        var neighbors = new List<(int dr, int dc)>
                        {
                            (-1, 0), (1, 0), (0, -1), (0, 1)
                        };
                        foreach (var (dr, dc) in neighbors)
                        {
                            if (MazeData[r + dr, c + dc] == 0)
                                openNeighbors++;
                        }
                        if (openNeighbors == 1 && _rnd.NextDouble() < braidProbability)
                        {
                            var closedNeighbors = new List<(int dr, int dc)>();
                            foreach (var (dr, dc) in neighbors)
                            {
                                int nr = r + dr;
                                int nc = c + dc;
                                if (nr > 0 && nr < Rows - 1 && nc > 0 && nc < Cols - 1)
                                {
                                    if (MazeData[nr, nc] == 1)
                                        closedNeighbors.Add((dr, dc));
                                }
                            }
                            if (closedNeighbors.Count > 0)
                            {
                                var (dr, dc) = closedNeighbors[_rnd.Next(closedNeighbors.Count)];
                                MazeData[r + dr, c + dc] = 0;
                            }
                        }
                    }
                }
            }
        }

        public void DrawDebugMap(SpriteBatch spriteBatch, Point start, Point exit)
        {
            var pathfinder = new MPathfinder();
            var distancesFromStart = pathfinder.GetBFSDistances(MazeData, Rows, Cols, start);
            var distancesFromExit = pathfinder.GetBFSDistances(MazeData, Rows, Cols, exit);
            if (distancesFromStart.Count == 0 || distancesFromExit.Count == 0)
                return;
            int minSum = int.MaxValue;
            int maxSum = 0;
            var sumDistances = new Dictionary<Point, int>();
            foreach (var kvp in distancesFromStart)
            {
                Point cell = kvp.Key;
                if (distancesFromExit.ContainsKey(cell))
                {
                    int sum = kvp.Value + distancesFromExit[cell];
                    sumDistances[cell] = sum;
                    if (sum < minSum)
                        minSum = sum;
                    if (sum > maxSum)
                        maxSum = sum;
                }
            }
            foreach (var kvp in sumDistances)
            {
                Point cell = kvp.Key;
                int sum = kvp.Value;
                float ratio = maxSum > minSum ? (float)(sum - minSum) / (maxSum - minSum) : 0f;
                Color cellColor;
                if (ratio <= 0.3f)
                {
                    float t = ratio / 0.3f;
                    cellColor = Color.Lerp(Color.Green, Color.Yellow, t);
                }
                else
                {
                    float t = (ratio - 0.3f) / 0.7f;
                    t = t * t;
                    cellColor = Color.Lerp(Color.Yellow, Color.Red, t);
                }
                Rectangle cellRect = new Rectangle(OffsetX + cell.X * CellSize, OffsetY + cell.Y * CellSize, CellSize, CellSize);
                spriteBatch.Draw(_pixel, cellRect, cellColor);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Rectangle cellRect = new Rectangle(OffsetX + c * CellSize, OffsetY + r * CellSize, CellSize, CellSize);
                    Color cellColor = (MazeData[r, c] == 0) ? Color.LightGray : Color.Black;
                    spriteBatch.Draw(_pixel, cellRect, cellColor);
                }
            }
            if (DEBUG.DebugOptions.MazeDebugMap)
            {
                DrawDebugMap(spriteBatch, new Point(1, Rows - 1), new Point(Cols - 2, 0));
            }
        }
    }
}
