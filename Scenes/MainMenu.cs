using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using System;
using System.IO;

namespace CompSci_NEA.Scenes
{
    public class MainMenu : Scene
    {
        private enum UIState
        {
            Main,
            Play
        }

        private bool loaded = false;
        private UIState currentState = UIState.Main;
        private Vector2 offScreen = new Vector2(-500, -500);

        private SpriteFont font;

        // UI Elements for main menu
        private GUI.Button logoutButton;
        private GUI.Button playButton;
        private GUI.Button backButton;

        // UI Elements for save slots (shown in Play state)
        private GUI.Button slot1Button;
        private GUI.Button slot2Button;
        private GUI.Button slot3Button;
        private GUI.Button slot1DeleteButton;
        private GUI.Button slot2DeleteButton;
        private GUI.Button slot3DeleteButton;

        // Background and Scene Elements
        private Texture2D backgroundTexture;
        private Texture2D ShorelineTitle;
        private Texture2D ShorelineTitleBG;
        private MotherCloud cloudManager;
        private Song song;
        float colourChangeSpeed = 0.5f;
        float colourTimer = 0f;
        Color shorelineBGColour = Color.White;

        // Render target and blur effect (mimicking your CRT effect usage)
        private RenderTarget2D blurTarget;
        private Effect blurEffect;

        // Save game directory
        private string saveDirectory = "Saves";

        private Main game;

        public MainMenu(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            loaded = true;
            font = game.Content.Load<SpriteFont>("DefaultFont");

            backgroundTexture = TextureManager.MainMenuBG;
            ShorelineTitle = TextureManager.Shoreline;
            ShorelineTitleBG = TextureManager.ShorelineBG;
            cloudManager = new MotherCloud(TextureManager.MainMenuClouds, 1920);

            song = TextureManager.MainMenuMusic;
            MediaPlayer.Play(song);
            MediaPlayer.Volume = 0.01f;
            MediaPlayer.IsRepeating = true;

            // Create UI Buttons for Main state
            logoutButton = new GUI.Button(game.GraphicsDevice, font, "Logout", new Vector2(100, 800), 400, 90);
            playButton = new GUI.Button(game.GraphicsDevice, font, "Play", new Vector2(100, 700), 400, 90);
            backButton = new GUI.Button(game.GraphicsDevice, font, "Back", offScreen, 400, 90);

            // Set main button actions
            logoutButton.OnClickAction = () => { game.ChangeState(GameState.Login); };
            playButton.OnClickAction = () => SwitchState(UIState.Play);
            backButton.OnClickAction = () => SwitchState(UIState.Main);

            // Create the render target and load the blur effect (ensure "BlurEffect" exists in your Content)
            blurTarget = new RenderTarget2D(
                game.GraphicsDevice,
                game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                game.GraphicsDevice.PresentationParameters.BackBufferHeight);
            blurEffect = game.Content.Load<Effect>("BlurEffect");

            // Create save directory if it doesn't exist
            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            // Create Save Slot UI Elements (initialize off-screen)
            slot1Button = new GUI.Button(game.GraphicsDevice, font, "Slot 1", offScreen, 1000, 80);
            slot2Button = new GUI.Button(game.GraphicsDevice, font, "Slot 2", offScreen, 1000, 80);
            slot3Button = new GUI.Button(game.GraphicsDevice, font, "Slot 3", offScreen, 1000, 80);

            slot1DeleteButton = new GUI.Button(game.GraphicsDevice, font, "Delete", offScreen, 150, 80);
            slot2DeleteButton = new GUI.Button(game.GraphicsDevice, font, "Delete", offScreen, 150, 80);
            slot3DeleteButton = new GUI.Button(game.GraphicsDevice, font, "Delete", offScreen, 150, 80);

            game.pauseCurrentSceneUpdateing = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!loaded) return;
            try
            {
                // Update background colour cycle
                colourTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * colourChangeSpeed;
                float r = (float)(Math.Sin(colourTimer) * 0.5 + 0.5);
                float g = (float)(Math.Sin(colourTimer + Math.PI / 3) * 0.5 + 0.5);
                float b = (float)(Math.Sin(colourTimer + 2 * Math.PI / 3) * 0.5 + 0.5);
                shorelineBGColour = new Color(r, g, b);

                cloudManager.Update(gameTime);

                // Update UI elements based on the current state
                if (currentState == UIState.Main)
                {
                    logoutButton.Update();
                    playButton.Update();
                }
                else if (currentState == UIState.Play)
                {
                    backButton.Update();
                    slot1Button.Update();
                    slot2Button.Update();
                    slot3Button.Update();
                    slot1DeleteButton.Update();
                    slot2DeleteButton.Update();
                    slot3DeleteButton.Update();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!loaded) return;

            if (currentState == UIState.Play)
            {
                // Draw blurred background elements into the blur target.
                game.GraphicsDevice.SetRenderTarget(blurTarget);
                game.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, blurTarget.Width, blurTarget.Height), Color.White);
                cloudManager.Draw(spriteBatch);
                spriteBatch.Draw(ShorelineTitleBG, Vector2.Zero, null, shorelineBGColour, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.Draw(ShorelineTitle, Vector2.Zero, null, Color.LightYellow, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.End();

                // Reset render target to back buffer and apply blur effect.
                game.GraphicsDevice.SetRenderTarget(null);
                if (blurEffect.Parameters["Resolution"] != null)
                    blurEffect.Parameters["Resolution"].SetValue(new Vector2(blurTarget.Width, blurTarget.Height));
                spriteBatch.Begin(effect: blurEffect, samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(blurTarget, new Rectangle(0, 0, blurTarget.Width, blurTarget.Height), Color.White);
                spriteBatch.End();

                // Draw UI elements for Play state.
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                backButton.Draw(spriteBatch);
                slot1Button.Draw(spriteBatch);
                slot2Button.Draw(spriteBatch);
                slot3Button.Draw(spriteBatch);
                slot1DeleteButton.Draw(spriteBatch);
                slot2DeleteButton.Draw(spriteBatch);
                slot3DeleteButton.Draw(spriteBatch);
                spriteBatch.End();
            }
            else // UIState.Main
            {
                game.GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1920, 1080), Color.White);
                cloudManager.Draw(spriteBatch);
                spriteBatch.Draw(ShorelineTitleBG, Vector2.Zero, null, shorelineBGColour, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.Draw(ShorelineTitle, Vector2.Zero, null, Color.LightYellow, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                logoutButton.Draw(spriteBatch);
                playButton.Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        private void SwitchState(UIState newState)
        {
            // Reposition UI elements based on the state.
            currentState = newState;

            if (newState == UIState.Main)
            {
                logoutButton.Move(new Vector2(100, 800));
                playButton.Move(new Vector2(100, 700));
                backButton.Move(offScreen);
                slot1Button.Move(offScreen);
                slot2Button.Move(offScreen);
                slot3Button.Move(offScreen);
                slot1DeleteButton.Move(offScreen);
                slot2DeleteButton.Move(offScreen);
                slot3DeleteButton.Move(offScreen);
            }
            else if (newState == UIState.Play)
            {
                logoutButton.Move(offScreen);
                playButton.Move(offScreen);
                backButton.Move(new Vector2(100, 800));
                // Position save slot buttons.
                slot1Button.Move(new Vector2(100, 200));
                slot2Button.Move(new Vector2(100, 300));
                slot3Button.Move(new Vector2(100, 400));
                slot1DeleteButton.Move(new Vector2(1120, 200));
                slot2DeleteButton.Move(new Vector2(1120, 300));
                slot3DeleteButton.Move(new Vector2(1120, 400));

                // Update the text and click actions for each save slot.
                UpdateSaveSlotUI();
            }
        }

        // Updates all three save slots.
        private void UpdateSaveSlotUI()
        {
            UpdateSlotUI(slot1Button, slot1DeleteButton, 0);
            UpdateSlotUI(slot2Button, slot2DeleteButton, 1);
            UpdateSlotUI(slot3Button, slot3DeleteButton, 2);
        }

        // Checks if a save file exists for a slot and sets up the UI accordingly.
        private void UpdateSlotUI(GUI.Button slotButton, GUI.Button deleteButton, int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            if (File.Exists(filePath))
            {
                // Save exists – set button to load the game.
                slotButton.SetText($"Slot {slotIndex + 1}: Load Game");
                slotButton.OnClickAction = () => LoadGame(filePath);
                deleteButton.OnClickAction = () => DeleteSave(slotIndex);
                deleteButton.SetText("Delete");
            }
            else
            {
                // No save exists – set button to start a new game.
                slotButton.SetText($"Slot {slotIndex + 1}: New Game");
                slotButton.OnClickAction = () => StartNewGame(slotIndex);
                deleteButton.OnClickAction = null;
                deleteButton.SetText("");
            }
        }

        // Returns the full path for the given save slot.
        private string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"save{slotIndex}.sav");
        }

        private void LoadGame(string filePath)
        {
            // TODO: Add your game-loading logic here.
            Console.WriteLine($"Loading game from: {filePath}");
        }

        private void StartNewGame(int slotIndex)
        {
            // TODO: Add your new game initialization and saving logic here.
            Console.WriteLine($"Starting a new game in slot {slotIndex + 1}");
        }

        private void DeleteSave(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Deleted save file: {filePath}");
                UpdateSaveSlotUI(); // Refresh the UI for save slots.
            }
        }

        public override void Shutdown()
        {
            loaded = false;

            backgroundTexture?.Dispose();
            // Dispose additional resources if necessary.
        }
    }
}


/*using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using System;

namespace CompSci_NEA.Scenes
{
    public class MainMenu : Scene
    {
        private enum UIState
        {
            Main,
            Play
        }

        private bool loaded = false;
        private UIState currentState = UIState.Main;
        private Vector2 offScreen = new Vector2(-500, -500);

        private SpriteFont font;

        // UI Elements
        private GUI.Button logoutButton;
        private GUI.Button playButton;
        private GUI.Button backButton;

        // Background and Scene Elements
        private Texture2D backgroundTexture;
        private Texture2D ShorelineTitle;
        private Texture2D ShorelineTitleBG;
        private CloudManager cloudManager;
        private Song song;
        float colourChangeSpeed = 0.5f;
        float colourTimer = 0f;
        Color shorelineBGColour = Color.White;

        // Render target and blur effect (mimicking your CRT effect usage)
        private RenderTarget2D blurTarget;
        private Effect blurEffect;

        private Main game;

        public MainMenu(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            loaded = true;
            font = game.Content.Load<SpriteFont>("DefaultFont");

            backgroundTexture = TextureManager.MainMenuBG;
            ShorelineTitle = TextureManager.Shoreline;
            ShorelineTitleBG = TextureManager.ShorelineBG;
            cloudManager = new CloudManager(TextureManager.MainMenuClouds, 1920);

            song = TextureManager.MainMenuMusic;
            MediaPlayer.Play(song);
            MediaPlayer.Volume = 0.01f;
            MediaPlayer.IsRepeating = true;

            // Create UI Buttons
            logoutButton = new GUI.Button(game.GraphicsDevice, font, "Logout", new Vector2(100, 800), 400, 90);
            playButton = new GUI.Button(game.GraphicsDevice, font, "Play", new Vector2(100, 700), 400, 90);
            backButton = new GUI.Button(game.GraphicsDevice, font, "Back", offScreen, 400, 90);

            // Set button actions
            logoutButton.OnClickAction = () => { game.ChangeState(GameState.Login); };
            playButton.OnClickAction = () => SwitchState(UIState.Play);
            backButton.OnClickAction = () => SwitchState(UIState.Main);

            // Create the render target and load the blur effect (ensure "BlurEffect" exists in your Content)
            blurTarget = new RenderTarget2D(
                game.GraphicsDevice,
                game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                game.GraphicsDevice.PresentationParameters.BackBufferHeight);
            blurEffect = game.Content.Load<Effect>("BlurEffect");

            game.pauseCurrentSceneUpdateing = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!loaded) return;
            try
            {
                // Update background colour cycle
                colourTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * colourChangeSpeed;
                float r = (float)(Math.Sin(colourTimer) * 0.5 + 0.5);
                float g = (float)(Math.Sin(colourTimer + Math.PI / 3) * 0.5 + 0.5);
                float b = (float)(Math.Sin(colourTimer + 2 * Math.PI / 3) * 0.5 + 0.5);
                shorelineBGColour = new Color(r, g, b);

                cloudManager.Update(gameTime);

                // Update UI elements based on the current state
                if (currentState == UIState.Main)
                {
                    logoutButton.Update();
                    playButton.Update();
                }
                else if (currentState == UIState.Play)
                {
                    backButton.Update();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!loaded) return;

            if (currentState == UIState.Play)
            {
                game.GraphicsDevice.SetRenderTarget(blurTarget);
                game.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, blurTarget.Width, blurTarget.Height), Color.White);
                cloudManager.Draw(spriteBatch);
                spriteBatch.Draw(ShorelineTitleBG, Vector2.Zero, null, shorelineBGColour, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.Draw(ShorelineTitle, Vector2.Zero, null, Color.LightYellow, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.End();

                game.GraphicsDevice.SetRenderTarget(null);
                if (blurEffect.Parameters["Resolution"] != null)
                    blurEffect.Parameters["Resolution"].SetValue(new Vector2(blurTarget.Width, blurTarget.Height));
                spriteBatch.Begin(effect: blurEffect, samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(blurTarget, new Rectangle(0, 0, blurTarget.Width, blurTarget.Height), Color.White);
                spriteBatch.End();

                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                backButton.Draw(spriteBatch);
                spriteBatch.End();
            }
            else
            {
                game.GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1920, 1080), Color.White);
                cloudManager.Draw(spriteBatch);
                spriteBatch.Draw(ShorelineTitleBG, Vector2.Zero, null, shorelineBGColour, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                spriteBatch.Draw(ShorelineTitle, Vector2.Zero, null, Color.LightYellow, 0f, Vector2.Zero, 3.75f, SpriteEffects.None, 0f);
                logoutButton.Draw(spriteBatch);
                playButton.Draw(spriteBatch);
                spriteBatch.End();
            }
        }
        private void SwitchState(UIState newState)
        {
            // Reposition UI elements as per the state.
            currentState = newState;

            if (newState == UIState.Main)
            {
                logoutButton.Move(new Vector2(100, 800));
                playButton.Move(new Vector2(100, 700));
                backButton.Move(offScreen);
            }
            else if (newState == UIState.Play)
            {
                logoutButton.Move(offScreen);
                playButton.Move(offScreen);
                backButton.Move(new Vector2(100, 800)); // Adjust the Back button position as needed
            }
        }

        public override void Shutdown()
        {
            loaded = false;

            backgroundTexture?.Dispose();
        }
    }
}
*/