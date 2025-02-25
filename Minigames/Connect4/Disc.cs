using CompSci_NEA.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.Minigames.Connect4
{
    public class Disc
    {
        public Vector2 Position;
        public Color DiscColor;
        private int discSize;
       
        public Disc(Vector2 position, int player, int discSize) //really the disc passed in is only ever the player, which makes it pointless. but whatever at least this handles cosmic bit flips
        {
            this.Position = position;
            if (player == 0) this.DiscColor = Color.Red;
            else if (player == 1) this.DiscColor = Color.Yellow;
            else this.DiscColor = Color.Green;
            this.discSize = discSize;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Console.WriteLine($"{Position.X}, {Position.Y}");
            spriteBatch.Draw(TextureManager.Disc,
                new Rectangle((int)(Position.X) * discSize, (int)(Position.Y + 1) * discSize, discSize, discSize), DiscColor);
        }

        public void MoveLeft()
        {
            if (Position.X - 1 > 0) Position = new Vector2(Position.X - 1, Position.Y);
        }

        public void MoveRight()
        {
            if (Position.X + 1 < 8) Position = new Vector2(Position.X + 1, Position.Y);
        }
    }
}
