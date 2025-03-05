using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CompSci_NEA.WorldGeneration.Structures
{
    public interface Structure
    {
        void DrawBackgroundInRect(SpriteBatch spriteBatch, Rectangle rect);
        void DrawForegroundInRect(SpriteBatch spriteBatch, Rectangle rect);
        IEnumerable<Rectangle> GetColliders();
    }

}
