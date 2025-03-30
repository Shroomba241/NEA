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
        private bool _loaded = false;
        private SpriteFont _font;
        private GUI.Button _continueButton;
        private GUI.Button _logoutButton;
        private GUI.Text _titleText;
        private GUI.DataTable _userDataTable;
        private GUI.DataTable _tetrisDataTable;
        private GUI.InputBox _editInputBox;
        private bool _editModeActive = false;
        private GUI.Button _toggleButton;
        private GUI.Button _deleteUserButton;
        private GUI.Button _deleteSessionButton;

        private int _currentTetrisUserId = -1;
        private bool _tetrisLocked = false;
        private bool _showTetrisTable = false;

        private RenderTarget2D _renderTarget;
        private Effect _crtEffect;

        public AdminView(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            _loaded = true;
            _font = game.Content.Load<SpriteFont>("DefaultFont");

            string adminName = Main.LoggedInUsername ?? "Admin";
            _titleText = new GUI.Text(_font, $"Admin Panel - {adminName}", new Vector2(100, 50), Color.White, 3.0f);

            _continueButton = new GUI.Button(game.GraphicsDevice, _font, "Continue", new Vector2(520, 900), 420, 90);
            _logoutButton = new GUI.Button(game.GraphicsDevice, _font, "Logout", new Vector2(100, 900), 400, 90);
            _continueButton.OnClickAction = () => game.ChangeState(GameState.MainMenu);
            _logoutButton.OnClickAction = () => game.ChangeState(GameState.Login);

            Database.DbFunctions dbFunctions = new Database.DbFunctions();
            List<string[]> userRows = dbFunctions.GetAllUserData();
            string[] userHeaders = new string[] { "User ID", "Username", "Admin", "Coins" };
            _userDataTable = new GUI.DataTable(game.GraphicsDevice, _font, new Vector2(100, 200), userHeaders, userRows);

            _tetrisDataTable = null;
            _showTetrisTable = false;
            _currentTetrisUserId = -1;
            _tetrisLocked = false;

            _editInputBox = new GUI.InputBox(game.GraphicsDevice, _font, new Vector2(-500, -500), 800, 90, false, 15);
            _toggleButton = new GUI.Button(game.GraphicsDevice, _font, "Toggle", new Vector2(-500, -500), 400, 90);
            _deleteUserButton = new GUI.Button(game.GraphicsDevice, _font, "Delete", new Vector2(-500, -500), 400, 90);
            _deleteSessionButton = new GUI.Button(game.GraphicsDevice, _font, "Delete", new Vector2(-500, -500), 400, 90);

            _renderTarget = new RenderTarget2D(game.GraphicsDevice,
                game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                game.GraphicsDevice.PresentationParameters.BackBufferHeight);
            _crtEffect = game.Content.Load<Effect>("CRTEffect");

            game.pauseCurrentSceneUpdating = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!_loaded) return;
            try
            {
                _userDataTable.Update();
                if (_showTetrisTable && _tetrisDataTable != null)
                    _tetrisDataTable.Update();
                _continueButton.Update();
                _logoutButton.Update();
                _editInputBox.Update(gameTime);
                _toggleButton.Update();
                _deleteUserButton.Update();
                _deleteSessionButton.Update();

                string adminName = Main.LoggedInUsername ?? "Admin";
                _titleText.UpdateContent($"Admin Panel - {adminName}");

                _userDataTable.IgnoreMouseClicks = _editModeActive;

                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    _tetrisLocked = false;
                    _showTetrisTable = false;
                }

                if (!_editModeActive)
                {
                    var userSel = _userDataTable.GetSelectedCell();
                    if (userSel.HasValue && userSel.Value.col == 0)
                    {
                        string userIdStr = _userDataTable.GetSelectedCellValue();
                        if (int.TryParse(userIdStr, out int userId))
                        {
                            if (!_tetrisLocked || userId != _currentTetrisUserId)
                            {
                                _currentTetrisUserId = userId;
                                Database.DbFunctions dbFunctions = new Database.DbFunctions();
                                List<string[]> tetrisRows = dbFunctions.GetTetrisData(userId);
                                string[] tetrisHeaders = new string[] { "Session ID", "User ID", "Username", "Score" };
                                _tetrisDataTable = new GUI.DataTable(game.GraphicsDevice, _font, new Vector2(1000, 200), tetrisHeaders, tetrisRows);
                                _tetrisLocked = true;
                            }
                            _showTetrisTable = true;

                            _deleteUserButton.Move(new Vector2(100, 600));
                            _deleteUserButton.OnClickAction = () =>
                            {
                                Database.DbFunctions dbFuncs = new Database.DbFunctions();
                                dbFuncs.DeleteUser(userId);
                                List<string[]> newUserRows = dbFuncs.GetAllUserData();
                                _userDataTable = new GUI.DataTable(game.GraphicsDevice, _font, new Vector2(100, 200),
                                    new string[] { "User ID", "Username", "Admin", "Coins" }, newUserRows);
                                _tetrisDataTable = null;
                                _showTetrisTable = false;
                                _tetrisLocked = false;
                                _currentTetrisUserId = -1;
                                _deleteUserButton.Move(new Vector2(-500, -500));
                            };
                        }
                    }
                    else
                    {
                        _deleteUserButton.Move(new Vector2(-500, -500));
                    }

                    if (userSel.HasValue)
                    {
                        if (userSel.Value.col == 2)
                        {
                            _editModeActive = true;
                            _toggleButton.Move(new Vector2(100, 700));
                            _toggleButton.OnClickAction = () =>
                            {
                                string currentValue = _userDataTable.GetSelectedCellValue().Trim().ToLower();
                                string newValue = (currentValue == "0" || currentValue == "false") ? "1" : "0";
                                _userDataTable.SetSelectedCellValue(newValue);
                                _userDataTable.ClearSelectedCell();
                                _toggleButton.Move(new Vector2(-500, -500));
                                _editModeActive = false;
                            };
                        }
                        else if (userSel.Value.col != 0)
                        {
                            _editModeActive = true;
                            _editInputBox.Move(new Vector2(100, 700));
                            string currentValue = _userDataTable.GetSelectedCellValue();
                            _editInputBox.SetText(currentValue);
                        }
                    }

                    if (_showTetrisTable && _tetrisDataTable != null)
                    {
                        var tetrisSel = _tetrisDataTable.GetSelectedCell();
                        if (tetrisSel.HasValue && tetrisSel.Value.col == 0)
                        {
                            string sessionIdStr = _tetrisDataTable.GetSelectedCellValue();
                            if (int.TryParse(sessionIdStr, out int sessionId))
                            {
                                _deleteSessionButton.Move(new Vector2(1000, 600));
                                _deleteSessionButton.OnClickAction = () =>
                                {
                                    Database.DbFunctions dbFuncs = new Database.DbFunctions();
                                    dbFuncs.DeleteTetrisSession(sessionId);
                                    List<string[]> newTetrisRows = dbFuncs.GetTetrisData(_currentTetrisUserId);
                                    string[] tetrisHeaders = new string[] { "Session ID", "User ID", "Username", "Score" };
                                    _tetrisDataTable = new GUI.DataTable(game.GraphicsDevice, _font, new Vector2(1000, 200), tetrisHeaders, newTetrisRows);
                                    _deleteSessionButton.Move(new Vector2(-500, -500));
                                };

                            }
                        }
                        else
                        {
                            _deleteSessionButton.Move(new Vector2(-500, -500));
                        }
                    }
                }

                if (_editModeActive && !_toggleButton.IsHovered)
                {
                    KeyboardState ks = Keyboard.GetState();
                    if (ks.IsKeyDown(Keys.Enter))
                    {
                        string newValue = _editInputBox.GetText();
                        _userDataTable.SetSelectedCellValue(newValue);
                        _editModeActive = false;
                        _editInputBox.Move(new Vector2(-500, -500));
                        _userDataTable.ClearSelectedCell();
                        _editInputBox.SetText("");
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
            if (!_loaded) return;

            game.GraphicsDevice.SetRenderTarget(_renderTarget);
            game.GraphicsDevice.Clear(Color.Blue);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _titleText.Draw(spriteBatch);
            _continueButton.Draw(spriteBatch);
            _logoutButton.Draw(spriteBatch);
            _userDataTable.Draw(spriteBatch);
            if (_showTetrisTable && _tetrisDataTable != null)
                _tetrisDataTable.Draw(spriteBatch);

            if (_editModeActive)
                _editInputBox.Draw(spriteBatch);
            if (_toggleButton != null)
                _toggleButton.Draw(spriteBatch);
            _deleteUserButton.Draw(spriteBatch);
            _deleteSessionButton.Draw(spriteBatch);

            spriteBatch.End();

            if (_crtEffect.Parameters["Resolution"] != null)
                _crtEffect.Parameters["Resolution"].SetValue(new Vector2(_renderTarget.Width, _renderTarget.Height));

            game.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(effect: _crtEffect, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(_renderTarget, new Rectangle(0, 0, _renderTarget.Width, _renderTarget.Height), Color.White);
            spriteBatch.End();
        }

        public override void Shutdown()
        {
            _continueButton = null;
            _logoutButton = null;
            _toggleButton = null;
            _editInputBox = null;
            _deleteUserButton = null;
            _deleteSessionButton = null;
            _userDataTable = null;
            _tetrisDataTable = null;
            _titleText = null;
        }
    }
}
