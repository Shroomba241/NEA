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

    /// <summary>
    /// Selects a texture region from the weighted variants using a deterministic hash based on world coordinates.
    /// </summary>
    public Rectangle GetEffectiveTextureRegion(int worldX, int worldY)
    {
        if (WeightedVariants == null || WeightedVariants.Count == 0)
            return new Rectangle(0, 0, 0, 0);

        // Compute total weight.
        float totalWeight = 0f;
        foreach (var variant in WeightedVariants)
        {
            totalWeight += variant.Weight;
        }

        // Create a deterministic "random" value based on the world coordinates.
        int hash = (worldX * 73856093) ^ (worldY * 19349663);
        float normalized = (Math.Abs(hash) % 10000) / 10000f;
        float threshold = normalized * totalWeight;

        // Iterate over the weighted variants and pick one based on the threshold.
        float cumulative = 0f;
        foreach (var variant in WeightedVariants)
        {
            cumulative += variant.Weight;
            if (threshold < cumulative)
                return variant.Region;
        }

        // Fallback: return the last variant if something unexpected occurs.
        return WeightedVariants[WeightedVariants.Count - 1].Region;
    }
}
