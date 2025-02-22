using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using CompSci_NEA.GUI;
using CompSci_NEA.Core;

namespace CompSci_NEA
{
    public class ScoreBoard
    {
        private SpriteFont _font;
        private Vector2 scorePosition;
        private Color colour = Color.White;
        private Leaderboard leaderboard;

        public ScoreBoard(GraphicsDevice graphicsDevice, Vector2 scorePosition, Color colour, Vector2 leaderboardPosition)
        {
            _font = TextureManager.DefaultFont;
            this.scorePosition = scorePosition;
            this.colour = colour;
            leaderboard = new Leaderboard(graphicsDevice, _font, leaderboardPosition);
        }

        public void Update(GameTime gameTime)
        {
            leaderboard.Update();
        }

        public void Draw(SpriteBatch spriteBatch, int score)
        {
            string formattedScore = score.ToString().PadLeft(10, '0');
            spriteBatch.DrawString(_font, formattedScore, scorePosition, colour, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            leaderboard.Draw(spriteBatch);
        }
    }
}
