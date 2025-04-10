using CompSci_NEA.Tilemap;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.WorldGeneration.Structures
{
    public class StructureGenerator
    {
        public List<Structure> GenerateWoodBridges(BaseTileMap tilemap, Texture2D woodBridgeAtlas)
        {
            List<Structure> bridges = new List<Structure>();
            int tileSize = 48;
            int worldWidthTiles = 24 * 64;
            int worldHeightTiles = 18 * 64;
            int middleY = worldHeightTiles / 2;
            int margin = 100;
            int requiredRiverWidth = 4; 
            int currentRiverStart = -1;
            int riverLength = 0;

            for (int x = margin; x < worldWidthTiles - margin; x++)
            {
                byte tile = tilemap.GetTile(x, middleY);
                if (tile == 2) 
                {
                    if (currentRiverStart == -1)
                        currentRiverStart = x;
                    riverLength++;
                }
                else
                {
                    if (riverLength >= requiredRiverWidth)
                    {
                        int midX = currentRiverStart + riverLength / 2;
                        Vector2 bridgePosition = new Vector2(midX * tileSize - 288, middleY * tileSize);
                        bridges.Add(new WoodBridge(woodBridgeAtlas, bridgePosition));
                    }
                    currentRiverStart = -1;
                    riverLength = 0;
                }
            }

            if (riverLength >= requiredRiverWidth)
            {
                int midX = currentRiverStart + riverLength / 2;
                Vector2 bridgePosition = new Vector2(midX * tileSize - 240, middleY * tileSize);
                bridges.Add(new WoodBridge(woodBridgeAtlas, bridgePosition));
            }

            return bridges;
        }
    }
}
