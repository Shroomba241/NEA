using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using CompSci_NEA.Core;
using CompSci_NEA.GUI;
using CompSci_NEA.WorldGeneration.Structures;

namespace CompSci_NEA.Scenes
{
    public class ShopMenu : Scene
    {
        private Main game;
        private SpriteFont font;
        private Text titleText;
        private Button confirmButton;
        private Button cancelButton;
        private List<ShopItem> items;
        private int selectedIndex = -1;
        private Rectangle[] itemCells;
        private const int gridColumns = 2;
        private const int gridRows = 3;
        private const int cellWidth = 200;
        private const int cellHeight = 100;
        private Vector2 gridOrigin = new Vector2(100, 150);

        public ShopMenu(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            font = game.Content.Load<SpriteFont>("DefaultFont");
            titleText = new Text(font, "Shop Menu", new Vector2(100, 50), Color.White, 3.0f);
            confirmButton = new Button(game.GraphicsDevice, font, "Confirm Purchase", new Vector2(100, 400), 200, 50);
            confirmButton.OnClickAction = ConfirmPurchase;
            cancelButton = new Button(game.GraphicsDevice, font, "Cancel", new Vector2(320, 400), 200, 50);
            cancelButton.OnClickAction = () => game.CloseMiniGame(0);

            // Create 6 items and assign grid positions (1-6)
            items = new List<ShopItem>
            {
                new ShopItem("Health Potion", 50, TextureManager.Shmacks, 1),
                new ShopItem("Mana Potion", 75, TextureManager.Shmacks, 2),
                new ShopItem("Sword", 150, TextureManager.Shmacks, 3),
                new ShopItem("Shield", 200, TextureManager.Shmacks, 4),
                new ShopItem("Armor", 300, TextureManager.Shmacks, 5),
                new ShopItem("Boots", 100, TextureManager.Shmacks, 6)
            };

            itemCells = new Rectangle[6];
            for (int row = 0; row < gridRows; row++)
            {
                for (int col = 0; col < gridColumns; col++)
                {
                    int index = row * gridColumns + col;
                    int x = (int)(gridOrigin.X + col * cellWidth);
                    int y = (int)(gridOrigin.Y + row * cellHeight);
                    itemCells[index] = new Rectangle(x, y, cellWidth, cellHeight);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            confirmButton.Update();
            cancelButton.Update();

            MouseState ms = Mouse.GetState();
            // If mouse left button is pressed, check which cell is hovered.
            for (int i = 0; i < itemCells.Length; i++)
            {
                if (itemCells[i].Contains(ms.X, ms.Y))
                {
                    selectedIndex = i;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                cancelButton.OnClickAction();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            titleText.Draw(spriteBatch);
            for (int i = 0; i < items.Count; i++)
            {
                ShopItem item = items[i];
                Rectangle cell = itemCells[i];
                Color cellColor = (i == selectedIndex) ? Color.LightBlue : Color.White;
                spriteBatch.Draw(TextureManager.MainMenuBG, cell, cellColor);
                // Display grid number, item name, and price
                string displayText = $"{item.GridPosition}. {item.Name}\n{item.Price} coins";
                Vector2 textSize = font.MeasureString(displayText);
                Vector2 textPos = new Vector2(cell.X + (cell.Width - textSize.X) / 2, cell.Y + (cell.Height - textSize.Y) / 2);
                spriteBatch.DrawString(font, displayText, textPos, Color.Black);
            }
            confirmButton.Draw(spriteBatch);
            cancelButton.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void ConfirmPurchase()
        {
            if (selectedIndex >= 0 && selectedIndex < items.Count)
            {
                ShopItem selectedItem = items[selectedIndex];
                System.Console.WriteLine($"Purchased {selectedItem.Name} for {selectedItem.Price} coins.");
                // Here you would deduct coins and add the item to the player's inventory.
            }
        }

        public override void Shutdown()
        {
        }
    }
}
