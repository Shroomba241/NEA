using CompSci_NEA.Entities;
using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompSci_NEA.Minigames.Maze
{
    public class MazeGame : Scenes.Scene
    {
        private Main game;
        private MGrid _grid;
        private Player _player;
        private float _timeElapsed = 0f;
        private SpriteFont _font;
        private Texture2D _pixelOverlay;
        private bool _isFinished = false;
        private int _timeReward = 0;
        private int _efficiencyReward = 0;
        private double _estimatedOptimalTime = 0.0;
        private double _efficiencyPercentage = 0.0;
        private List<Point> _optimalPath;
        private MazeGraph _mazeGraph;

        public MazeGame(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            _grid = new MGrid(game.GraphicsDevice, 111, 83, _mazeGraph);
            _player = new Player(_grid, new Point(1, _grid.Rows - 1));
            _font = game.Content.Load<SpriteFont>("DefaultFont");
            _pixelOverlay = new Texture2D(game.GraphicsDevice, 1, 1);
            _pixelOverlay.SetData(new Color[] { Color.White });
            _mazeGraph = new MazeGraph(_grid.MazeGrid, _grid.Rows, _grid.Cols);
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                game.CloseMiniGame(0);
            if (!_isFinished)
            {
                _player.Update(gameTime);
                _timeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            Point exitCell = new Point(_grid.Cols - 2, 0);
            if (!_isFinished && _player.UniquePath.Count > 0 && _player.UniquePath[_player.UniquePath.Count - 1] == exitCell)
            {
                Point startPoint = new Point(1, _grid.Rows - 1);
                if (_mazeGraph.Nodes.TryGetValue(startPoint, out Node startNode) &&
                    _mazeGraph.Nodes.TryGetValue(exitCell, out Node exitNode))
                {
                    List<Node> path = _mazeGraph.BFSPath(startNode, exitNode);
                    if (path != null)
                    {
                        _optimalPath = path.Select(n => n.Position).ToList();
                
                        int steps = _optimalPath.Count - 1;
                        double totalDistance = steps * _grid.CellSize;
                        _estimatedOptimalTime = totalDistance / _player.speed;
                        double timeFactor = _timeElapsed / _estimatedOptimalTime;
                        if (timeFactor <= 2)
                            _timeReward = 3;
                        else if (timeFactor <= 3)
                            _timeReward = 2;
                        else if (timeFactor <= 4.5)
                            _timeReward = 1;
                        else
                            _timeReward = 0;
                        _efficiencyPercentage = ((double)_optimalPath.Count / _player.UniquePath.Count) * 100.0;
                        if (_efficiencyPercentage >= 85)
                            _efficiencyReward = 3;
                        else if (_efficiencyPercentage >= 65)
                            _efficiencyReward = 2;
                        else if (_efficiencyPercentage >= 45)
                            _efficiencyReward = 1;
                        else
                            _efficiencyReward = 0;
                        _isFinished = true;
                        /*Console.WriteLine($"efficiency: {_efficiencyPercentage:F2}%");
                        Console.WriteLine($"eptimal time: {_estimatedOptimalTime} s");
                        Console.WriteLine($"your time: {_timeElapsed} s");
                        Console.WriteLine($"time reward: {_timeReward}");
                        Console.WriteLine($"efficiency reward: {_efficiencyReward}");*/
                    }
                }
            } 
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            _grid.Draw(spriteBatch);
            _player.Draw(spriteBatch);
            if (_isFinished && _optimalPath != null)
            {
                foreach (Point p in _optimalPath)
                {
                    Rectangle cellRect = new Rectangle(_grid.OffsetX + p.X * _grid.CellSize, _grid.OffsetY + p.Y * _grid.CellSize, _grid.CellSize, _grid.CellSize);
                    spriteBatch.Draw(_pixelOverlay, cellRect, Color.Red * 0.5f);
                }
                foreach (Point p in _player.UniquePath)
                {
                    Rectangle cellRect = new Rectangle(_grid.OffsetX + p.X * _grid.CellSize, _grid.OffsetY + p.Y * _grid.CellSize, _grid.CellSize, _grid.CellSize);
                    spriteBatch.Draw(_pixelOverlay, cellRect, Color.Blue * 0.5f);
                }
            }
            spriteBatch.DrawString(_font, $"Elapsed Time: {_timeElapsed:F2} s", new Vector2(10, 10), Color.Red);
            int infoX = _grid.OffsetX + _grid.Cols * _grid.CellSize + 20;
            int lineHeight = 20;
            int infoY = 10;
            if (_isFinished)
            {
                spriteBatch.DrawString(_font, $"Optimal Time: {_estimatedOptimalTime:F2} s", new Vector2(infoX, infoY), Color.Yellow);
                infoY += lineHeight;
                spriteBatch.DrawString(_font, $"Time Factor: {(_timeElapsed / _estimatedOptimalTime):F2}", new Vector2(infoX, infoY), Color.Yellow);
                infoY += lineHeight;
                spriteBatch.DrawString(_font, $"Time Reward: {_timeReward}", new Vector2(infoX, infoY), Color.Green);
                infoY += lineHeight;
                spriteBatch.DrawString(_font, $"Efficiency: {_efficiencyPercentage:F2}%", new Vector2(infoX, infoY), Color.Yellow);
                infoY += lineHeight;
                spriteBatch.DrawString(_font, $"Efficiency Reward: {_efficiencyReward}", new Vector2(infoX, infoY), Color.Green);
            }
            spriteBatch.End();
        }

        public override void Shutdown()
        {
        }
    }
}
