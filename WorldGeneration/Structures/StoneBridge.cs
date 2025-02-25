using CompSci_NEA.Tilemap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CompSci_NEA.WorldGeneration.Structures
{
    public struct StructureTileType
    {
        public TileType TileType;
        public Vector2 Position;
        public StructureTileType(TileType tileType, Vector2 position)
        {
            this.TileType = tileType;
            this.Position = position;
        }
    }

    public class StoneBridge
    {
        private List<StructureTileType> _topBaseTiles;
        private List<StructureTileType> _bottomBaseTiles;
        private List<StructureTileType> _topRailingTiles;
        private List<StructureTileType> _bottomRailingTiles;
        private List<StructureTileType> _topPillarTiles;
        private List<StructureTileType> _bottomPillarTiles;
        private List<StructureTileType> _supportTiles;
        private Texture2D _tileAtlas;
        private int _tileSize;
        private Random _random;

        private static readonly int[] _possibleX = { 0, 16, 32, 48, 64, 80, 96, 112 };
        private static readonly int[] _possibleY = { 160, 176 };
        private static readonly int[] _railingX = { 16, 32, 48, 64, 80, 96 };
        private static readonly int[] _railingY = { 144 };
        private static readonly int[] _railingBottomX = { 16, 32, 48, 64, 80, 96 };
        private static readonly int[] _railingBottomY = { 208 };
        private static readonly int[] _pillarX = { 0, 112 };
        private static readonly int[] _pillarY = { 144 };

        private const int TILE_SPACING = 48;
        private const int SUBSTRACT_OFFSET = 10;
        private const float SCALE = 3.0f;

        public StoneBridge(Texture2D tileAtlas, int seed)
        {
            this._tileAtlas = tileAtlas;
            this._tileSize = 16;
            this._random = new Random(seed);
            _topBaseTiles = new List<StructureTileType>(200);
            _bottomBaseTiles = new List<StructureTileType>(200);
            _topRailingTiles = new List<StructureTileType>(100);
            _bottomRailingTiles = new List<StructureTileType>(100);
            _topPillarTiles = new List<StructureTileType>(40);
            _bottomPillarTiles = new List<StructureTileType>(40);
            _supportTiles = new List<StructureTileType>(100);
        }

        public void Generate(int startX, int startY, int length)
        {
            int railingTopY = startY - TILE_SPACING;
            int baseTopY = startY;
            int baseBottomY = startY + TILE_SPACING;
            int railingBottomYPos = startY + TILE_SPACING * 2 - SUBSTRACT_OFFSET;
            int supportY = startY + TILE_SPACING * 3 - SUBSTRACT_OFFSET;
            Rectangle supportTextureRegion = new Rectangle(0, 224, 128, 48);

            for (int i = 0; i < length; i++)
            {
                int currentX = startX + i * TILE_SPACING;
                int nextX = currentX + TILE_SPACING;
                Vector2 railingTopPosition = new Vector2(currentX, railingTopY);
                Vector2 baseTopPosition = new Vector2(currentX, baseTopY);
                Vector2 baseBottomPosition = new Vector2(currentX, baseBottomY);
                Vector2 railingBottomPosition = new Vector2(currentX, railingBottomYPos);
                Vector2 supportPosition = new Vector2(nextX, supportY);

                int randRailingX = _railingX[_random.Next(_railingX.Length)];
                int randRailingY = _railingY[_random.Next(_railingY.Length)];
                _topRailingTiles.Add(new StructureTileType(
                    new TileType("RailingTop", false, Color.White,
                        new List<WeightedVariant> { new WeightedVariant(new Rectangle(randRailingX, randRailingY, _tileSize, _tileSize), 1) }),
                    railingTopPosition));

                int randPossibleX1 = _possibleX[_random.Next(_possibleX.Length)];
                int randPossibleY1 = _possibleY[_random.Next(_possibleY.Length)];
                _topBaseTiles.Add(new StructureTileType(
                    new TileType("Base", true, Color.White,
                        new List<WeightedVariant> { new WeightedVariant(new Rectangle(randPossibleX1, randPossibleY1, _tileSize, _tileSize), 1) }),
                    baseTopPosition));

                int randPossibleX2 = _possibleX[_random.Next(_possibleX.Length)];
                int randPossibleY2 = _possibleY[_random.Next(_possibleY.Length)];
                _bottomBaseTiles.Add(new StructureTileType(
                    new TileType("Base", true, Color.White,
                        new List<WeightedVariant> { new WeightedVariant(new Rectangle(randPossibleX2, randPossibleY2, _tileSize, _tileSize), 1) }),
                    baseBottomPosition));

                int randRailingBottomX = _railingBottomX[_random.Next(_railingBottomX.Length)];
                int randRailingBottomY = _railingBottomY[_random.Next(_railingBottomY.Length)];
                _bottomRailingTiles.Add(new StructureTileType(
                    new TileType("RailingBottom", false, Color.White,
                        new List<WeightedVariant> { new WeightedVariant(new Rectangle(randRailingBottomX, randRailingBottomY, _tileSize, _tileSize), 1) }),
                    railingBottomPosition));

                if (i % 8 == 0)
                {
                    Vector2 pillarTopBaseLeft = new Vector2(currentX, railingTopY);
                    Vector2 pillarTopBaseRight = new Vector2(nextX, railingTopY);
                    Vector2 pillarTopTopLeft = new Vector2(currentX, railingTopY - TILE_SPACING);
                    Vector2 pillarTopTopRight = new Vector2(nextX, railingTopY - TILE_SPACING);
                    _topPillarTiles.Add(new StructureTileType(
                        new TileType("Pillar", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(_pillarX[0], _pillarY[0], _tileSize, _tileSize), 1) }),
                        pillarTopBaseRight));
                    _topPillarTiles.Add(new StructureTileType(
                        new TileType("Pillar", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(_pillarX[1], _pillarY[0], _tileSize, _tileSize), 1) }),
                        pillarTopBaseLeft));
                    _topPillarTiles.Add(new StructureTileType(
                        new TileType("PillarTop", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(_pillarX[0], _pillarY[0] - 16, _tileSize, _tileSize), 1) }),
                        pillarTopTopRight));
                    _topPillarTiles.Add(new StructureTileType(
                        new TileType("PillarTop", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(_pillarX[1], _pillarY[0] - 16, _tileSize, _tileSize), 1) }),
                        pillarTopTopLeft));
                    _supportTiles.Add(new StructureTileType(
                        new TileType("BridgeSupport", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(supportTextureRegion, 1) }),
                        supportPosition));

                    int pillarBottomUpperY = railingBottomYPos;
                    int pillarBottomLowerY = startY + TILE_SPACING - SUBSTRACT_OFFSET;
                    Vector2 pillarBottomBaseLeft = new Vector2(currentX, pillarBottomUpperY);
                    Vector2 pillarBottomBaseRight = new Vector2(nextX, pillarBottomUpperY);
                    Vector2 pillarBottomLowerLeft = new Vector2(currentX, pillarBottomLowerY);
                    Vector2 pillarBottomLowerRight = new Vector2(nextX, pillarBottomLowerY);
                    _bottomPillarTiles.Add(new StructureTileType(
                        new TileType("Pillar", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(_pillarX[0], _pillarY[0], _tileSize, _tileSize), 1) }),
                        pillarBottomBaseRight));
                    _bottomPillarTiles.Add(new StructureTileType(
                        new TileType("Pillar", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(_pillarX[1], _pillarY[0], _tileSize, _tileSize), 1) }),
                        pillarBottomBaseLeft));
                    _bottomPillarTiles.Add(new StructureTileType(
                        new TileType("PillarBottom", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(_pillarX[0], _pillarY[0] - 16, _tileSize, _tileSize), 1) }),
                        pillarBottomLowerRight));
                    _bottomPillarTiles.Add(new StructureTileType(
                        new TileType("PillarBottom", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(_pillarX[1], _pillarY[0] - 16, _tileSize, _tileSize), 1) }),
                        pillarBottomLowerLeft));
                }
            }
        }

        public void DrawBackgroundInRect(SpriteBatch spriteBatch, Rectangle rect)
        {
            foreach (var tile in _supportTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.Position.X, (int)tile.Position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.TileType.GetEffectiveTextureRegion((int)tile.Position.X, (int)tile.Position.Y);
                    spriteBatch.Draw(_tileAtlas, tile.Position, region, tile.TileType.Color, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in _topBaseTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.Position.X, (int)tile.Position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.TileType.GetEffectiveTextureRegion((int)tile.Position.X, (int)tile.Position.Y);
                    spriteBatch.Draw(_tileAtlas, tile.Position, region, tile.TileType.Color, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in _bottomBaseTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.Position.X, (int)tile.Position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.TileType.GetEffectiveTextureRegion((int)tile.Position.X, (int)tile.Position.Y);
                    spriteBatch.Draw(_tileAtlas, tile.Position, region, tile.TileType.Color, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in _topRailingTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.Position.X, (int)tile.Position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.TileType.GetEffectiveTextureRegion((int)tile.Position.X, (int)tile.Position.Y);
                    spriteBatch.Draw(_tileAtlas, tile.Position, region, tile.TileType.Color, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in _topPillarTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.Position.X, (int)tile.Position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.TileType.GetEffectiveTextureRegion((int)tile.Position.X, (int)tile.Position.Y);
                    spriteBatch.Draw(_tileAtlas, tile.Position, region, tile.TileType.Color, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
                }
            }
        }

        public void DrawForegroundInRect(SpriteBatch spriteBatch, Rectangle rect)
        {
            foreach (var tile in _bottomRailingTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.Position.X, (int)tile.Position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.TileType.GetEffectiveTextureRegion((int)tile.Position.X, (int)tile.Position.Y);
                    spriteBatch.Draw(_tileAtlas, tile.Position, region, tile.TileType.Color, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in _bottomPillarTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.Position.X, (int)tile.Position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.TileType.GetEffectiveTextureRegion((int)tile.Position.X, (int)tile.Position.Y);
                    spriteBatch.Draw(_tileAtlas, tile.Position, region, tile.TileType.Color, 0f, Vector2.Zero, SCALE, SpriteEffects.None, 0f);
                }
            }
        }

        public List<Rectangle> GetBottomRailingColliders()
        {
            List<Rectangle> colliders = new List<Rectangle>();
            int colliderWidth = 48;
            int colliderHeight = 48;

            foreach (var tile in _bottomRailingTiles)
            {
                Rectangle collider = new Rectangle((int)tile.Position.X, (int)tile.Position.Y + 48, colliderWidth, colliderHeight);
                colliders.Add(collider);
            }
            return colliders;
        }
    }
}
