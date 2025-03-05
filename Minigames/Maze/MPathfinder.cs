using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CompSci_NEA.Minigames.Maze
{
    public class MPathfinder
    {
        private (Dictionary<Point, int> distances, Dictionary<Point, Point> predecessors) BFS(
            byte[,] grid, int rows, int cols, Point start, Point? target = null, HashSet<(Point, Point)> bannedEdges = null)
        {
            var distances = new Dictionary<Point, int>();
            var predecessors = new Dictionary<Point, Point>();
            var queue = new Queue<Point>();
            var visited = new bool[rows, cols];

            queue.Enqueue(start);
            visited[start.Y, start.X] = true;
            distances[start] = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (target.HasValue && current == target.Value)
                    break;

                foreach (var neighbor in GetValidNeighbors(grid, rows, cols, current, bannedEdges))
                {
                    if (!visited[neighbor.Y, neighbor.X])
                    {
                        visited[neighbor.Y, neighbor.X] = true;
                        distances[neighbor] = distances[current] + 1;
                        predecessors[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return (distances, predecessors);
        }

        private IEnumerable<Point> GetValidNeighbors(
            byte[,] grid, int rows, int cols, Point p, HashSet<(Point, Point)> bannedEdges = null)
        {
            var directions = new Point[]
            {
                new Point(0, -1),
                new Point(0, 1),
                new Point(-1, 0),
                new Point(1, 0)
            };

            foreach (var d in directions)
            {
                var neighbor = new Point(p.X + d.X, p.Y + d.Y);
                if (neighbor.X >= 0 && neighbor.X < cols && neighbor.Y >= 0 && neighbor.Y < rows && grid[neighbor.Y, neighbor.X] == 0)
                {
                    if (bannedEdges != null)
                    {
                        var edge = GetEdgeKey(p, neighbor);
                        if (bannedEdges.Contains(edge))
                            continue;
                    }
                    yield return neighbor;
                }
            }
        }

        public List<Point> BFSPath(
            byte[,] grid, int rows, int cols, Point start, Point end, HashSet<(Point, Point)> bannedEdges)
        {
            var (distances, predecessors) = BFS(grid, rows, cols, start, end, bannedEdges);
            if (!distances.ContainsKey(end))
                return null;

            var path = new List<Point>();
            var current = end;
            while (current != start)
            {
                path.Add(current);
                current = predecessors[current];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }

        public Dictionary<Point, int> GetBFSDistances(byte[,] grid, int rows, int cols, Point start)
        {
            return BFS(grid, rows, cols, start).distances;
        }

        public (Point, Point) GetEdgeKey(Point a, Point b)
        {
            return (a.X < b.X || (a.X == b.X && a.Y <= b.Y)) ? (a, b) : (b, a);
        }
    }
}
