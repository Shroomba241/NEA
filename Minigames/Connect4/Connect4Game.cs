using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        private BMO _bmo;
        private int _currentPlayer;
        private bool _gameOver;
        //board settings.
        private const int COLS = 7;
        private const int ROWS = 6;
        private const int CELLSIZE = 112;
        private int _boardWidth = COLS * CELLSIZE;
        private int _boardHeight = ROWS * CELLSIZE;

        private Texture2D _boardTexture;
        private float _cooldown = 0.5f; //vars to prevent rapid input issues.
        private bool _holdingLeft, _holdingRight;

        private BMODifficulty bmoDifficulty;

        /* NEW: Field to store the pending AI move task */
        private Task<int> pendingAIMoveTask = null;

        public Connect4Game(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            _disc = new Disc(new Vector2(4, 0), 0, CELLSIZE);
            _board = new C4Board(COLS, ROWS);

            Random random = new Random(); //randomly decides if the player or AI starts first.
            _currentPlayer = (random.NextDouble() < 0.5) ? 1 : -1;
            Console.WriteLine(_currentPlayer == 1 ? "player starts" : "AI starts");

            //game.pauseCurrentSceneUpdating = false;

            double diffChance = random.NextDouble();
            if (diffChance < 0.33)
                bmoDifficulty = BMODifficulty.Novice;
            else if (diffChance < 0.66)
                bmoDifficulty = BMODifficulty.Intermediate;
            else
                bmoDifficulty = BMODifficulty.Advanced;
            _bmo = new BMO(bmoDifficulty, 10);
        }

        public override async void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                game.CloseMiniGame(0);

            if (_gameOver)
                return;

            _bmo.Update(gameTime);

            _cooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_currentPlayer == 1) // Player's turn
            {
                //_bmo.SetEmotion(BMOEmotion.Neutral);
                // Reset pending AI task
                pendingAIMoveTask = null;

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
                            _bmo.UpdateMoodForPlayerMove(_board);  // << ADDED: Now mood updates after player move!
                            _currentPlayer = -1;
                        }
                    }
                    _cooldown = 0.5f;
                }


            }
            else if (_currentPlayer == -1) // AI's turn
            {
                if (pendingAIMoveTask == null && _cooldown <= 0)
                {
                    // Start the asynchronous solver task.
                    pendingAIMoveTask = _bmo.GetMove(_board);
                    _cooldown = 0.5f;
                }
                else if (pendingAIMoveTask != null && pendingAIMoveTask.IsCompleted && _cooldown <= 0)
                {
                    int aiMove = pendingAIMoveTask.Result;
                    if (aiMove != -1 && _board.IsValidCol(aiMove))
                    {
                        _board.Play(aiMove, -1);
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
                    pendingAIMoveTask = null;
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
            // Draw BMO with its current emotion.
            _bmo.Draw(spriteBatch);
            spriteBatch.DrawString(TextureManager.DefaultFont, "BMO Difficulty: " + bmoDifficulty.ToString(), new Vector2(10, 10), Color.White);
            spriteBatch.End();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
