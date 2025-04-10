using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompSci_NEA.Core;

namespace CompSci_NEA.WorldGeneration.Structures
{
    public class Podium : Structure
    {
        private Texture2D _tileAtlas;
        public Vector2 Position;
        private const float SCALE = 3.0f;
        private readonly Rectangle _bgSource = new Rectangle(16, 352, 48, 48);
        private readonly Rectangle _pillar1Source = new Rectangle(0, 336, 16, 48);
        private readonly Rectangle _pillar2Source = new Rectangle(32, 304, 16, 48);
        private readonly Rectangle _pillar3Source = new Rectangle(64, 336, 16, 48);

        private Rectangle _pillar1Dest;
        private Rectangle _pillar2Dest;
        private Rectangle _pillar3Dest;

        public float PlayerFeet;

        public int GoalAmount; 
        private GUI.Text _goalText;
        public bool PlayerWasOnPodium = false;

        public Podium(Texture2D tileAtlas, Vector2 position)
        {
            _tileAtlas = tileAtlas;
            Position = position;
            _pillar1Dest = new Rectangle((int)(Position.X - 16 * SCALE), (int)(Position.Y - 16 * SCALE), (int)(16 * SCALE), (int)(48 * SCALE));
            _pillar2Dest = new Rectangle((int)(Position.X + 16 * SCALE), (int)(Position.Y - 48 * SCALE), (int)(16 * SCALE), (int)(48 * SCALE));
            _pillar3Dest = new Rectangle((int)(Position.X + 48 * SCALE), (int)(Position.Y - 16 * SCALE), (int)(16 * SCALE), (int)(48 * SCALE));
            _goalText = new GUI.Text(TextureManager.DefaultFont, $"{GoalAmount}", Position - new Vector2(-35, 60), Color.Gold, 2.0f);
        }

        public void DrawBackgroundInRect(SpriteBatch spriteBatch, Rectangle visibleRect)
        {
            Rectangle bgDest = new Rectangle((int)Position.X, (int)Position.Y, (int)(_bgSource.Width * SCALE), (int)(_bgSource.Height * SCALE));
            if (bgDest.Intersects(visibleRect))
                spriteBatch.Draw(_tileAtlas, Position, _bgSource, Color.White, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);

            DrawPillarIf(spriteBatch, visibleRect, _pillar1Dest, _pillar1Source, r => r.Bottom < PlayerFeet);
            DrawPillarIf(spriteBatch, visibleRect, _pillar2Dest, _pillar2Source, r => r.Bottom < PlayerFeet);
            DrawPillarIf(spriteBatch, visibleRect, _pillar3Dest, _pillar3Source, r => r.Bottom < PlayerFeet);
        }

        public void DrawForegroundInRect(SpriteBatch spriteBatch, Rectangle visibleRect)
        {
            _goalText.UpdateContent($"{Math.Max(GoalAmount, 0)}");
            DrawPillarIf(spriteBatch, visibleRect, _pillar1Dest, _pillar1Source, r => r.Bottom >= PlayerFeet);
            DrawPillarIf(spriteBatch, visibleRect, _pillar2Dest, _pillar2Source, r => r.Bottom >= PlayerFeet);
            DrawPillarIf(spriteBatch, visibleRect, _pillar3Dest, _pillar3Source, r => r.Bottom >= PlayerFeet);
            if (_goalText != null && GoalAmount > 0)
                _goalText.Draw(spriteBatch);
        }

        private void DrawPillarIf(SpriteBatch spriteBatch, Rectangle visibleRect, Rectangle dest, Rectangle source, Func<Rectangle, bool> condition)
        {
            if (dest.Intersects(visibleRect) && condition(dest))
                spriteBatch.Draw(_tileAtlas, dest, source, Color.White);
        }

        public List<Rectangle> GetPillarColliders()
        {
            List<Rectangle> colliders = new List<Rectangle>();
            colliders.Add(new Rectangle(_pillar1Dest.X, _pillar1Dest.Y + _pillar1Dest.Height - 48, 48, 48));
            colliders.Add(new Rectangle(_pillar2Dest.X, _pillar2Dest.Y + _pillar2Dest.Height - 48, 48, 48));
            colliders.Add(new Rectangle(_pillar3Dest.X, _pillar3Dest.Y + _pillar3Dest.Height - 48, 48, 48));
            return colliders;
        }

        public IEnumerable<Rectangle> GetColliders()
        {
            List<Rectangle> colliders = new List<Rectangle>();
            
            colliders.AddRange(GetPillarColliders());
            return colliders;
        }

        public void Donate(int amount)
        {
            GoalAmount -= amount;
            _goalText.UpdateContent($"{Math.Max(GoalAmount, 0)}");

            if (GoalAmount <= 0)
            {
                Console.WriteLine("win");
            }
        }
    }
}