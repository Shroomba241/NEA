using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

//TODO: add ui - like for when the bot is thinking so the player doesnt get confused
//Alaos add rewards, since right now its pointless to play connect 4 at all as far as the main game loop  is concered
//Fianlly think about difficulty - maxdepth is not the best way to make the ai feel human

namespace CompSci_NEA.Minigames.Connect4
{
    public class Connect4Game : Scenes.Scene
    {
        private Main game;

        private Disc _disc;
        private C4Board _board;
        private C4Solver _solver;
        private int _currentPlayer;
        private bool _gameOver;
        //board settings.
        private const int COLS = 7;
        private const int ROWS = 6;
        private const int CELLSIZE = 112;
        private int _boardWidth = COLS * CELLSIZE;
        private int _boardHeight = ROWS * CELLSIZE;

        private Texture2D _boardTexture;
        private float _cooldown = 0.5f; //vars to prrvent rapid input issues.
        private bool _holdingLeft, _holdingRight;

        public Connect4Game(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            _disc = new Disc(new Vector2(4, 0), 0, CELLSIZE);
            _board = new C4Board(COLS, ROWS);
            _solver = new C4Solver(10);

            Random random = new Random(); //randomly decides if the player or AI starts first. the first player has the significant advantage but oh well who cares.
            _currentPlayer = (random.NextDouble() < 0.5) ? 1 : -1;
            Console.WriteLine(_currentPlayer == 1 ? "player starts" : "AI starts");

            //game.pauseCurrentSceneUpdating = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (_gameOver) //TODO shove gameover logic here. maybe like a screen telling you how many coins you got... (or missed out on)
                return;

            _cooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_currentPlayer == 1) //input handling as long as its your turn
            {
                KeyboardState keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.Left) && !_holdingLeft)
                {
                    _disc.MoveLeft();
                    _holdingLeft = true;
                }
                else if (!keyState.IsKeyDown(Keys.Left))
                {
                    _holdingLeft = false;
                }

                if (keyState.IsKeyDown(Keys.Right) && !_holdingRight)
                {
                    _disc.MoveRight();
                    _holdingRight = true;
                }
                else if (!keyState.IsKeyDown(Keys.Right))
                {
                    _holdingRight = false;
                }

                if (keyState.IsKeyDown(Keys.Space) && _cooldown <= 0)
                {
                    int col = (int)_disc.Position.X - 1;

                    if (_board.IsValidCol(col))
                    {
                        _board.Play(col, 1);
                        //checks instanty after dropping if that move won the game
                        if (_board.CheckWin(1))
                        {
                            Console.WriteLine("player wins");
                            _gameOver = true;
                        }
                        else if (_board.IsFull())
                        {
                            Console.WriteLine("draw");
                            _gameOver = true;
                        }
                        else
                        {
                            _currentPlayer = -1;
                        }
                    }
                    _cooldown = 0.5f;
                }
            }
            else if (_currentPlayer == -1) //very similar logic but from the AI's side
            {
                if (_cooldown <= 0)
                {
                    int bestMove = _solver.GetBestMove(_board, -1);
                    if (bestMove != -1 && _board.IsValidCol(bestMove))
                    {
                        _board.Play(bestMove, -1);

                        if (_board.CheckWin(-1))
                        {
                            Console.WriteLine("AI wins");
                            _gameOver = true;
                        }
                        else if (_board.IsFull())
                        {
                            Console.WriteLine("draw");
                            _gameOver = true;
                        }
                        else
                        {
                            _currentPlayer = 1;
                        }
                    }
                    _cooldown = 0.5f;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _board.Draw(spriteBatch, CELLSIZE);

            if (_currentPlayer == 1)
            {
                _disc.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        public override void Shutdown() //sort this shit out
        {
            throw new NotImplementedException();
        }
    }
}
