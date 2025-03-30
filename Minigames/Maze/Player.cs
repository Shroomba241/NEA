using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.Minigames.Maze
{
    public class Player
    {
        private Vector2 _position;
        public float speed;
        private MGrid _grid;
        private Texture2D _texture;
        private int _size;
        public List<Point> UniquePath { get; private set; } = new List<Point>();
        private HashSet<Point> _visitedCells = new HashSet<Point>();

        public Player(MGrid grid, Point startingCell)
        {
            _grid = grid;
            speed = 150f;
            _size = grid.CellSize - 2;
            _position = new Vector2(grid.OffsetX + startingCell.X * grid.CellSize, grid.OffsetY + startingCell.Y * grid.CellSize);
            UniquePath.Add(startingCell);
            _visitedCells.Add(startingCell);
        }

        public void Update(GameTime gameTime)
        {
            Vector2 movement = Vector2.Zero;
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W))
                movement.Y -= 1;
            if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S))
                movement.Y += 1;
            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A))
                movement.X -= 1;
            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D))
                movement.X += 1;
            if (movement != Vector2.Zero)
                movement.Normalize();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 delta = movement * speed * dt;
            Vector2 newPosX = _position;
            newPosX.X += delta.X;
            if (!Collides(newPosX))
                _position.X = newPosX.X;
            Vector2 newPosY = _position;
            newPosY.Y += delta.Y;
            if (!Collides(newPosY))
                _position.Y = newPosY.Y;

            Rectangle playerRect = new Rectangle((int)_position.X, (int)_position.Y, _size, _size);
            int startCol = (playerRect.X - _grid.OffsetX) / _grid.CellSize;
            int startRow = (playerRect.Y - _grid.OffsetY) / _grid.CellSize;
            int endCol = (playerRect.Right - _grid.OffsetX - 1) / _grid.CellSize;
            int endRow = (playerRect.Bottom - _grid.OffsetY - 1) / _grid.CellSize;
            for (int r = startRow; r <= endRow; r++)
            {
                for (int c = startCol; c <= endCol; c++)
                {
                    Point cell = new Point(c, r);
                    if (!_visitedCells.Contains(cell))
                    {
                        UniquePath.Add(cell);
                        _visitedCells.Add(cell);
                    }
                }
            }
        }

        private bool Collides(Vector2 pos)
        {
            Rectangle newRect = new Rectangle((int)pos.X, (int)pos.Y, _size, _size);
            int startCol = (newRect.X - _grid.OffsetX) / _grid.CellSize;
            int startRow = (newRect.Y - _grid.OffsetY) / _grid.CellSize;
            int endCol = (newRect.Right - _grid.OffsetX) / _grid.CellSize;
            int endRow = (newRect.Bottom - _grid.OffsetY) / _grid.CellSize;
            for (int r = startRow; r <= endRow; r++)
            {
                for (int c = startCol; c <= endCol; c++)
                {
                    if (r >= 0 && r < _grid.Rows && c >= 0 && c < _grid.Cols)
                    {
                        if (_grid.MazeGrid[r, c] == 1)
                        {
                            Rectangle cellRect = new Rectangle(_grid.OffsetX + c * _grid.CellSize, _grid.OffsetY + r * _grid.CellSize, _grid.CellSize, _grid.CellSize);
                            if (newRect.Intersects(cellRect))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _texture.SetData(new Color[] { Color.White });
            }
            Rectangle rect = new Rectangle((int)_position.X, (int)_position.Y, _size, _size);
            spriteBatch.Draw(_texture, rect, Color.Cyan);
        }
    }
}
