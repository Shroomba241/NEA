using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CompSci_NEA.Tilemap
{
    public struct TileType
    {
        public string Name;
        public bool IsSolid;
        public Rectangle TextureRegion;
        public Color Color;
        public List<Rectangle> VariationRegions;

        public TileType(string name, bool isSolid, Rectangle textureRegion, Color color)
        {
            Name = name;
            IsSolid = isSolid;
            TextureRegion = textureRegion;
            Color = color;
            VariationRegions = null;
        }

        public TileType(string name, bool isSolid, Rectangle textureRegion, Color color, List<Rectangle> variationRegions)
        {
            Name = name;
            IsSolid = isSolid;
            TextureRegion = textureRegion;
            Color = color;
            VariationRegions = variationRegions;
        }

        public Rectangle GetEffectiveTextureRegion(int worldX, int worldY)
        {
            if (VariationRegions == null || VariationRegions.Count == 0)
                return TextureRegion;

            int hash = (worldX * 73856093) ^ (worldY * 19349663);
            int index = Math.Abs(hash) % VariationRegions.Count;
            return VariationRegions[index];
        }
    }
}
