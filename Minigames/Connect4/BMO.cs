using CompSci_NEA.Core;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompSci_NEA.Minigames.Connect4
{
    public enum BMODifficulty
    {
        Novice,
        Intermediate,
        Advanced
    }

    public enum BMOEmotion
    {
        Neutral,
        Thinking,
        HappyLow,
        HappyMed,
        HappyHigh,
        SadLow,
        SadMed,
        SadHigh,
        AngryLow,
        AngryMed,
        AngryHigh,
        Cocky,
        Shocked
    }

    public class BMO
    {
        private BMODifficulty difficulty;
        private C4Solver _solver;
        private Random _random;
        private const int BMOSCALE = 20;

        private BMOEmotion _currentEmotion = BMOEmotion.Neutral;
        private float _mood = 0f;
        private float _volatility = 1.0f;
        private float _soreLoser = 1.0f;
        private float _excitement = 1.0f;
        private bool _isThinking = false;

        public BMO(BMODifficulty difficulty, int solverDepth)
        {
            this.difficulty = difficulty;
            _solver = new C4Solver(solverDepth);
            _random = new Random();

            if (difficulty == BMODifficulty.Advanced)
            {
                _volatility = 1.5f;
                _soreLoser = 1.5f;
                _excitement = 1.2f;
            }
            else if (difficulty == BMODifficulty.Intermediate)
            {
                _volatility = 1.0f;
                _soreLoser = 1.0f;
                _excitement = 1.0f;
            }
            else
            {
                _volatility = 0.8f;
                _soreLoser = 0.8f;
                _excitement = 1.3f;
            }

            _currentEmotion = BMOEmotion.Neutral;
        }

        private Rectangle GetFaceRectangle()
        {
            if (difficulty == BMODifficulty.Advanced)
            {
                switch (_currentEmotion)
                {
                    case BMOEmotion.Neutral:
                        return new Rectangle(96, 0, 32, 32);
                    case BMOEmotion.Thinking:
                        return new Rectangle(0, 0, 32, 32);
                    case BMOEmotion.Cocky:
                        return new Rectangle(160, 0, 32, 32);
                    case BMOEmotion.HappyMed:
                    case BMOEmotion.HappyHigh:
                        return new Rectangle(64, 64, 32, 32);
                    case BMOEmotion.AngryLow:
                        return new Rectangle(0, 96, 32, 32);
                    case BMOEmotion.AngryMed:
                        return new Rectangle(32, 96, 32, 32);
                    case BMOEmotion.AngryHigh:
                        return new Rectangle(64, 96, 32, 32);
                    default:
                        return new Rectangle(96, 0, 32, 32);
                }
            }
            else if (difficulty == BMODifficulty.Intermediate)
            {
                switch (_currentEmotion)
                {
                    case BMOEmotion.Neutral:
                        return new Rectangle(32, 0, 32, 32);
                    case BMOEmotion.Thinking:
                        return new Rectangle(64, 0, 32, 32);
                    case BMOEmotion.HappyLow:
                        return new Rectangle(0, 64, 32, 32);
                    case BMOEmotion.HappyMed:
                        return new Rectangle(32, 64, 32, 32);
                    case BMOEmotion.HappyHigh:
                        return new Rectangle(64, 64, 32, 32);
                    case BMOEmotion.SadLow:
                        return new Rectangle(0, 32, 32, 32);
                    case BMOEmotion.SadMed:
                        return new Rectangle(32, 32, 32, 32);
                    case BMOEmotion.SadHigh:
                        return new Rectangle(64, 32, 32, 32);
                    default:
                        return new Rectangle(32, 0, 32, 32);
                }
            }
            else //novice
            {
                switch (_currentEmotion)
                {
                    case BMOEmotion.Neutral:
                    case BMOEmotion.Thinking:
                        return new Rectangle(0, 64, 32, 32);
                    case BMOEmotion.HappyMed:
                        return new Rectangle(32, 64, 32, 32);
                    case BMOEmotion.SadLow:
                        return new Rectangle(32, 32, 32, 32);
                    case BMOEmotion.AngryLow:
                        return new Rectangle(64, 32, 32, 32);
                    default:
                        return new Rectangle(0, 64, 32, 32);
                }
            }
        }

        public async Task<int> GetMove(C4Board board)
        {
            _isThinking = true;
            _currentEmotion = BMOEmotion.Thinking;

            DateTime startTime = DateTime.Now;
            float diff = 0f;

            int chosenMove = await Task.Run(() =>
            {
                var moveScores = _solver.EvaluateMoves(board, -1);
                moveScores.Sort((a, b) => b.score.CompareTo(a.score));
                int bestScore = moveScores[0].score;
                int moveScore;
                int chosenMoveL;
                switch (difficulty)
                {
                    case BMODifficulty.Advanced:
                        chosenMoveL = moveScores[0].move;
                        moveScore = moveScores[0].score;
                        break;
                    case BMODifficulty.Intermediate:
                        if (moveScores.Count >= 2)
                        {
                            double prob = _random.NextDouble();
                            if (prob < 0.7)
                            {
                                chosenMoveL = moveScores[0].move;
                                moveScore = moveScores[0].score;
                            }
                            else
                            {
                                chosenMoveL = moveScores[1].move;
                                moveScore = moveScores[1].score;
                            }
                        }
                        else
                        {
                            chosenMoveL = moveScores[0].move;
                            moveScore = moveScores[0].score;
                        }
                        break;
                    case BMODifficulty.Novice:
                        if (moveScores.Count >= 3)
                        {
                            double prob = _random.NextDouble();
                            if (prob < 0.5)
                            {
                                chosenMoveL = moveScores[0].move;
                                moveScore = moveScores[0].score;
                            }
                            else if (prob < 0.8)
                            {
                                chosenMoveL = moveScores[1].move;
                                moveScore = moveScores[1].score;
                            }
                            else
                            {
                                chosenMoveL = moveScores[2].move;
                                moveScore = moveScores[2].score;
                            }
                        }
                        else if (moveScores.Count == 2)
                        {
                            double prob = _random.NextDouble();
                            if (prob < 0.5)
                            {
                                chosenMoveL = moveScores[0].move;
                                moveScore = moveScores[0].score;
                            }
                            else
                            {
                                chosenMoveL = moveScores[1].move;
                                moveScore = moveScores[1].score;
                            }
                        }
                        else
                        {
                            chosenMoveL = moveScores[0].move;
                            moveScore = moveScores[0].score;
                        }
                        break;
                    default:
                        chosenMoveL = moveScores[0].move;
                        moveScore = moveScores[0].score;
                        break;
                }
                diff = bestScore - moveScore;
                UpdateMood(diff);
                return chosenMoveL;
            });

            if (diff > 5)
            {
                int delayMs = (difficulty == BMODifficulty.Advanced) ? 200 :
                              (difficulty == BMODifficulty.Intermediate) ? 300 : 400;
                await Task.Delay(delayMs);
            }

            TimeSpan elapsed = DateTime.Now - startTime;
            TimeSpan minThinkingTime = TimeSpan.FromSeconds(1.0);
            if (elapsed < minThinkingTime)
                await Task.Delay(minThinkingTime - elapsed);

            _isThinking = false;
            _currentEmotion = GetEmotionFromMood();
            return chosenMove;
        }

        private void UpdateMood(float diff)
        {
            if (diff > 3)
            {
                if (difficulty == BMODifficulty.Advanced)
                    _mood -= 0.5f * _volatility * _soreLoser;
                else if (difficulty == BMODifficulty.Intermediate)
                    _mood -= 0.3f * _volatility * _soreLoser;
                else
                    _mood -= 0.1f * _volatility;
            }
            else
            {
                if (difficulty == BMODifficulty.Advanced)
                    _mood += 0.15f * _volatility * _excitement;
                else if (difficulty == BMODifficulty.Intermediate)
                    _mood += 0.08f * _volatility * _excitement;
                else
                    _mood += 0.12f * _excitement;
            }
            _mood = Math.Clamp(_mood, -1f, 1f);
        }

        public void UpdateMoodForPlayerMove(C4Board board)
        {
            var moves = _solver.EvaluateMoves(board, 1);
            moves.Sort((a, b) => b.score.CompareTo(a.score));
            int bestScore = moves[0].score;
            int currentScore = board.EvaluateBoard(1);
            float diff = bestScore - currentScore;
            if (diff < 5)
                _mood -= 0.3f * _volatility;
            else
                _mood += 0.05f * _volatility;
            _mood = Math.Clamp(_mood, -1f, 1f);
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float decayRate = 0.001f;

            if (_mood > 0)
                _mood -= decayRate * elapsed;
            else if (_mood < 0)
                _mood += decayRate * elapsed;
            if (Math.Abs(_mood) < 0.001f)
                _mood = 0f;

            BMOEmotion newEmotion = GetEmotionFromMood();
            if (newEmotion != _currentEmotion)
            {
                _currentEmotion = newEmotion;
            }
        }

        private BMOEmotion GetEmotionFromMood()
        {
            if (difficulty == BMODifficulty.Advanced)
            {
                if (_mood >= 0.1f)
                {
                    if (_mood < 0.5f)
                        return BMOEmotion.HappyLow;
                    else if (_mood < 0.75f)
                        return BMOEmotion.HappyMed;
                    else
                        return BMOEmotion.HappyHigh;
                }
                else if (_mood > -0.1f)
                {
                    return BMOEmotion.Neutral;
                }
                else
                {
                    if (_mood > -0.6f)
                    {
                        if (_mood > -0.3f)
                            return BMOEmotion.SadLow;
                        else if (_mood > -0.45f)
                            return BMOEmotion.SadMed;
                        else
                            return BMOEmotion.SadHigh;
                    }
                    else
                    {
                        if (_mood < -0.8f)
                            return BMOEmotion.AngryHigh;
                        else if (_mood < -0.7f)
                            return BMOEmotion.AngryMed;
                        else
                            return BMOEmotion.AngryLow;
                    }
                }
            }
            else if (difficulty == BMODifficulty.Intermediate)
            {
                if (_mood >= 0.1f)
                {
                    if (_mood < 0.4f)
                        return BMOEmotion.HappyLow;
                    else if (_mood < 0.7f)
                        return BMOEmotion.HappyMed;
                    else
                        return BMOEmotion.HappyHigh;
                }
                else if (_mood > -0.1f)
                {
                    return BMOEmotion.Neutral;
                }
                else
                {
                    if (_mood > -0.4f)
                        return BMOEmotion.SadLow;
                    else if (_mood > -0.7f)
                        return BMOEmotion.SadMed;
                    else
                        return BMOEmotion.SadHigh;
                }
            }
            else
            {
                if (_mood >= 0.1f)
                {
                    if (_mood < 0.4f)
                        return BMOEmotion.HappyLow;
                    else
                        return BMOEmotion.HappyMed;
                }
                else if (_mood > -0.1f)
                {
                    return BMOEmotion.Neutral;
                }
                else
                {
                    if (_mood > -0.4f)
                        return BMOEmotion.SadLow;
                    else
                        return BMOEmotion.AngryLow;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Console.WriteLine(_mood.ToString());

            Rectangle sourceRect = GetFaceRectangle();
            Rectangle destRect = new Rectangle(1000, 200, 32 * BMOSCALE, 32 * BMOSCALE);
            spriteBatch.Draw(TextureManager.BMOAtlas, destRect, sourceRect, Color.White);
            spriteBatch.DrawString(TextureManager.DefaultFont, _mood.ToString(), new Vector2(0, 50), Color.Red);
        }
    }
}
