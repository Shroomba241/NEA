using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using CompSci_NEA.Core;
using CompSci_NEA.GUI;
using CompSci_NEA.WorldGeneration.Structures;

namespace CompSci_NEA.Scenes
{
    public class ShopMenu : Scene
    {
        private Main game;
        private GameSave save;
        private SpriteFont _font;
        private Text _titleText;
        private Button _confirmButton;
        private Button _cancelButton;
        private List<ShopItem> _items;
        private Rectangle[] _itemCells;
        private const int GRIDCOLUMNS = 3;
        private const int GRIDROWS = 2;
        private const int CELLWIDTH = 400;
        private const int CELLHEIGHT = 400;
        private Vector2 _gridOrigin = new Vector2(100, 150);
        private bool _isConfirming = false;
        private ShopItem _selectedItem = null;
        private int _selectedIndex = -1;

        public ShopMenu(Main game, GameSave save)
        {
            this.game = game;
            this.save = save;
        }

        public override void LoadContent()
        {
            _font = game.Content.Load<SpriteFont>("DefaultFont");
            _titleText = new Text(_font, "Shop Menu", new Vector2(100, 50), Color.White, 3.0f);

            _confirmButton = new Button(game.GraphicsDevice, _font, "Confirm", new Vector2(440, 900), 400, 90);
            _confirmButton.OnClickAction = ConfirmPurchase;
            _cancelButton = new Button(game.GraphicsDevice, _font, "Cancel", new Vector2(1080, 900), 400, 90);
            _cancelButton.OnClickAction = CancelPurchase;

            _items = new List<ShopItem>();
            for (int i = 0; i < GRIDCOLUMNS * GRIDROWS; i++)
            {
                _items.Add(null);
            }

            if (save.ShopInventory != null)
            {
                if (save.ShopInventory.PendingItems != null && save.ShopInventory.PendingItems.Count > 0)
                {
                    save.ShopInventory.AvailableItems.AddRange(save.ShopInventory.PendingItems);
                    save.ShopInventory.PendingItems.Clear();
                }
            }

            List<ShopItem> shopItems = new List<ShopItem>();
            if (save.ShopInventory != null && save.ShopInventory.AvailableItems != null)
            {
                foreach (var shopItemSave in save.ShopInventory.AvailableItems)
                {
                    Texture2D icon = GetIconFromName(shopItemSave.Icon);
                    ShopItem newItem = new ShopItem(shopItemSave.Name, shopItemSave.Price, icon, -1);
                    shopItems.Add(newItem);
                }
            }
            else
            {
                shopItems = new List<ShopItem>
                {
                    new ShopItem("Health Potion", 50, TextureManager.Shmacks, 1),
                    new ShopItem("Mana Potion", 75, TextureManager.Shmacks, 2),
                    new ShopItem("Sword", 150, TextureManager.Shmacks, 3)
                };
            }

            AddShopItems(shopItems);

            _itemCells = new Rectangle[GRIDCOLUMNS * GRIDROWS];
            for (int row = 0; row < GRIDROWS; row++)
            {
                for (int col = 0; col < GRIDCOLUMNS; col++)
                {
                    int index = row * GRIDCOLUMNS + col;
                    int x = (int)(_gridOrigin.X + col * CELLWIDTH);
                    int y = (int)(_gridOrigin.Y + row * CELLHEIGHT);
                    _itemCells[index] = new Rectangle(x, y, CELLWIDTH, CELLHEIGHT);
                }
            }
        }

        private Texture2D GetIconFromName(string iconName)
        {
            switch (iconName)
            {
                case "Shmack":
                    return TextureManager.Shmacks;
                default:
                    return TextureManager.Shmacks;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                game.CloseMiniGame(0);

            if (_isConfirming)
            {
                _confirmButton.Update();
                _cancelButton.Update();
            }
            else
            {
                MouseState ms = Mouse.GetState();
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    for (int i = 0; i < _itemCells.Length; i++)
                    {
                        if (_itemCells[i].Contains(ms.X, ms.Y))
                        {
                            if (i < _items.Count && _items[i] != null)
                            {
                                _selectedItem = _items[i];
                                _selectedIndex = i;
                                _isConfirming = true;
                            }
                            break;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            game.GraphicsDevice.Clear(Color.Gray);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _titleText.Draw(spriteBatch);

            if (_isConfirming && _selectedItem != null)
            {
                Rectangle overlayRect = new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
                spriteBatch.Draw(TextureManager.MainMenuBG, overlayRect, new Color(0, 0, 0, 150));

                float confirmTextScale = 3.0f;
                string confirmMsg = $"Buy {_selectedItem.Name}?";
                Vector2 msgSize = _font.MeasureString(confirmMsg) * confirmTextScale;
                Vector2 msgPos = new Vector2((overlayRect.Width - msgSize.X) / 2, 100);
                spriteBatch.DrawString(_font, confirmMsg, msgPos, Color.White, 0f, Vector2.Zero, confirmTextScale, SpriteEffects.None, 0f);

                int iconWidth = 300;
                int iconHeight = 300;
                int iconX = (overlayRect.Width - iconWidth) / 2;
                int iconY = (int)msgPos.Y + (int)msgSize.Y + 20;
                Rectangle bigIconRect = new Rectangle(iconX, iconY, iconWidth, iconHeight);
                spriteBatch.Draw(_selectedItem.Icon, bigIconRect, Color.White);

                _confirmButton.Draw(spriteBatch);
                _cancelButton.Draw(spriteBatch);
            }
            else
            {
                for (int i = 0; i < _itemCells.Length; i++)
                {
                    Rectangle cell = _itemCells[i];

                    if (i < _items.Count && _items[i] != null)
                    {
                        ShopItem item = _items[i];
                        int iconWidth = (int)(cell.Width * 0.7);
                        int iconHeight = (int)(cell.Height * 0.7);
                        int iconX = cell.X + (cell.Width - iconWidth) / 2;
                        int iconY = cell.Y + (int)(cell.Height * 0.1);
                        Rectangle iconRect = new Rectangle(iconX, iconY, iconWidth, iconHeight);
                        spriteBatch.Draw(item.Icon, iconRect, Color.White);

                        string priceText = $"{item.Price} coins";
                        float textScale = 2.0f;
                        Vector2 textSize = _font.MeasureString(priceText) * textScale;
                        int textX = cell.X + (cell.Width - (int)textSize.X) / 2;
                        int textY = iconY + iconHeight + 5;
                        spriteBatch.DrawString(_font, priceText, new Vector2(textX, textY), Color.Black, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    }
                    else
                    {
                        string emptyText = "Empty";
                        Vector2 emptyTextSize = _font.MeasureString(emptyText);
                        int textX = cell.X + (cell.Width - (int)emptyTextSize.X) / 2;
                        int textY = cell.Y + (cell.Height - (int)emptyTextSize.Y) / 2;
                        spriteBatch.DrawString(_font, emptyText, new Vector2(textX, textY), Color.White);
                    }
                }
            }
            spriteBatch.End();
        }

        private void ConfirmPurchase()
        {
            if (_selectedItem != null)
            {
                _items[_selectedIndex] = null;
                var matchingItem = save.ShopInventory.AvailableItems.Find(i => i.Name == _selectedItem.Name && i.Price == _selectedItem.Price && i.Icon == "Shmack");
                if (matchingItem != null)
                {
                    save.ShopInventory.AvailableItems.Remove(matchingItem);
                }
            }
            _isConfirming = false;
            _selectedItem = null;
            _selectedIndex = -1;
        }

        private void CancelPurchase()
        {
            _isConfirming = false;
            _selectedItem = null;
            _selectedIndex = -1;
        }

        public void AddShopItem(ShopItem newItem)
        {
            List<int> emptyIndices = new List<int>();
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == null)
                    emptyIndices.Add(i);
            }
            if (emptyIndices.Count == 0)
            {
                throw new Exception("no shop item slot availible");
            }
            Random rand = new Random();
            int index = emptyIndices[rand.Next(emptyIndices.Count)];
            _items[index] = newItem;
        }

        public void AddShopItems(List<ShopItem> newItems)
        {
            foreach (var item in newItems)
            {
                AddShopItem(item);
            }
        }

        public override void Shutdown()
        {
        }
    }
}
