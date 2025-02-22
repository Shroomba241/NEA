using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.GUI
{
    public class Text
    {
        private SpriteFont font;
        private string content;
        public Vector2 position;
        private Color colour;
        private float scale;

        public Text(SpriteFont font, string content, Vector2 position, Color colour, float scale = 1.0f)
        {
            this.font = font;
            this.content = content;
            this.position = position;
            this.colour = colour;
            this.scale = scale;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, content, position, colour, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public void UpdateContent(string newContent)
        {
            content = newContent;
        }
    }
}
