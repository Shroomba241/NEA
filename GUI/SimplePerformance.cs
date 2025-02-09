using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.GUI
{
    public class SimplePerformance
    {
        private float elapsedTime = 0f;
        private int frameCount = 0;
        private int fps = 0;
        private Text fpsText;

        public SimplePerformance(SpriteFont font)
        {
            fpsText = new Text(font, "FPS: 0", new Vector2(10, 10), Color.White, 1.0f);
        }

        public void Update(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCount++;

            if (elapsedTime >= 1.0f) // Update FPS every second
            {
                fps = frameCount;
                frameCount = 0;
                elapsedTime = 0f;
                fpsText.UpdateContent($"FPS: {fps}");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            fpsText.Draw(spriteBatch);
        }
    }
}
