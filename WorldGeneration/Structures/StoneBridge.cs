using CompSci_NEA.Tilemap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CompSci_NEA.WorldGeneration.Structures
{
    public struct StructureTileType
    {
        public TileType tileType;
        public Vector2 position;
        public StructureTileType(TileType tileType, Vector2 position)
        {
            this.tileType = tileType;
            this.position = position;
        }
    }

    public class StoneBridge
    {
        private List<StructureTileType> topBaseTiles;
        private List<StructureTileType> bottomBaseTiles;
        private List<StructureTileType> topRailingTiles;
        private List<StructureTileType> bottomRailingTiles;
        private List<StructureTileType> topPillarTiles;
        private List<StructureTileType> bottomPillarTiles;
        private List<StructureTileType> supportTiles;
        private Texture2D tileAtlas;
        private int tileSize;
        private Random random;

        private static readonly int[] possibleX = { 0, 16, 32, 48, 64, 80, 96, 112 };
        private static readonly int[] possibleY = { 160, 176 };
        private static readonly int[] railingX = { 16, 32, 48, 64, 80, 96 };
        private static readonly int[] railingY = { 144 };
        private static readonly int[] railingBottomX = { 16, 32, 48, 64, 80, 96 };
        private static readonly int[] railingBottomY = { 208 };
        private static readonly int[] pillarX = { 0, 112 };
        private static readonly int[] pillarY = { 144 };

        private const int tileSpacing = 48;
        private const int substractOffset = 10;
        private const float scale = 3.0f;

        public StoneBridge(Texture2D tileAtlas, int seed)
        {
            this.tileAtlas = tileAtlas;
            this.tileSize = 16;
            this.random = new Random(seed);
            topBaseTiles = new List<StructureTileType>(200);
            bottomBaseTiles = new List<StructureTileType>(200);
            topRailingTiles = new List<StructureTileType>(100);
            bottomRailingTiles = new List<StructureTileType>(100);
            topPillarTiles = new List<StructureTileType>(40);
            bottomPillarTiles = new List<StructureTileType>(40);
            supportTiles = new List<StructureTileType>(100);
        }

        public void Generate(int startX, int startY, int length)
        {
            int railingTopY = startY - tileSpacing;
            int baseTopY = startY;
            int baseBottomY = startY + tileSpacing;
            int railingBottomYPos = startY + tileSpacing * 2 - substractOffset;
            int supportY = startY + tileSpacing * 3 - substractOffset;
            Rectangle supportTextureRegion = new Rectangle(0, 224, 128, 48);

            for (int i = 0; i < length; i++)
            {
                int currentX = startX + i * tileSpacing;
                int nextX = currentX + tileSpacing;
                Vector2 railingTopPosition = new Vector2(currentX, railingTopY);
                Vector2 baseTopPosition = new Vector2(currentX, baseTopY);
                Vector2 baseBottomPosition = new Vector2(currentX, baseBottomY);
                Vector2 railingBottomPosition = new Vector2(currentX, railingBottomYPos);
                Vector2 supportPosition = new Vector2(nextX, supportY);

                int randRailingX = railingX[random.Next(railingX.Length)];
                int randRailingY = railingY[random.Next(railingY.Length)];
                topRailingTiles.Add(new StructureTileType(
                    new TileType("RailingTop", false, Color.White,
                        new List<WeightedVariant> { new WeightedVariant(new Rectangle(randRailingX, randRailingY, tileSize, tileSize), 1) }),
                    railingTopPosition));

                int randPossibleX1 = possibleX[random.Next(possibleX.Length)];
                int randPossibleY1 = possibleY[random.Next(possibleY.Length)];
                topBaseTiles.Add(new StructureTileType(
                    new TileType("Base", true, Color.White,
                        new List<WeightedVariant> { new WeightedVariant(new Rectangle(randPossibleX1, randPossibleY1, tileSize, tileSize), 1) }),
                    baseTopPosition));

                int randPossibleX2 = possibleX[random.Next(possibleX.Length)];
                int randPossibleY2 = possibleY[random.Next(possibleY.Length)];
                bottomBaseTiles.Add(new StructureTileType(
                    new TileType("Base", true, Color.White,
                        new List<WeightedVariant> { new WeightedVariant(new Rectangle(randPossibleX2, randPossibleY2, tileSize, tileSize), 1) }),
                    baseBottomPosition));

                int randRailingBottomX = railingBottomX[random.Next(railingBottomX.Length)];
                int randRailingBottomY = railingBottomY[random.Next(railingBottomY.Length)];
                bottomRailingTiles.Add(new StructureTileType(
                    new TileType("RailingBottom", false, Color.White,
                        new List<WeightedVariant> { new WeightedVariant(new Rectangle(randRailingBottomX, randRailingBottomY, tileSize, tileSize), 1) }),
                    railingBottomPosition));

                if (i % 8 == 0)
                {
                    Vector2 pillarTopBaseLeft = new Vector2(currentX, railingTopY);
                    Vector2 pillarTopBaseRight = new Vector2(nextX, railingTopY);
                    Vector2 pillarTopTopLeft = new Vector2(currentX, railingTopY - tileSpacing);
                    Vector2 pillarTopTopRight = new Vector2(nextX, railingTopY - tileSpacing);
                    topPillarTiles.Add(new StructureTileType(
                        new TileType("Pillar", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(pillarX[0], pillarY[0], tileSize, tileSize), 1) }),
                        pillarTopBaseRight));
                    topPillarTiles.Add(new StructureTileType(
                        new TileType("Pillar", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(pillarX[1], pillarY[0], tileSize, tileSize), 1) }),
                        pillarTopBaseLeft));
                    topPillarTiles.Add(new StructureTileType(
                        new TileType("PillarTop", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(pillarX[0], pillarY[0] - 16, tileSize, tileSize), 1) }),
                        pillarTopTopRight));
                    topPillarTiles.Add(new StructureTileType(
                        new TileType("PillarTop", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(pillarX[1], pillarY[0] - 16, tileSize, tileSize), 1) }),
                        pillarTopTopLeft));
                    supportTiles.Add(new StructureTileType(
                        new TileType("BridgeSupport", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(supportTextureRegion, 1) }),
                        supportPosition));

                    int pillarBottomUpperY = railingBottomYPos;
                    int pillarBottomLowerY = startY + tileSpacing - substractOffset;
                    Vector2 pillarBottomBaseLeft = new Vector2(currentX, pillarBottomUpperY);
                    Vector2 pillarBottomBaseRight = new Vector2(nextX, pillarBottomUpperY);
                    Vector2 pillarBottomLowerLeft = new Vector2(currentX, pillarBottomLowerY);
                    Vector2 pillarBottomLowerRight = new Vector2(nextX, pillarBottomLowerY);
                    bottomPillarTiles.Add(new StructureTileType(
                        new TileType("Pillar", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(pillarX[0], pillarY[0], tileSize, tileSize), 1) }),
                        pillarBottomBaseRight));
                    bottomPillarTiles.Add(new StructureTileType(
                        new TileType("Pillar", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(pillarX[1], pillarY[0], tileSize, tileSize), 1) }),
                        pillarBottomBaseLeft));
                    bottomPillarTiles.Add(new StructureTileType(
                        new TileType("PillarBottom", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(pillarX[0], pillarY[0] - 16, tileSize, tileSize), 1) }),
                        pillarBottomLowerRight));
                    bottomPillarTiles.Add(new StructureTileType(
                        new TileType("PillarBottom", true, Color.White,
                            new List<WeightedVariant> { new WeightedVariant(new Rectangle(pillarX[1], pillarY[0] - 16, tileSize, tileSize), 1) }),
                        pillarBottomLowerLeft));
                }
            }
        }

        public void DrawBackgroundInRect(SpriteBatch spriteBatch, Rectangle rect)
        {
            foreach (var tile in supportTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.tileType.GetEffectiveTextureRegion((int)tile.position.X, (int)tile.position.Y);
                    spriteBatch.Draw(tileAtlas, tile.position, region, tile.tileType.Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in topBaseTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.tileType.GetEffectiveTextureRegion((int)tile.position.X, (int)tile.position.Y);
                    spriteBatch.Draw(tileAtlas, tile.position, region, tile.tileType.Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in bottomBaseTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.tileType.GetEffectiveTextureRegion((int)tile.position.X, (int)tile.position.Y);
                    spriteBatch.Draw(tileAtlas, tile.position, region, tile.tileType.Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in topRailingTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.tileType.GetEffectiveTextureRegion((int)tile.position.X, (int)tile.position.Y);
                    spriteBatch.Draw(tileAtlas, tile.position, region, tile.tileType.Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in topPillarTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.tileType.GetEffectiveTextureRegion((int)tile.position.X, (int)tile.position.Y);
                    spriteBatch.Draw(tileAtlas, tile.position, region, tile.tileType.Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
        }

        public void DrawForegroundInRect(SpriteBatch spriteBatch, Rectangle rect)
        {
            foreach (var tile in bottomRailingTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.tileType.GetEffectiveTextureRegion((int)tile.position.X, (int)tile.position.Y);
                    spriteBatch.Draw(tileAtlas, tile.position, region, tile.tileType.Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
            foreach (var tile in bottomPillarTiles)
            {
                Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, 48, 48);
                if (tileRect.Intersects(rect))
                {
                    Rectangle region = tile.tileType.GetEffectiveTextureRegion((int)tile.position.X, (int)tile.position.Y);
                    spriteBatch.Draw(tileAtlas, tile.position, region, tile.tileType.Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
        }

        public List<Rectangle> GetBottomRailingColliders()
        {
            List<Rectangle> colliders = new List<Rectangle>();
            int colliderWidth = 48;
            int colliderHeight = 48;

            foreach (var tile in bottomRailingTiles)
            {
                Rectangle collider = new Rectangle((int)tile.position.X, (int)tile.position.Y + 48, colliderWidth, colliderHeight);
                colliders.Add(collider);
            }
            return colliders;
        }
    }
}
