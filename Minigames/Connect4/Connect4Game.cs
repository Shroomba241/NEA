using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using CompSci_NEA.GUI;

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
        private Task<int> pendingAIMoveTask = null;
        private Texture2D _pixelOverlay;

        private int _reward = 0;

        private bool _aiThinking = false;
        private C4Board _boardSnapshot;

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

            _pixelOverlay = new Texture2D(game.GraphicsDevice, 1, 1);
            _pixelOverlay.SetData(new Color[] { Color.White });

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

            if (_gameOver && Keyboard.GetState().IsKeyDown(Keys.Enter))
                game.CloseMiniGame(_reward);
                
            _bmo.Update(gameTime);

            _cooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_currentPlayer == 1)
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
                            switch (bmoDifficulty)
                            {
                                case BMODifficulty.Novice:
                                    _reward = 2;
                                    break;
                                case BMODifficulty.Intermediate:
                                    _reward = 3;
                                    break;
                                case BMODifficulty.Advanced:
                                    _reward = 4;
                                    break;
                                default:
                                    _reward = 2;
                                    break;
                            }
                            _gameOver = true;
                        }
                        else if (_board.IsFull())
                        {
                            _reward = 1;
                            _gameOver = true;
                        }
                        else
                        {
                            _bmo.UpdateMoodForPlayerMove(_board);
                            _currentPlayer = -1;
                        }
                    }
                    _cooldown = 0.5f;
                }


            }
            else if (_currentPlayer == -1)
            {
                if (pendingAIMoveTask == null && _cooldown <= 0)
                {
                    _boardSnapshot = _board.Clone();
                    _aiThinking = true;
                    pendingAIMoveTask = _bmo.GetMove(_board);
                    _cooldown = 0.5f;
                }
                else if (pendingAIMoveTask != null && pendingAIMoveTask.IsCompleted && _cooldown <= 0)
                {
                    _aiThinking = false;
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
            if (_aiThinking)
                _boardSnapshot.Draw(spriteBatch, CELLSIZE);
            else
                _board.Draw(spriteBatch, CELLSIZE);
            if (_currentPlayer == 1)
            {
                _disc.Draw(spriteBatch);
            }
            _bmo.Draw(spriteBatch);
            spriteBatch.DrawString(TextureManager.DefaultFont, "BMO Difficulty: " + bmoDifficulty.ToString(), new Vector2(10, 10), Color.White);
            spriteBatch.End();

            if (_gameOver)
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(_pixelOverlay, new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height), Color.Black * 0.8f);
                string resultMessage;
                if (_reward == 1)
                    resultMessage = "Draw!";
                else if (_reward > 1)
                    resultMessage = "You win!";
                else
                    resultMessage = "You lose!";
                Text msg1 = new Text(TextureManager.DefaultFont, resultMessage, new Vector2(500, 300), Color.White, 3f);
                Text msg2 = new Text(TextureManager.DefaultFont, $"Reward: {_reward}", new Vector2(500, 350), Color.Green, 2.5f);
                Text msg3 = new Text(TextureManager.DefaultFont, "Press ENTER to exit", new Vector2(500, 450), Color.Gray, 2.5f);
                msg1.Draw(spriteBatch);
                msg2.Draw(spriteBatch);
                msg3.Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        public override void Shutdown()
        {
            //throw new NotImplementedException();
        }
    }
}
