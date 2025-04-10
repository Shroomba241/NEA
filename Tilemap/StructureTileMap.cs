using CompSci_NEA.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using CompSci_NEA.WorldGeneration.Structures;
using CompSci_NEA.WorldGeneration;
using System;

namespace CompSci_NEA.Tilemap
{
    public class StructureTileMap : BaseTileMap
    {
        private List<Structure> _structures;
        public Shop Shop;
        public Podium Podium;
        public List<Structure> Bridges;

        public StructureTileMap(GraphicsDevice graphicsDevice, int totalChunksX, int totalChunksY, int seed)
            : base(graphicsDevice, totalChunksX, totalChunksY, seed)
        {
            tileAtlas = TextureManager.ATLAS;
            _structures = new List<Structure>();

            StoneBridge stoneBridge = new StoneBridge(tileAtlas, seed);
            stoneBridge.Generate(400 * 48, 400 * 48, 100);
            _structures.Add(stoneBridge);

            Shop = new Shop(tileAtlas, new Vector2(390 * 48, 400 * 48));
            _structures.Add(Shop);

            Podium = new Podium(tileAtlas, new Vector2(390 * 48, 450 * 48));
            _structures.Add(Podium);

            StructureGenerator sg = new StructureGenerator();
            Bridges = sg.GenerateWoodBridges(this, tileAtlas);
            _structures.AddRange(Bridges);

            
        }

        public override byte GenerateTile(int x, int y)
        {
            float totalWidth = totalChunksX * chunkSize;
            float totalHeight = totalChunksY * chunkSize;
            float centerX = totalWidth * 0.5f;
            float centerY = totalHeight * 0.5f;
            float scaleFactor = totalChunksX / 16f;
            float normX = (x - centerX) / (totalWidth * 0.35f);
            float normY = (y - centerY) / (totalHeight * 0.25f);
            float baseShape = (normX * normX) + (normY * normY);
            baseShape -= NoiseGenerator.Generate(x * 0.005f, y * 0.005f) * 0.3f;
            float largeNoise = NoiseGenerator.Generate(x * 0.0067f, y * 0.0067f, 3, 50f, 0.5f, 2.5f);
            float mediumNoise = NoiseGenerator.Generate(x * 0.025f, y * 0.025f, 4, 20f, 0.6f, 2.2f);
            float smallNoise = NoiseGenerator.Generate(x * 0.1f, y * 0.1f, 5, 10f, 0.8f, 2.0f);
            float coastlineDistortion = (largeNoise * 0.4f) + (mediumNoise * 0.3f) + (smallNoise * 0.6f);
            float finalShape = baseShape - coastlineDistortion;
            float landThreshold = 0.9f - (largeNoise * 0.1f) + (mediumNoise * 0.05f);

            float t = y / totalHeight;
            float leftRiverCenter = MathHelper.Lerp(centerX - 110f * scaleFactor, centerX - 150f * scaleFactor, t) +
                                    ((NoiseGenerator.Generate(1000, y * 0.01f) - 0.5f) * 2 * totalWidth * 0.05f);

            float rightRiverCenter = MathHelper.Lerp(centerX + 200f * scaleFactor, centerX + 400f * scaleFactor, t) -
                                     (100f * scaleFactor * 4 * t * (1 - t)) -
                                     ((NoiseGenerator.Generate(1000, y * 0.01f) - 0.5f) * 2 * totalWidth * 0.05f);

            if (MathF.Abs(x - leftRiverCenter) < 5f || MathF.Abs(x - rightRiverCenter) < 5f)
                return 2;

            return finalShape < landThreshold
                ? (x < leftRiverCenter ? (byte)3 : x > rightRiverCenter ? (byte)4 : (byte)5)
                : (byte)2;
        }

        public void DrawBackgroundLayer(SpriteBatch spriteBatch, Entities.Player player)
        {
            int chunkX = (int)(player.Position.X / (48 * chunkSize));
            int chunkY = (int)(player.Position.Y / (48 * chunkSize));
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    int currentChunkX = chunkX + offsetX;
                    int currentChunkY = chunkY + offsetY;
                    Rectangle chunkRect = new Rectangle(
                        currentChunkX * chunkSize * 48,
                        currentChunkY * chunkSize * 48,
                        chunkSize * 48,
                        chunkSize * 48);

                    foreach (var structure in _structures)
                    {
                        structure.DrawBackgroundInRect(spriteBatch, chunkRect);
                    }
                }
            }
        }

        public void DrawForegroundLayer(SpriteBatch spriteBatch, Entities.Player player)
        {
            int chunkX = (int)(player.Position.X / (48 * chunkSize));
            int chunkY = (int)(player.Position.Y / (48 * chunkSize));
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    int currentChunkX = chunkX + offsetX;
                    int currentChunkY = chunkY + offsetY;
                    Rectangle chunkRect = new Rectangle(
                        currentChunkX * chunkSize * 48,
                        currentChunkY * chunkSize * 48,
                        chunkSize * 48,
                        chunkSize * 48);

                    foreach (var structure in _structures)
                    {
                        structure.DrawForegroundInRect(spriteBatch, chunkRect);
                    }
                }
            }
        }

        public List<Rectangle> GetAllColliders()
        {
            List<Rectangle> colliders = new List<Rectangle>();
            foreach (var structure in _structures)
            {
                colliders.AddRange(structure.GetColliders());
            }
            return colliders;
        }
    }
}
