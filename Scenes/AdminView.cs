using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace CompSci_NEA.Scenes
{
    public class AdminView : Scene
    {
        private Main game;
        private bool loaded = false;
        private SpriteFont font;

        private GUI.Button continueButton;
        private GUI.Button logoutButton;
        private GUI.Text titleText;
        private GUI.DataTable userDataTable;
        private GUI.InputBox editInputBox;
        private bool editModeActive = false;
        private GUI.Button toggleButton;

        private RenderTarget2D renderTarget;
        private Effect crtEffect;

        public AdminView(Main game)
        {
            this.game = game;
        }

        public override void LoadContent() //TODO: userworldsaves and worlds data tables
        {
            loaded = true;
            font = game.Content.Load<SpriteFont>("DefaultFont");

            string adminName = game.LoggedInUsername ?? "Admin";
            titleText = new GUI.Text(font, $"Admin Panel - {adminName}", new Vector2(100, 50), Color.White, 3.0f);

            continueButton = new GUI.Button(game.GraphicsDevice, font, "Continue", new Vector2(520, 900), 420, 90);
            logoutButton = new GUI.Button(game.GraphicsDevice, font, "Logout", new Vector2(100, 900), 400, 90);
            continueButton.OnClickAction = () => game.ChangeState(GameState.DEBUG);
            logoutButton.OnClickAction = () => game.ChangeState(GameState.Login);

            Database.DbFunctions dbFunctions = new Database.DbFunctions();
            List<string[]> userRows = dbFunctions.GetAllUserData();
            string[] headers = new string[] { "User ID", "Username", "Admin", "Coins" };
            userDataTable = new GUI.DataTable(game.GraphicsDevice, font, new Vector2(100, 200), headers, userRows);

            //start offscreen and used in editing data
            editInputBox = new GUI.InputBox(game.GraphicsDevice, font, new Vector2(-500, -500), 800, 90, false, 15);
            toggleButton = new GUI.Button(game.GraphicsDevice, font, "Toggle", new Vector2(-500, -500), 100, 40, 1.0f, new Vector2(10, 10));

            renderTarget = new RenderTarget2D(game.GraphicsDevice,
                game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                game.GraphicsDevice.PresentationParameters.BackBufferHeight);
            crtEffect = game.Content.Load<Effect>("CRTEffect");

            game.pauseCurrentSceneUpdateing = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!loaded) return;
            try
            {
                userDataTable.Update();
                continueButton.Update();
                logoutButton.Update();
                editInputBox.Update(gameTime);
                toggleButton.Update();

                string adminName = game.LoggedInUsername ?? "Admin";
                titleText.UpdateContent($"Admin Panel - {adminName}");

                userDataTable.IgnoreMouseClicks = editModeActive;

                var sel = userDataTable.GetSelectedCell();
                if (sel.HasValue && !editModeActive)
                {
                    if (sel.Value.col == 2)
                    {
                        editModeActive = true;
                        toggleButton.Move(new Vector2(100, 700));
                        toggleButton.OnClickAction = () =>
                        {
                            string currentValue = userDataTable.GetSelectedCellValue().Trim().ToLower();
                            string newValue = (currentValue == "0" || currentValue == "false") ? "1" : "0";
                            userDataTable.SetSelectedCellValue(newValue);
                            userDataTable.ClearSelectedCell();
                            toggleButton.Move(new Vector2(-500, -500));
                            editModeActive = false;
                        };
                    }
                    else if (sel.Value.col != 0)
                    {
                        editModeActive = true;
                        editInputBox.Move(new Vector2(100, 700));
                        string currentValue = userDataTable.GetSelectedCellValue();
                        editInputBox.SetText(currentValue);
                    }
                }

                if (editModeActive && !toggleButton.IsHovered)
                {
                    KeyboardState ks = Keyboard.GetState();
                    if (ks.IsKeyDown(Keys.Enter))
                    {
                        string newValue = editInputBox.GetText();
                        userDataTable.SetSelectedCellValue(newValue);
                        editModeActive = false;
                        editInputBox.Move(new Vector2(-500, -500));
                        userDataTable.ClearSelectedCell();
                        editInputBox.SetText("");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!loaded) return;

            game.GraphicsDevice.SetRenderTarget(renderTarget);
            game.GraphicsDevice.Clear(Color.Blue);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            titleText.Draw(spriteBatch);
            continueButton.Draw(spriteBatch);
            logoutButton.Draw(spriteBatch);
            userDataTable.Draw(spriteBatch);

            if (editModeActive)
                editInputBox.Draw(spriteBatch);
            var selCel = userDataTable.GetSelectedCell();
            if (selCel.HasValue && selCel.Value.col == 2)
                toggleButton.Draw(spriteBatch);

            spriteBatch.End();

            if (crtEffect.Parameters["Resolution"] != null)
                crtEffect.Parameters["Resolution"].SetValue(new Vector2(renderTarget.Width, renderTarget.Height));

            game.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(effect: crtEffect, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
            spriteBatch.End();
        }

        public override void Shutdown() //TODO: look into texture GPU disposal if needed
        {
            continueButton = null;
            logoutButton = null;
            toggleButton = null;
            editInputBox = null;
            userDataTable = null;
            titleText = null;
        }
    }
}
