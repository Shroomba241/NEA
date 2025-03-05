using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CompSci_NEA.Minigames.Tetris
{
    public class TetrisGame : Scenes.Scene
    {
        private Main game;

        protected TBoard _board;
        protected ScoreBoard _scoreboard;
        protected Tetromino _currentPiece;

        private bool _rotated = false;
        private Keys _lastKeyPressed;
        private List<Tetromino> _tetrominoQueue = new List<Tetromino>();
        private Texture2D _debugBG;

        public int Level = 0;
        public int TotalLinesCleared = 0;
        public int ShmackInc = 0;
        private float _timer;
        private float _dropInterval = 0.5f;
        private int _score = 0;
        private int _moveHDirection = 0;
        private float _moveHRepeatTimer = 0f;

        private const float MoveStartDelay = 0.2f;
        private const float MoveRepeatDelay = 0.05f;

        public TetrisGame(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            _debugBG = game.Content.Load<Texture2D>("debugBG2");

            _board = new TBoard();
            _scoreboard = new ScoreBoard(game.GraphicsDevice ,new Vector2(450, 30), Color.White, new Vector2(600, 70));

            //tetromino queue init
            _tetrominoQueue.Clear();
            _tetrominoQueue.Add(Tetromino.GenerateRandomTetromino());
            _tetrominoQueue.Add(Tetromino.GenerateRandomTetromino());
            _tetrominoQueue.Add(Tetromino.GenerateRandomTetromino());
            _currentPiece = _tetrominoQueue[0];
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                game.CloseMiniGame(0);

            KeyboardState keyboardState = Keyboard.GetState();

            //movement and rotation stuff
            if (keyboardState.IsKeyDown(Keys.A))
            {
                if (_moveHDirection != -1)
                {
                    _moveHDirection = -1;
                    _moveHRepeatTimer = MoveStartDelay;
                    if (_board.IsValidPosition(_currentPiece, -1, 0))
                        _currentPiece.Position -= new Vector2(1, 0);
                }
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                if (_moveHDirection != 1)
                {
                    _moveHDirection = 1;
                    _moveHRepeatTimer = MoveStartDelay;
                    if (_board.IsValidPosition(_currentPiece, 1, 0))
                        _currentPiece.Position += new Vector2(1, 0);
                }
            }
            else
            {
                _moveHDirection = 0;
                _moveHRepeatTimer = 0f;
            }

            if (_moveHDirection != 0)
            {
                _moveHRepeatTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_moveHRepeatTimer <= 0f)
                {
                    if (_board.IsValidPosition(_currentPiece, _moveHDirection, 0))
                        _currentPiece.Position += new Vector2(_moveHDirection, 0);
                    _moveHRepeatTimer = MoveRepeatDelay;
                }
            }

            if (keyboardState.IsKeyDown(Keys.W) && !_rotated)
            {
                _rotated = true;
                Tetromino tempPiece = _currentPiece.Clone();
                tempPiece.Rotate();
                if (_board.IsValidPosition(tempPiece, 0, 0))
                    _currentPiece.Rotate();
            }
            if (keyboardState.IsKeyUp(Keys.W))
                _rotated = false;

            //difficulty modifiers drop
            float baseDropInterval = Math.Max(0.05f, 0.75f - Level * 0.05f);
            _dropInterval = keyboardState.IsKeyDown(Keys.S) ? 0.05f : baseDropInterval;

            //instant drop
            if (keyboardState.IsKeyDown(Keys.Space) && _lastKeyPressed != Keys.Space)
            {
                while (_board.IsValidPosition(_currentPiece, 0, 1))
                    _currentPiece.MoveDown();
                _board.LockPiece(_currentPiece);

                int linesCleared = _board.ClearLines();
                if (linesCleared > 0)
                {
                    TotalLinesCleared += linesCleared;
                    switch (linesCleared)
                    {
                        case 1: _score += 40 * (Level + 1); break;
                        case 2: _score += 100 * (Level + 1); break;
                        case 3: _score += 300 * (Level + 1); break;
                        case 4: _score += 1200 * (Level + 1); break;
                    }
                    Level = TotalLinesCleared / 8;
                }

                ShmackInc = Math.Max(0, (int)Math.Floor(1 + (Math.Log(Level) / Math.Log(1.8))));

                _tetrominoQueue.RemoveAt(0);
                _tetrominoQueue.Add(Tetromino.GenerateRandomTetromino());
                _currentPiece = _tetrominoQueue[0];

                Console.WriteLine(ShmackInc.ToString());

                if (!_board.IsValidPosition(_currentPiece, 0, 0))
                    ManageYetAnotherLoss();
            }

            //auto drop
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _dropInterval)
            {
                _timer = 0;
                if (_board.IsValidPosition(_currentPiece, 0, 1))
                    _currentPiece.MoveDown();
                else
                {
                    _board.LockPiece(_currentPiece);
                    int linesCleared = _board.ClearLines();
                    if (linesCleared > 0)
                    {
                        TotalLinesCleared += linesCleared;
                        switch (linesCleared)
                        {
                            case 1: _score += 40 * (Level + 1); break;
                            case 2: _score += 100 * (Level + 1); break;
                            case 3: _score += 300 * (Level + 1); break;
                            case 4: _score += 1200 * (Level + 1); break;
                        }
                        Level = TotalLinesCleared / 8;
                    }

                    ShmackInc = Math.Max(0, (int)Math.Floor(1 + (Math.Log(Level) / Math.Log(1.8))));

                    _tetrominoQueue.RemoveAt(0);
                    _tetrominoQueue.Add(Tetromino.GenerateRandomTetromino());
                    _currentPiece = _tetrominoQueue[0];

                    Console.WriteLine(ShmackInc.ToString());

                    if (!_board.IsValidPosition(_currentPiece, 0, 0))
                        ManageYetAnotherLoss();
                }
            }

            _lastKeyPressed = keyboardState.GetPressedKeys().Length > 0 ? keyboardState.GetPressedKeys()[0] : Keys.None;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            game.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(transformMatrix: Matrix.CreateScale(1.5f), samplerState: SamplerState.PointClamp);

            Console.WriteLine(Level);
            spriteBatch.Draw(_debugBG, new Rectangle(0, 0, _debugBG.Width, _debugBG.Height), Color.White);
            _board.Draw(spriteBatch);
            _scoreboard.Draw(spriteBatch, _score);

            //ghost piece
            Tetromino ghostPiece = _currentPiece.Clone();
            while (_board.IsValidPosition(ghostPiece, 0, 1))
                ghostPiece.MoveDown();
            ghostPiece.TetrominoColour = new Color(_currentPiece.TetrominoColour.R, _currentPiece.TetrominoColour.G, _currentPiece.TetrominoColour.B, (byte)100);
            ghostPiece.Draw(spriteBatch);

            _currentPiece.Draw(spriteBatch);

            //next tetromino preview
            if (_tetrominoQueue.Count > 1)
                _tetrominoQueue[1].DrawAt(spriteBatch, new Vector2(480, 250));

            spriteBatch.End();
        }

        private void ManageYetAnotherLoss()
        {
            Database.DbFunctions db = new Database.DbFunctions();
            db.AddTetrisEntry(Main.LoggedInUserID, Main.LoggedInUsername, _score);
            game.CloseMiniGame(ShmackInc);
        }


        public override void Shutdown()
        {
            _debugBG = null;
            _board = null;
            _scoreboard = null;
            _currentPiece = null;
            _tetrominoQueue.Clear();
            game.InMiniGame = false;
        }
    }
}
