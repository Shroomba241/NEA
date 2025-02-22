using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.GUI
{
    public class SimplePerformance //to work change values in main
    {
        private float _elapsedTime = 0f;
        private int _frameCount = 0;
        private int _fps = 0;
        private Text fpsText;

        public SimplePerformance(SpriteFont font)
        {
            fpsText = new Text(font, "FPS: 0", new Vector2(10, 10), Color.White, 1.0f);
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCount++;

            if (_elapsedTime >= 1.0f)
            {
                _fps = _frameCount;
                _frameCount = 0;
                _elapsedTime = 0f;
                fpsText.UpdateContent($"FPS: {_fps}");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            fpsText.Draw(spriteBatch);
        }
    }
}
