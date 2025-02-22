using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CompSci_NEA
{
    internal class MotherCloud
    {
        private List<Cloud> _clouds = new List<Cloud>();
        private Texture2D cloudTextureAtlas;
        private int screenWidth;
        private double _spawnTimer;
        private Random _random = new Random();

        public MotherCloud(Texture2D cloudTextureAtlas, int screenWidth)
        {
            this.cloudTextureAtlas = cloudTextureAtlas;
            this.screenWidth = screenWidth;
        }

        public void Update(GameTime gameTime)
        {
            _spawnTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_spawnTimer >= _random.Next(3, 6))
            {
                _clouds.Add(new Cloud(cloudTextureAtlas, screenWidth));
                _spawnTimer = 0; 
            }

            for (int i = _clouds.Count - 1; i >= 0; i--)
            {
                _clouds[i].Update(gameTime);
                if (_clouds[i].IsOffscreen)
                {
                    _clouds.RemoveAt(i); 
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var cloud in _clouds)
            {
                cloud.Draw(spriteBatch);
            }
        }
    }
}
