using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using System;
using System.IO;
using CompSci_NEA.Database;

namespace CompSci_NEA.Scenes
{
    public class MainMenu : Scene
    {
        private enum UIState
        {
            Main,
            Play
        }

        private string _userInfoText = "";
        private GUI.Text _userInfoDisplay;
        private DbFunctions _db;

        private bool _loaded = false;
        private UIState _currentState = UIState.Main;
        private Vector2 _offScreen = new Vector2(-500, -500);

        private SpriteFont _font;

        //main menu
        private GUI.Button _logoutButton;
        private GUI.Button _playButton;
        private GUI.Button _backButton;

        //save slots (not yet functional)
        private GUI.Button _slot1Button;
        private GUI.Button _slot2Button;
        private GUI.Button _slot3Button;
        private GUI.Button _slot1DeleteButton;
        private GUI.Button _slot2DeleteButton;
        private GUI.Button _slot3DeleteButton;

        //background and scene stuff
        private Texture2D _backgroundTexture;
        private Texture2D _shorelineTitle;
        private Texture2D _shorelineTitleBG;
        private MotherCloud _cloudManager;
        private Song _song;
        private float _colourChangeSpeed = 0.5f;
        private float _colourTimer = 0f;
        private Color _shorelineBGColour = Color.White;

        //blur
        private RenderTarget2D _blurTarget;
        private Effect _blurEffect;

        private string _saveDirectory = "Saves";
        private int worldID;

        private Main game;

        public MainMenu(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            _loaded = true;
            _font = game.Content.Load<SpriteFont>("DefaultFont");

            _backgroundTexture = TextureManager.MainMenuBG;
            _shorelineTitle = TextureManager.Shoreline;
            _shorelineTitleBG = TextureManager.ShorelineBG;
            _cloudManager = new MotherCloud(TextureManager.MainMenuClouds, 1920);

            _song = TextureManager.MainMenuMusic;
            MediaPlayer.Play(_song);
            MediaPlayer.Volume = 0.01f;
            MediaPlayer.IsRepeating = true;

            //main state
            _logoutButton = new GUI.Button(game.GraphicsDevice, _font, "Logout", new Vector2(100, 800), 400, 90);
            _playButton = new GUI.Button(game.GraphicsDevice, _font, "Play", new Vector2(100, 700), 400, 90);
            _backButton = new GUI.Button(game.GraphicsDevice, _font, "Back", _offScreen, 400, 90);
            _logoutButton.OnClickAction = () => { game.ChangeState(GameState.Login); };
            _playButton.OnClickAction = () => SwitchState(UIState.Play);
            _backButton.OnClickAction = () => SwitchState(UIState.Main);

            //blur
            _blurTarget = new RenderTarget2D(
                game.GraphicsDevice,
                game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                game.GraphicsDevice.PresentationParameters.BackBufferHeight);
            _blurEffect = game.Content.Load<Effect>("BlurEffect");

            //create save directory if it doesnt exist yet
            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            //save slot ui elements
            _slot1Button = new GUI.Button(game.GraphicsDevice, _font, "Slot 1", _offScreen, 1000, 80);
            _slot2Button = new GUI.Button(game.GraphicsDevice, _font, "Slot 2", _offScreen, 1000, 80);
            _slot3Button = new GUI.Button(game.GraphicsDevice, _font, "Slot 3", _offScreen, 1000, 80);

            _slot1DeleteButton = new GUI.Button(game.GraphicsDevice, _font, "Delete", _offScreen, 150, 80);
            _slot2DeleteButton = new GUI.Button(game.GraphicsDevice, _font, "Delete", _offScreen, 150, 80);
            _slot3DeleteButton = new GUI.Button(game.GraphicsDevice, _font, "Delete", _offScreen, 150, 80);

            _db = new DbFunctions();
            var allUserData = _db.GetAllUserData();
            string totalCoins = "???";
            foreach (var row in allUserData)
            {
                if (row[0] == Main.LoggedInUserID.ToString())
                {
                    totalCoins = row[3]; // users.coins
                    break;
                }
            }
            _userInfoText = $"{Main.LoggedInUsername} | Total Shmacks: {totalCoins}";

            Vector2 textSize = _font.MeasureString(_userInfoText);
            Vector2 position = new Vector2(1920 - textSize.X - 300, 1080 - textSize.Y - 30); // padding from bottom-right
            _userInfoDisplay = new GUI.Text(_font, _userInfoText, position, Color.White, 1.5f);

            game.pauseCurrentSceneUpdating = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!_loaded) return;
            try
            {
                //title colour cyclie
                _colourTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * _colourChangeSpeed;
                float r = (float)(Math.Sin(_colourTimer) * 0.5 + 0.5);
                float g = (float)(Math.Sin(_colourTimer + Math.PI / 3) * 0.5 + 0.5);
                float b = (float)(Math.Sin(_colourTimer + 2 * Math.PI / 3) * 0.5 + 0.5);
                _shorelineBGColour = new Color(r, g, b);

                _cloudManager.Update(gameTime);

                if (_currentState == UIState.Main)
                {
                    _logoutButton.Update();
                    _playButton.Update();
                }
                else if (_currentState == UIState.Play)
                {
                    _backButton.Update();
                    _slot1Button.Update();
                    _slot2Button.Update();
                    _slot3Button.Update();
                    _slot1DeleteButton.Update();
                    _slot2DeleteButton.Update();
                    _slot3DeleteButton.Update();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_loaded) return;

            if (_currentState == UIState.Play)
            {
                game.GraphicsDevice.SetRenderTarget(_blurTarget);
                game.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _blurTarget.Width, _blurTarget.Height), Color.White);
                _cloudManager.Draw(spriteBatch);
                spriteBatch.Draw(_shorelineTitleBG, Vector2.Zero, null, _shorelineBGColour, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.Draw(_shorelineTitle, Vector2.Zero, null, Color.LightYellow, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.End();
                game.GraphicsDevice.SetRenderTarget(null);
                if (_blurEffect.Parameters["Resolution"] != null)
                    _blurEffect.Parameters["Resolution"].SetValue(new Vector2(_blurTarget.Width, _blurTarget.Height));
                spriteBatch.Begin(effect: _blurEffect, samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(_blurTarget, new Rectangle(0, 0, _blurTarget.Width, _blurTarget.Height), Color.White);
                spriteBatch.End();

                //not effected by blur
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _backButton.Draw(spriteBatch);
                _slot1Button.Draw(spriteBatch);
                _slot2Button.Draw(spriteBatch);
                _slot3Button.Draw(spriteBatch);
                _slot1DeleteButton.Draw(spriteBatch);
                _slot2DeleteButton.Draw(spriteBatch);
                _slot3DeleteButton.Draw(spriteBatch);
                spriteBatch.End();
            }
            else //main
            {
                game.GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 1920, 1080), Color.White);
                _cloudManager.Draw(spriteBatch);
                spriteBatch.Draw(_shorelineTitleBG, Vector2.Zero, null, _shorelineBGColour, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.Draw(_shorelineTitle, Vector2.Zero, null, Color.LightYellow, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                _logoutButton.Draw(spriteBatch);
                _playButton.Draw(spriteBatch);
                _userInfoDisplay.Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        private void SwitchState(UIState newState)
        {
            _currentState = newState;

            if (newState == UIState.Main)
            {
                _logoutButton.Move(new Vector2(100, 800));
                _playButton.Move(new Vector2(100, 700));
                _backButton.Move(_offScreen);
                _slot1Button.Move(_offScreen);
                _slot2Button.Move(_offScreen);
                _slot3Button.Move(_offScreen);
                _slot1DeleteButton.Move(_offScreen);
                _slot2DeleteButton.Move(_offScreen);
                _slot3DeleteButton.Move(_offScreen);
            }
            else if (newState == UIState.Play)
            {
                _logoutButton.Move(_offScreen);
                _playButton.Move(_offScreen);
                _backButton.Move(new Vector2(100, 800));

                _slot1Button.Move(new Vector2(100, 200));
                _slot2Button.Move(new Vector2(100, 300));
                _slot3Button.Move(new Vector2(100, 400));
                _slot1DeleteButton.Move(new Vector2(1120, 200));
                _slot2DeleteButton.Move(new Vector2(1120, 300));
                _slot3DeleteButton.Move(new Vector2(1120, 400));

                UpdateSaveSlotUI();
            }
        }

        private void UpdateSaveSlotUI()
        {
            UpdateSlotUI(_slot1Button, _slot1DeleteButton, 0);
            UpdateSlotUI(_slot2Button, _slot2DeleteButton, 1);
            UpdateSlotUI(_slot3Button, _slot3DeleteButton, 2);
        }

        private void UpdateSlotUI(GUI.Button slotButton, GUI.Button deleteButton, int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            Console.WriteLine(filePath);
            if (File.Exists(filePath))
            {
                slotButton.SetText($"Slot {slotIndex + 1}: Load Game");
                slotButton.OnClickAction = () => LoadGame(filePath);
                deleteButton.OnClickAction = () => DeleteSave(slotIndex);
                deleteButton.SetText("Delete");
            }
            else
            {
                slotButton.SetText($"Slot {slotIndex + 1}: New Game");
                slotButton.OnClickAction = () => StartNewGame(slotIndex);
                deleteButton.OnClickAction = null;
                deleteButton.SetText("");
            }
        }

        private string GetUserSaveDirectory()
        {
            string userFolder = Path.Combine(_saveDirectory, Main.LoggedInUserID.ToString());
            if (!Directory.Exists(userFolder))
            {
                Console.WriteLine("creating new usersavedir");
                Directory.CreateDirectory(userFolder);
            }
            return userFolder;
        }

        private string GetSaveFilePath(int slotIndex)
        {
            string userFolder = GetUserSaveDirectory();
            return Path.Combine(userFolder, $"slot{slotIndex + 1}.json");
        }

        private void LoadGame(string filePath)
        {
            Console.WriteLine($"Loading game from: {filePath}");
            GameSave loadedSave = SaveManager.LoadGame(filePath);
            if (loadedSave != null)
            {
                DbFunctions db = new DbFunctions();
                int worldId = db.GetWorldIdFromSavePath(Main.LoggedInUserID, filePath);
                if (worldId == -1)
                {
                    Console.WriteLine("error retrieving worldid");
                    return;
                }
                var (seed, creation_date, difficulty) = db.GetWorldData(worldId);
                MOVEDEBUGTEST.SEED = seed;
                game.ChangeState(GameState.DEBUG, loadedSave);
            }
            else
            {
                throw new FileNotFoundException("failed to load save file");
            }
        }

        private void StartNewGame(int slotIndex)
        {
            Console.WriteLine($"Starting a new game in slot {slotIndex + 1}");
            GameSave newSave = GameSave.CreateDefaultSave((slotIndex + 1).ToString());
            string filePath = GetSaveFilePath(slotIndex);
            SaveManager.SaveGame(newSave, filePath);
            Console.WriteLine($"New game saved at: {filePath}");
            DbFunctions db = new DbFunctions();
            int userId = Main.LoggedInUserID;
            int worldId = db.CreateNewWorld();
            int locationX = 0;
            int locationY = 0;
            int coins = 100;
            db.UpdateUserWorldSavesData(userId, worldId, locationX, locationY, coins, filePath);
            UpdateSaveSlotUI();
        }

        private void DeleteSave(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Deleted save file: {filePath}");
                UpdateSaveSlotUI();
            }
        }

        public override void Shutdown()
        {
            _loaded = false;

            _backgroundTexture?.Dispose();
        }
    }
}
