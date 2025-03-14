using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using CompSci_NEA.Tilemap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using CompSci_NEA;
using System.Linq;

public class FoliageManager
{
    private Main game;
    private readonly int _chunkSize = 64;
    private readonly int _step = 3;
    private readonly int _tileSize = 48;
    private readonly int globalSeed;
    private readonly VisualTileMap tileMap;
    public List<Vector2> MinigamePositions = new List<Vector2>();
    private List<FoliageDefinition> _definitions;

    private Dictionary<Point, List<Foliage>> _staticFoliageChunks = new Dictionary<Point, List<Foliage>>();
    private Dictionary<Point, List<Foliage>> _minigameFoliageChunks = new Dictionary<Point, List<Foliage>>();

    private const float MINIMUM_DISTANCE = 100f;
    private const int OFFSET_SEED_BASE = 40;
    private const int OFFSET_SEED_Y_BASE = 50;
    private const int SUBGAME_SEED_BASE = 60;
    private const float INTERRACTIONRADIUS = 50f;

    public bool UseSavedMinigames = false;

    public FoliageManager(Main game, VisualTileMap tileMap, int seed)
    {
        this.game = game;
        this.tileMap = tileMap;
        this.globalSeed = seed;
        SetupDefinitions();
    }

    private void SetupDefinitions()
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
            displacementMultiplier: 3f,
            tintFunction: (biome, rand) =>
            {
                int variation = (int)(rand * 60) - 30;
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
                int variation = (int)(rand * 60) - 30;
                int r = Math.Clamp(40 + variation, 0, 255);
                int g = Math.Clamp(200 + variation, 0, 255);
                int b = Math.Clamp(40 + variation, 0, 255);
                return new Color(r, g, b);
            }
        ));

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
            possibleSubGames: new SubGameState[] { SubGameState.Tetris, SubGameState.Connect4, SubGameState.Maze }
        ));
    }

    private List<Point> GetVisibleChunks(Vector2 playerPosition)
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

    // Generate static foliage (non-minigame) for a chunk.
    private void GenerateStaticFoliageInChunk(Point chunkCoord)
    {
        if (_staticFoliageChunks.ContainsKey(chunkCoord))
            return;

        List<Foliage> foliageList = new List<Foliage>();
        Dictionary<Point, List<Foliage>> spatialGrid = new Dictionary<Point, List<Foliage>>();
        int gridCellSize = (int)MINIMUM_DISTANCE;
        int startTileX = chunkCoord.X * _chunkSize;
        int startTileY = chunkCoord.Y * _chunkSize;
        int endTileX = startTileX + _chunkSize;
        int endTileY = startTileY + _chunkSize;

        for (int tileX = startTileX; tileX < endTileX; tileX += _step)
        {
            for (int tileY = startTileY; tileY < endTileY; tileY += _step)
            {
                Vector2 position = new Vector2(tileX * _tileSize, tileY * _tileSize);
                byte biome = tileMap.GetTile(tileX, tileY);

                for (int defIndex = 0; defIndex < _definitions.Count; defIndex++)
                {
                    FoliageDefinition def = _definitions[defIndex];
                    // Only process static foliage (skip minigames)
                    if (def.Type == FoliageType.Minigame)
                        continue;
                    if (!def.BiomePredicate(biome))
                        continue;

                    float chanceRand = GetDeterministicRandom(tileX, tileY, globalSeed + 10 + defIndex);
                    if (chanceRand < def.BaseChance)
                    {
                        float offsetX = (GetDeterministicRandom(tileX, tileY, globalSeed + OFFSET_SEED_BASE + defIndex) * 2f - 1f) * def.DisplacementMultiplier;
                        float offsetY = (GetDeterministicRandom(tileX, tileY, globalSeed + OFFSET_SEED_Y_BASE + defIndex) * 2f - 1f) * def.DisplacementMultiplier;
                        Vector2 pixelDisplacement = new Vector2(offsetX, offsetY);
                        Vector2 newPos = position + pixelDisplacement;

                        Point gridCell = new Point((int)(newPos.X / gridCellSize), (int)(newPos.Y / gridCellSize));
                        bool tooClose = false;
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

                        if (!tooClose)
                        {
                            Foliage f = new Foliage(
                                position,
                                def.TintFunction(biome, GetDeterministicRandom(tileX, tileY, globalSeed + 20 + defIndex)),
                                def.Type,
                                def.AtlasVariants[(int)(GetDeterministicRandom(tileX, tileY, globalSeed + 30 + defIndex) * def.AtlasVariants.Length)],
                                def.Scale,
                                0f,
                                pixelDisplacement,
                                null);
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
        _staticFoliageChunks[chunkCoord] = foliageList;
    }

    private void GenerateMinigameFoliageInChunk(Point chunkCoord)
    {
        if (UseSavedMinigames)
            return;
        if (_minigameFoliageChunks.ContainsKey(chunkCoord))
            return;

        List<Foliage> foliageList = new List<Foliage>();
        Dictionary<Point, List<Foliage>> spatialGrid = new Dictionary<Point, List<Foliage>>();
        int gridCellSize = (int)MINIMUM_DISTANCE;
        int startTileX = chunkCoord.X * _chunkSize;
        int startTileY = chunkCoord.Y * _chunkSize;
        int endTileX = startTileX + _chunkSize;
        int endTileY = startTileY + _chunkSize;

        for (int tileX = startTileX; tileX < endTileX; tileX += _step)
        {
            for (int tileY = startTileY; tileY < endTileY; tileY += _step)
            {
                Vector2 position = new Vector2(tileX * _tileSize, tileY * _tileSize);
                byte biome = tileMap.GetTile(tileX, tileY);

                for (int defIndex = 0; defIndex < _definitions.Count; defIndex++)
                {
                    FoliageDefinition def = _definitions[defIndex];
                    if (def.Type != FoliageType.Minigame)
                        continue;
                    if (!def.BiomePredicate(biome))
                        continue;

                    float chanceRand = GetDeterministicRandom(tileX, tileY, globalSeed + 10 + defIndex);
                    if (chanceRand < def.BaseChance)
                    {
                        float offsetX = (GetDeterministicRandom(tileX, tileY, globalSeed + OFFSET_SEED_BASE + defIndex) * 2f - 1f) * def.DisplacementMultiplier;
                        float offsetY = (GetDeterministicRandom(tileX, tileY, globalSeed + OFFSET_SEED_Y_BASE + defIndex) * 2f - 1f) * def.DisplacementMultiplier;
                        Vector2 pixelDisplacement = new Vector2(offsetX, offsetY);
                        Vector2 newPos = position + pixelDisplacement;

                        Point gridCell = new Point((int)(newPos.X / gridCellSize), (int)(newPos.Y / gridCellSize));
                        bool tooClose = false;
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

                        if (!tooClose)
                        {
                            SubGameState? subGame = null;
                            if (def.PossibleSubGames != null && def.PossibleSubGames.Length > 0)
                            {
                                float subgameRand = GetDeterministicRandom(tileX, tileY, globalSeed + SUBGAME_SEED_BASE + defIndex);
                                int subgameIndex = (int)(subgameRand * def.PossibleSubGames.Length);
                                subGame = def.PossibleSubGames[subgameIndex];
                                MinigamePositions.Add(newPos);
                            }

                            Foliage f = new Foliage(
                                position,
                                def.TintFunction(biome, GetDeterministicRandom(tileX, tileY, globalSeed + 20 + defIndex)),
                                def.Type,
                                def.AtlasVariants[(int)(GetDeterministicRandom(tileX, tileY, globalSeed + 30 + defIndex) * def.AtlasVariants.Length)],
                                def.Scale,
                                0f,
                                pixelDisplacement,
                                subGame);
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
        _minigameFoliageChunks[chunkCoord] = foliageList;
    }

    private List<Foliage> GetVisibleFoliage(Vector2 playerPosition)
    {
        List<Foliage> result = new List<Foliage>();
        List<Point> visibleChunks = GetVisibleChunks(playerPosition);
        foreach (var chunk in visibleChunks)
        {
            GenerateStaticFoliageInChunk(chunk);
            if (_staticFoliageChunks.TryGetValue(chunk, out var staticList))
                result.AddRange(staticList);

            GenerateMinigameFoliageInChunk(chunk);
            if (_minigameFoliageChunks.TryGetValue(chunk, out var miniList))
                result.AddRange(miniList);
        }
        return result;
    }

    public void UpdateMinigames(Player player, GameSave save)
    {
        KeyboardState ks = Keyboard.GetState();
        List<Point> visibleChunks = GetVisibleChunks(player.Position);
        foreach (Point chunk in visibleChunks)
        {
            if (_minigameFoliageChunks.TryGetValue(chunk, out List<Foliage> list))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    Foliage f = list[i];
                    Vector2 minigamePos = f.Position + f.PixelDisplacement;
                    if (Vector2.Distance(player.Position, minigamePos) < INTERRACTIONRADIUS &&
                        ks.IsKeyDown(Keys.E))
                    {
                        game.StartMiniGame(f.SubGame.Value);
                        list.RemoveAt(i);
                        if (save.Minigames != null && save.Minigames.Minigames != null)
                        {
                            save.Minigames.Minigames.RemoveAll(mi =>
                                Math.Abs(mi.X - minigamePos.X) < 10f &&
                                Math.Abs(mi.Y - minigamePos.Y) < 10f &&
                                mi.SubGameType == f.SubGame.Value.ToString());
                        }
                    }
                }
            }
        }
    }

    public List<MinigameInfo> GenerateAllMinigameInfo()
    {
        List<MinigameInfo> minigameList = new List<MinigameInfo>();
        foreach (var chunk in _minigameFoliageChunks.Values)
        {
            foreach (Foliage f in chunk)
            {
                if (f.Type == FoliageType.Minigame)
                {
                    Vector2 pos = f.Position + f.PixelDisplacement;
                    minigameList.Add(new MinigameInfo
                    {
                        X = pos.X,
                        Y = pos.Y,
                        SubGameType = f.SubGame.HasValue ? f.SubGame.Value.ToString() : ""
                    });
                }
            }
        }
        return minigameList;
    }

    public void LoadMinigamesFromSave(GameSave save)
    {
        int tileSize = 48;
        int chunkPixelSize = _chunkSize * tileSize;
        foreach (var mi in save.Minigames.Minigames)
        {
            Vector2 pos = new Vector2(mi.X, mi.Y);
            int chunkX = (int)(pos.X / chunkPixelSize);
            int chunkY = (int)(pos.Y / chunkPixelSize);
            Point chunk = new Point(chunkX, chunkY);

            if (!_minigameFoliageChunks.ContainsKey(chunk))
                _minigameFoliageChunks[chunk] = new List<Foliage>();

            Foliage f = new Foliage(
                pos,
                Color.White,
                FoliageType.Minigame,
                new Rectangle(80, 0, 16, 16),
                3f,
                0f,
                Vector2.Zero,
                (SubGameState)Enum.Parse(typeof(SubGameState), mi.SubGameType)
            );
            _minigameFoliageChunks[chunk].Add(f);
        }
    }

    private float GetDeterministicRandom(int x, int y, int seed)
    {
        int n = x + y * 57 + seed * 131;
        n = (n << 13) ^ n;
        int m = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
        return ((float)m / 1073741824.0f) % 1.0f;
    }

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

    public void ClearMinigameFoliage()
    {
        _minigameFoliageChunks.Clear();
    }
}
