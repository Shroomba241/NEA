using CompSci_NEA.GUI;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CompSci_NEA.Core
{
    public class GameSave
    {
        public string Slot {  get; set; }
        public ShopSave ShopInventory { get; set; }
        public InventorySave PlayerInventory { get; set; }
        public ExistingMinigames Minigames { get; set; }

        public static GameSave CreateDefaultSave(string slot)
        {
            return new GameSave
            {
                Slot = $"slot{slot}.json",
                ShopInventory = new ShopSave
                {
                    AvailableItems = new List<ShopItemSave>
                    {
                        new ShopItemSave { Name = "Map", Price = 5, Icon = "Shmack" },
                        new ShopItemSave { Name = "Rubbish", Price = 500, Icon = "Shmack" }
                    },
                    PendingItems = new List<ShopItemSave>()
                },
                PlayerInventory = new InventorySave
                {
                    Items = new List<int>()
                },
                Minigames = new ExistingMinigames
                {
                    Minigames = new List<MinigameInfo>()
                }
            };
        }
    }

    public class ShopSave
    {
        public List<ShopItemSave> AvailableItems { get; set; }
        public List<ShopItemSave> PendingItems { get; set; }
    }

    public class InventorySave
    {
        public List<int> Items { get; set; }
    }

    public class ExistingMinigames
    {
        public List<MinigameInfo> Minigames { get; set; }
    }

    public class MinigameInfo
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string SubGameType { get; set; }
    }

    public class ShopItemSave
    {
        public string Name { get; set; } 
        public int Price { get; set; }
        public string Icon { get; set; }
    }
}
