using CompSci_NEA.Entities;
using CompSci_NEA.Core;
using CompSci_NEA.GUI;
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
        private int _totalReward = 0;

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

            if (_isFinished && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                game.CloseMiniGame(_totalReward);
                return;
            }

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
                        _totalReward = _timeReward + _efficiencyReward;
                       
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
            
            spriteBatch.End();

            if (_isFinished)
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(_pixelOverlay, new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height), Color.Black * 0.8f);
                Text gameOverText1 = new Text(_font, "You escaped the maze!", new Vector2(500, 300), Color.White, 3f);
                Text gameOverText2 = new Text(_font, $"Time Taken: {_timeElapsed:F2} s", new Vector2(500, 350), Color.Red, 2.5f);
                Text gameOverText3 = new Text(_font, $"Optimal Time: {_estimatedOptimalTime:F2} s", new Vector2(500, 400), Color.Yellow, 2.5f);
                Text gameOverText4 = new Text(_font, $"Time Reward: {_timeReward}", new Vector2(500, 450), Color.Green, 2.5f);
                Text gameOverText5 = new Text(_font, $"Efficiency: {_efficiencyPercentage:F2}%", new Vector2(500, 500), Color.Yellow, 2.5f);
                Text gameOverText6 = new Text(_font, $"Efficiency Reward: {_efficiencyReward}", new Vector2(500, 550), Color.Green, 2.5f);
                Text gameOverText7 = new Text(_font, $"Total Reward: {_totalReward}", new Vector2(500, 600), Color.Cyan, 2.5f);
                Text gameOverText8 = new Text(_font, "Press ENTER to exit", new Vector2(500, 700), Color.Gray, 2.5f);


                gameOverText1.Draw(spriteBatch);
                gameOverText2.Draw(spriteBatch);
                gameOverText3.Draw(spriteBatch);
                gameOverText4.Draw(spriteBatch);
                gameOverText5.Draw(spriteBatch);
                gameOverText6.Draw(spriteBatch);
                gameOverText7.Draw(spriteBatch);
                gameOverText8.Draw(spriteBatch);

                spriteBatch.End();
            }
        }

        public override void Shutdown()
        {
        }
    }
}
