using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CompSci_NEA.Core
{
    public class Animation
    {
        public Texture2D Texture;
        public int FrameWidth;
        public int FrameHeight;
        public int FrameCount;
        public float FrameTime;
        public bool Loop;
        public int StartFrame;
        public int Row;

        private float timer;
        public int CurrentFrame;

        public Animation(Texture2D texture, int frameWidth, int frameHeight, int frameCount, float frameTime, int startFrame, int row, bool loop = true)
        {
            Texture = texture;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            FrameCount = frameCount;
            FrameTime = frameTime;
            Loop = loop;
            StartFrame = startFrame;
            Row = row;
            timer = 0f;
            CurrentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= FrameTime)
            {
                timer -= FrameTime;
                CurrentFrame++;
                if (CurrentFrame >= FrameCount)
                {
                    CurrentFrame = Loop ? 0 : FrameCount - 1;
                }
            }
        }

        public Rectangle GetCurrentFrameRectangle()
        {
            return new Rectangle((StartFrame + CurrentFrame) * FrameWidth, Row * FrameHeight, FrameWidth, FrameHeight);
        }
    }
}
