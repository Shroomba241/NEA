using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.GUI
{
    public class ShopItem
    {
        public string Name;
        public int Price;
        public Texture2D Icon;
        public int GridPosition;

        public ShopItem(string name, int price, Texture2D icon, int gridPosition)
        {
            Name = name;
            Price = price;
            Icon = icon;
            GridPosition = gridPosition;
        }
    }
}
