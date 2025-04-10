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
        public int PodiumGoalRemaining { get; set; }
        public string Slot {  get; set; }
        public ShopSave ShopInventory { get; set; }
        public InventorySave PlayerInventory { get; set; }
        public ExistingMinigames Minigames { get; set; }

        public static GameSave CreateDefaultSave(string slot)
        {
            return new GameSave
            {
                Slot = $"slot{slot}.json",
                PodiumGoalRemaining = 9999,
                ShopInventory = new ShopSave
                {
                    BoughtItems = new List<ShopItemSave>(),
                    AvailableItems = new List<ShopItemSave>
                    {
                        new ShopItemSave { Name = "Map", Price = 5, Icon = "Shmack" },
                        new ShopItemSave { Name = "Key 1", Price = 15, Icon = "Padlock" }
                    },
                    PendingItems = new List<ShopItemSave>
                    {
                        new ShopItemSave { Name = "Key 2", Price = 30, Icon = "Padlock" }
                        //new ShopItemSave { Name = "Dash Boots", Price = 15, Icon = "Padlock" }
                    }
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
        public List<ShopItemSave> BoughtItems { get; set; }
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
