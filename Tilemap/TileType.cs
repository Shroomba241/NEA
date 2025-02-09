using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CompSci_NEA.Tilemap
{
    public struct TileType
    {
        public string Name;
        public bool IsSolid;
        public Rectangle TextureRegion; // UV mapping for tile atlas
        public Color Color;

        public TileType(string name, bool isSolid, Rectangle textureRegion, Color color)
        {
            Name = name;
            IsSolid = isSolid;
            TextureRegion = textureRegion;
            Color = color;
        }
    }
}
