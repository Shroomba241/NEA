using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using CompSci_NEA.Tilemap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public enum FoliageType
{
    Shrub,
    Tree,
    GrassTuft,
    Minigame
}

public struct Foliage
{
    public Vector2 Position;
    public Color Tint;
    public FoliageType Type;
    public Rectangle SourceRectangle;
    public float Scale;
    public float Rotation;
    public Vector2 PixelDisplacement; 

    public SubGameState? SubGame;

    public Foliage(Vector2 position, Color tint, FoliageType type = FoliageType.Shrub,
                   Rectangle? sourceRectangle = null, float scale = 3f, float rotation = 0f,
                   Vector2? pixelDisplacement = null, SubGameState? subGame = null)
    {
        Position = position;
        Tint = tint;
        Type = type;
        SourceRectangle = sourceRectangle ?? new Rectangle(0, 0, 16, 16); //defensive, but this shouldnt ever really happen
        Scale = scale;
        Rotation = rotation;
        PixelDisplacement = pixelDisplacement ?? Vector2.Zero;
        SubGame = subGame;
    }
}

public class FoliageDefinition //this exists so that each foliage type has its own behaviour, as defined in the foligage manager
{
    public string Name;
    public FoliageType Type;
    public float BaseChance;
    public Func<byte, bool> BiomePredicate; 
    public Rectangle[] AtlasVariants; 
    public float Scale;
    public float DisplacementMultiplier;  
    public Func<byte, float, Color> TintFunction; 

    public SubGameState[] PossibleSubGames; //only for special minigame type

    public FoliageDefinition(string name, FoliageType type, float baseChance, Func<byte, bool> biomePredicate,
                             Rectangle[] atlasVariants, float scale, float displacementMultiplier,
                             Func<byte, float, Color> tintFunction,
                             SubGameState[] possibleSubGames = null)
    {
        Name = name;
        Type = type;
        BaseChance = baseChance;
        BiomePredicate = biomePredicate;
        AtlasVariants = atlasVariants;
        Scale = scale;
        DisplacementMultiplier = displacementMultiplier;
        TintFunction = tintFunction;
        PossibleSubGames = possibleSubGames;
    }
}