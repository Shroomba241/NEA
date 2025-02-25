using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

public struct WeightedVariant
{
    public Rectangle Region;
    public float Weight;

    public WeightedVariant(Rectangle region, float weight)
    {
        Region = region;
        Weight = weight;
    }
}

public struct TileType
{
    public string Name;
    public bool IsSolid;
    public Color Color;
    public List<WeightedVariant> WeightedVariants;

    public TileType(string name, bool isSolid, Color color, List<WeightedVariant> weightedVariants)
    {
        Name = name;
        IsSolid = isSolid;
        Color = color;
        WeightedVariants = weightedVariants;
    }

    public Rectangle GetEffectiveTextureRegion(int worldX, int worldY)
    {
        if (WeightedVariants == null || WeightedVariants.Count == 0)
            return new Rectangle(0, 0, 0, 0);

        float totalWeight = 0f;
        foreach (var variant in WeightedVariants)
        {
            totalWeight += variant.Weight;
        }
        int hash = (worldX * 73856093) ^ (worldY * 19349663);
        float normalized = (Math.Abs(hash) % 10000) / 10000f;
        float threshold = normalized * totalWeight;

        float cumulative = 0f;
        foreach (var variant in WeightedVariants)
        {
            cumulative += variant.Weight;
            if (threshold < cumulative)
                return variant.Region;
        }

        return WeightedVariants[WeightedVariants.Count - 1].Region;
    }
}
