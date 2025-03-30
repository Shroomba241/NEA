using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CompSci_NEA.Minigames.Maze
{
    public class Node
    {
        public Point Position;
        public List<Node> Neighbors;
        public object ExtraData;

        public Node(Point position)
        {
            Position = position;
            Neighbors = new List<Node>();
        }

        public override bool Equals(object obj)
        {
            if (obj is Node other)
                return this.Position.Equals(other.Position);
            return false;
        }

        public override int GetHashCode() => Position.GetHashCode();
    }

    public class MazeGraph
    {
        public Dictionary<Point, Node> Nodes;

        public MazeGraph(byte[,] mazeData, int rows, int cols)
        {
            Nodes = BuildGraph(mazeData, rows, cols);
        }

        private Dictionary<Point, Node> BuildGraph(byte[,] mazeData, int rows, int cols)
        {
            var nodes = new Dictionary<Point, Node>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (mazeData[r, c] == 0)
                    {
                        Point p = new Point(c, r);
                        nodes[p] = new Node(p);
                    }
                }
            }

            foreach (var node in nodes.Values)
            {
                Point pos = node.Position;
                Point[] directions = new Point[]
                {
                    new Point(0, -1),
                    new Point(0, 1),
                    new Point(-1, 0),
                    new Point(1, 0)
                };

                foreach (var d in directions)
                {
                    Point neighborPos = new Point(pos.X + d.X, pos.Y + d.Y);
                    if (nodes.ContainsKey(neighborPos))
                    {
                        node.Neighbors.Add(nodes[neighborPos]);
                    }
                }
            }

            return nodes;
        }

        public List<Node> BFSPath(Node start, Node end)
        {
            var queue = new Queue<Node>();
            var visited = new HashSet<Node>();
            var predecessors = new Dictionary<Node, Node>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();
                if (current.Equals(end))
                    break;

                foreach (var neighbor in current.Neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        predecessors[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (!predecessors.ContainsKey(end) && !start.Equals(end))
                return null;

            var path = new List<Node>();
            Node curr = end;
            while (!curr.Equals(start))
            {
                path.Add(curr);
                if (!predecessors.TryGetValue(curr, out curr))
                    break;
            }
            path.Add(start);
            path.Reverse();
            return path;
        }
    }
}
