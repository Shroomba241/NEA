using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using CompSci_NEA.Tilemap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using CompSci_NEA;

//BIOME DEFINITIONS REFERENCE:
//6 - Beach
//7 - Plains
//8 - Colourful Plains
//9 - Snow
//10 - Wetlands
//11 - Savanna


public class FoliageManager
{
    private Main game;

    private Dictionary<Point, List<Foliage>> _foliageChunks = new Dictionary<Point, List<Foliage>>();

    private readonly int _chunkSize = 64;   
    private readonly int _step = 3;  
    private readonly int _tileSize = 48;    

    private readonly int globalSeed;    
    private readonly VisualTileMap tileMap; 

    public List<Vector2> MinigamePositions = new List<Vector2>();
    private List<FoliageDefinition> _definitions;

    private const float MINIMUM_DISTANCE = 100f;
    private const int OFFSET_SEED_BASE = 40;
    private const int OFFSET_SEED_Y_BASE = 50;
    private const int SUBGAME_SEED_BASE = 60;
    private const float INTERRACTIONRADIUS = 50f;

    public FoliageManager(Main game, VisualTileMap tileMap, int seed)
    {
        this.game = game;
        this.tileMap = tileMap;
        this.globalSeed = seed;
        SetupDefinitions();
    }

    private void SetupDefinitions() //definitions for the various foliage types which manage their density, behaviour, colour etc
    {
        _definitions = new List<FoliageDefinition>();

        _definitions.Add(new FoliageDefinition(
            name: "Shrub",
            type: FoliageType.Shrub,
            baseChance: 0.05f,
            biomePredicate: b => (b == 7 || b == 11), 
            atlasVariants: new Rectangle[]
            {
                new Rectangle(0, 0, 16, 16),
                new Rectangle(16, 0, 16, 16),
                new Rectangle(32, 0, 16, 16)
            },
            scale: 3f,
            displacementMultiplier: 3f, //3 world pixels max displacement
            tintFunction: (biome, rand) =>
            {
                int variation = (int)(rand * 60) - 30; //-30<v<30
                if (biome == 7)
                {
                    int r = Math.Clamp(50 + variation, 0, 255);
                    int g = Math.Clamp(140 + variation, 0, 255);
                    int b = Math.Clamp(50 + variation, 0, 255);
                    return new Color(r, g, b);
                }
                else if (biome == 11)
                {
                    int r = Math.Clamp(220 + variation, 0, 255);
                    int g = Math.Clamp(200 + variation, 0, 255);
                    int b = Math.Clamp(120 + variation, 0, 255);
                    return new Color(r, g, b);
                }
                return Color.White;
            }
        ));

        _definitions.Add(new FoliageDefinition(
            name: "GrassTuft",
            type: FoliageType.GrassTuft,
            baseChance: 0.1f,
            biomePredicate: b => (b == 7), 
            atlasVariants: new Rectangle[]
            {
                new Rectangle(48, 0, 16, 16),
                new Rectangle(64, 0, 16, 16)
            },
            scale: 3f,
            displacementMultiplier: 6f, 
            tintFunction: (biome, rand) =>
            {
                int variation = (int)(rand * 60) - 30; //-30<v<30
                int r = Math.Clamp(40 + variation, 0, 255);
                int g = Math.Clamp(200 + variation, 0, 255);
                int b = Math.Clamp(40 + variation, 0, 255);
                return new Color(r, g, b);
            }
        ));

        //SPECIAL DEFINITION. foliage system is used to distribute minigames for now
        _definitions.Add(new FoliageDefinition(
            name: "Minigame",
            type: FoliageType.Minigame,
            baseChance: 0.002f,
            biomePredicate: b => (b == 7 || b == 9 || b == 11),
            atlasVariants: new Rectangle[]
            {
                new Rectangle(80, 0, 16, 16) 
            },
            scale: 3f,
            displacementMultiplier: 3f,
            tintFunction: (biome, rand) => Color.White,
            possibleSubGames: new SubGameState[] { SubGameState.Tetris } 
        ));
    }


    private List<Point> GetVisibleChunks(Vector2 playerPosition) //directly copied from tilemap logic, only render nearby foliage
    {
        int playerChunkX = (int)(playerPosition.X / (_tileSize * _chunkSize));
        int playerChunkY = (int)(playerPosition.Y / (_tileSize * _chunkSize));
        List<Point> visible = new List<Point>();
        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                visible.Add(new Point(playerChunkX + offsetX, playerChunkY + offsetY));
            }
        }
        return visible;
    }

    //TLDR is that is generates new foliage for chunks that havent yet been generated, using the position based randomness and applies some distance checks before finishing
    private void GenerateFoliageInChunk(Point chunkCoord)
    {
        if (_foliageChunks.ContainsKey(chunkCoord))
            return;

        List<Foliage> foliageList = new List<Foliage>();

        //spatial hash grid to track foliage positions optimisation. splits chunk into MINIMUM_DISTENCE to reduce comparison efforts
        Dictionary<Point, List<Foliage>> spatialGrid = new Dictionary<Point, List<Foliage>>();

        int gridCellSize = (int)MINIMUM_DISTANCE;
        int startTileX = chunkCoord.X * _chunkSize;
        int startTileY = chunkCoord.Y * _chunkSize;
        int endTileX = startTileX + _chunkSize;
        int endTileY = startTileY + _chunkSize;

        //itterate over posible positions, step is so not every tile has the opportunity for a piece of foliage to be there
        for (int tileX = startTileX; tileX < endTileX; tileX += _step)
        {
            for (int tileY = startTileY; tileY < endTileY; tileY += _step)
            {
                Vector2 position = new Vector2(tileX * _tileSize, tileY * _tileSize);
                byte biome = tileMap.GetTile(tileX, tileY);

                for (int defIndex = 0; defIndex < _definitions.Count; defIndex++)
                {
                    FoliageDefinition def = _definitions[defIndex];
                    if (!def.BiomePredicate(biome))
                        continue;

                    float chanceRand = GetDeterministicRandom(tileX, tileY, globalSeed + 10 + defIndex);
                    if (chanceRand < def.BaseChance)
                    {
                        //calculate tint, variant, and pixel displacement
                        float offsetX = (GetDeterministicRandom(tileX, tileY, globalSeed + OFFSET_SEED_BASE + defIndex) * 2f - 1f) * def.DisplacementMultiplier;
                        float offsetY = (GetDeterministicRandom(tileX, tileY, globalSeed + OFFSET_SEED_Y_BASE + defIndex) * 2f - 1f) * def.DisplacementMultiplier;
                        Vector2 pixelDisplacement = new Vector2(offsetX, offsetY);
                        Vector2 newPos = position + pixelDisplacement;

                        Point gridCell = new Point((int)(newPos.X / gridCellSize), (int)(newPos.Y / gridCellSize));
                        bool tooClose = false;

                        //rubbish - checking if neighbouring cells are foliage and skips if theyre too close to one another
                        for (int offsetXCell = -1; offsetXCell <= 1 && !tooClose; offsetXCell++)
                        {
                            for (int offsetYCell = -1; offsetYCell <= 1 && !tooClose; offsetYCell++)
                            {
                                Point neighbor = new Point(gridCell.X + offsetXCell, gridCell.Y + offsetYCell);
                                if (spatialGrid.TryGetValue(neighbor, out List<Foliage> nearby))
                                {
                                    foreach (var existing in nearby)
                                    {
                                        Vector2 existingPos = existing.Position + existing.PixelDisplacement;
                                        if (Vector2.Distance(newPos, existingPos) < MINIMUM_DISTANCE)
                                        {
                                            tooClose = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (!tooClose) //if no foliage was found being too close, then add the foliage
                        {
                            SubGameState? subGame = null;
                            if (def.Type == FoliageType.Minigame && def.PossibleSubGames != null && def.PossibleSubGames.Length > 0)
                            {
                                float subgameRand = GetDeterministicRandom(tileX, tileY, globalSeed + SUBGAME_SEED_BASE + defIndex);
                                int subgameIndex = (int)(subgameRand * def.PossibleSubGames.Length);
                                subGame = def.PossibleSubGames[subgameIndex];
                                MinigamePositions.Add(newPos); //keeping track of minigames seperately as they are special snowflakes
                            }

                            Foliage f = new Foliage(position, def.TintFunction(biome, GetDeterministicRandom(tileX, tileY, globalSeed + 20 + defIndex)),
                                                      def.Type, def.AtlasVariants[(int)(GetDeterministicRandom(tileX, tileY, globalSeed + 30 + defIndex) * def.AtlasVariants.Length)],
                                                      def.Scale, 0f, pixelDisplacement, subGame);
                            foliageList.Add(f);

                            
                            if (!spatialGrid.TryGetValue(gridCell, out List<Foliage> cellList))
                            {
                                cellList = new List<Foliage>();
                                spatialGrid[gridCell] = cellList;
                            }
                            cellList.Add(f);
                        }
                    }
                }
            }
        }
        _foliageChunks[chunkCoord] = foliageList;
    }

    //TODO: set up GUI interraction button later
    public void UpdateMinigames(Player player) //all this does is chack the player is in a certain radius, and if he is let him start the minigame upon interraciton
    {
        
        KeyboardState state = Keyboard.GetState();

        List<Point> visibleChunks = GetVisibleChunks(player.Position);
        foreach (Point chunk in visibleChunks)
        {
            if (_foliageChunks.TryGetValue(chunk, out List<Foliage> list))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    Foliage f = list[i];
                    if (f.Type == FoliageType.Minigame)
                    {
                        Vector2 minigamePos = f.Position + f.PixelDisplacement;
                        if (Vector2.Distance(player.Position, minigamePos) < INTERRACTIONRADIUS)
                        {
                            //Console.WriteLine("in minigame range");
                            if (state.IsKeyDown(Keys.E))
                            {
                                game.StartMiniGame(f.SubGame.Value);
                                list.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }
    }

    //foliage gets its own random. TODO: shove this in noise generator or something
    private float GetDeterministicRandom(int x, int y, int seed)
    {
        int n = x + y * 57 + seed * 131;
        n = (n << 13) ^ n;
        int m = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
        return ((float)m / 1073741824.0f) % 1.0f;
    }

    // Retrieves all foliage objects from visible chunks.
    private List<Foliage> GetVisibleFoliage(Vector2 playerPosition)
    {
        List<Foliage> result = new List<Foliage>();
        List<Point> visibleChunks = GetVisibleChunks(playerPosition);
        foreach (var chunk in visibleChunks)
        {
            GenerateFoliageInChunk(chunk);
            if (_foliageChunks.TryGetValue(chunk, out var list))
                result.AddRange(list);
        }
        return result;
    }

    //draws beyhind the player
    public void DrawBehind(SpriteBatch spriteBatch, Player player)
    {
        float playerFeet = player.Position.Y + 100;
        List<Foliage> visible = GetVisibleFoliage(player.Position);
        foreach (var f in visible)
        {
            float foliageBase = (f.Position + f.PixelDisplacement).Y + _tileSize;
            if (foliageBase < playerFeet)
            {
                spriteBatch.Draw(
                    TextureManager.FoliageAtlas,
                    f.Position + f.PixelDisplacement,
                    f.SourceRectangle,
                    f.Tint,
                    f.Rotation,
                    Vector2.Zero,
                    f.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }

    //draws infront of the player.
    public void DrawInFront(SpriteBatch spriteBatch, Player player)
    {
        float playerFeet = player.Position.Y + 100;
        List<Foliage> visible = GetVisibleFoliage(player.Position);
        foreach (var f in visible)
        {
            float foliageBase = (f.Position + f.PixelDisplacement).Y + _tileSize;
            if (foliageBase >= playerFeet)
            {
                spriteBatch.Draw(
                    TextureManager.FoliageAtlas,
                    f.Position + f.PixelDisplacement,
                    f.SourceRectangle,
                    f.Tint,
                    f.Rotation,
                    Vector2.Zero,
                    f.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}
