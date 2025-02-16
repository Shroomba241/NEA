

using CompSci_NEA.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CompSci_NEA.Scenes
{
    public class LoginScene : Scene
    {
        private enum UIState
        {
            Welcome,
            Login,
            Signup
        }

        private bool loaded = false;
        private UIState currentState = UIState.Welcome;
        private Vector2 offScreen = new Vector2(-500, -500);

        private SpriteFont font;

        private GUI.Button quitButton;
        // welcome
        private GUI.Text titleText;
        private GUI.Button loginButton;
        private GUI.Button signupButton;
        // login specific
        private GUI.Text loginTitleText;
        // signup specific
        private GUI.Text signupTitleText;
        private GUI.InputBox confirmPasswordInputBox;
        // login and signup
        private GUI.Text errorText;
        private GUI.InputBox usernameInputBox;
        private GUI.InputBox passwordInputBox;
        private GUI.Button submitButton;
        private GUI.Button cancelButton;

        Database.DbFunctions dbFunctions;

        private GUI.InputBox activeInputBox;
        private GUI.InputBox[] allInputBoxes;

        private string errorTextContents = "";

        private RenderTarget2D renderTarget;
        private Effect crtEffect;

        private Main game;

        public LoginScene(Main game)
        {
            this.game = game;
            dbFunctions = new Database.DbFunctions();
        }

        public override void LoadContent()
        {
            loaded = true;
            font = game.Content.Load<SpriteFont>("DefaultFont");

            titleText = new GUI.Text(font, "Welcome to my Rubbish Game", new Vector2(100, 50), Color.White, 3.0f);
            loginButton = new GUI.Button(game.GraphicsDevice, font, "Login", new Vector2(100, 600), 400, 90);
            signupButton = new GUI.Button(game.GraphicsDevice, font, "Sign-Up", new Vector2(100, 700), 400, 90);
            quitButton = new GUI.Button(game.GraphicsDevice, font, "Quit", new Vector2(100, 800), 400, 90);

            loginTitleText = new GUI.Text(font, "Login", offScreen, Color.White, 3.0f);
            signupTitleText = new GUI.Text(font, "Sign-Up", offScreen, Color.White, 3.0f);
            errorText = new GUI.Text(font, errorTextContents, offScreen, Color.Red, 2.0f);
            usernameInputBox = new GUI.InputBox(game.GraphicsDevice, font, offScreen, 800, 90, false, 15);
            passwordInputBox = new GUI.InputBox(game.GraphicsDevice, font, offScreen, 800, 90, true, 15);
            confirmPasswordInputBox = new GUI.InputBox(game.GraphicsDevice, font, offScreen, 800, 90, true, 15);

            cancelButton = new GUI.Button(game.GraphicsDevice, font, "Cancel", offScreen, 400, 90);
            submitButton = new GUI.Button(game.GraphicsDevice, font, "Submit", offScreen, 400, 90);

            loginButton.OnClickAction = () => SwitchState(UIState.Login);
            signupButton.OnClickAction = () => SwitchState(UIState.Signup);
            quitButton.OnClickAction = () => game.Exit();

            cancelButton.OnClickAction = () => SwitchState(UIState.Welcome);
            submitButton.OnClickAction = () => { AttemptingLogin(usernameInputBox.GetText(), passwordInputBox.GetText()); };

            usernameInputBox.OnClickAction = () => OnActiveInputBoxClick(usernameInputBox);
            passwordInputBox.OnClickAction = () => OnActiveInputBoxClick(passwordInputBox);
            confirmPasswordInputBox.OnClickAction = () => OnActiveInputBoxClick(confirmPasswordInputBox);

            allInputBoxes = new GUI.InputBox[] { usernameInputBox, passwordInputBox, confirmPasswordInputBox };

            renderTarget = new RenderTarget2D(game.GraphicsDevice, game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                        game.GraphicsDevice.PresentationParameters.BackBufferHeight);

            crtEffect = game.Content.Load<Effect>("CRTEffect");

            game.pauseCurrentSceneUpdateing = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!loaded) return;
            try
            {
                loginButton.Update();
                signupButton.Update();
                quitButton.Update();

                cancelButton.Update();
                submitButton.Update();

                usernameInputBox.Update(gameTime);
                passwordInputBox.Update(gameTime);
                confirmPasswordInputBox.Update(gameTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!loaded) return;
            game.GraphicsDevice.SetRenderTarget(renderTarget);
            game.GraphicsDevice.Clear(Color.Blue);  

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            titleText.Draw(spriteBatch);
            loginButton.Draw(spriteBatch);
            signupButton.Draw(spriteBatch);
            quitButton.Draw(spriteBatch);

            loginTitleText.Draw(spriteBatch);
            signupTitleText.Draw(spriteBatch);
            errorText.Draw(spriteBatch);

            usernameInputBox.Draw(spriteBatch);
            passwordInputBox.Draw(spriteBatch);
            confirmPasswordInputBox.Draw(spriteBatch);

            cancelButton.Draw(spriteBatch);
            submitButton.Draw(spriteBatch);

            errorText.UpdateContent(errorTextContents);

            spriteBatch.End();


            if (crtEffect.Parameters["Resolution"] != null)
                crtEffect.Parameters["Resolution"].SetValue(new Vector2(renderTarget.Width, renderTarget.Height));

            game.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(effect: crtEffect, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
            spriteBatch.End();
        }

        private void SwitchState(UIState newState)
        {
            ClearInputBoxes();

            currentState = newState;

            if (newState == UIState.Welcome)
            {
                titleText.position = new Vector2(100, 50);

                loginButton.Move(new Vector2(100, 600));
                signupButton.Move(new Vector2(100, 700));
                quitButton.Move(new Vector2(100, 800));

                loginTitleText.position = offScreen;
                signupTitleText.position = offScreen;
                errorText.position = offScreen;

                usernameInputBox.Move(offScreen);
                passwordInputBox.Move(offScreen);
                confirmPasswordInputBox.Move(offScreen);

                cancelButton.Move(offScreen);
                submitButton.Move(offScreen);

                submitButton.OnClickAction = () => { AttemptingLogin(usernameInputBox.GetText(), passwordInputBox.GetText()); };
            }
            else if (newState == UIState.Login)
            {
                titleText.position = offScreen;
                loginButton.Move(offScreen);
                signupButton.Move(offScreen);
                quitButton.Move(offScreen);

                loginTitleText.position = new Vector2(100, 50);
                signupTitleText.position = offScreen;
                errorText.position = new Vector2(100, 400);

                usernameInputBox.Move(new Vector2(100, 200));
                passwordInputBox.Move(new Vector2(100, 300));
                confirmPasswordInputBox.Move(offScreen);

                submitButton.Move(new Vector2(100, 700));
                cancelButton.Move(new Vector2(100, 800));

                submitButton.OnClickAction = () => { AttemptingLogin(usernameInputBox.GetText(), passwordInputBox.GetText()); };
            }
            else if (newState == UIState.Signup)
            {
                titleText.position = offScreen;
                loginButton.Move(offScreen);
                signupButton.Move(offScreen);
                quitButton.Move(offScreen);

                signupTitleText.position = new Vector2(100, 50);
                loginTitleText.position = offScreen;
                errorText.position = new Vector2(100, 500);


                usernameInputBox.Move(new Vector2(100, 200));
                passwordInputBox.Move(new Vector2(100, 300));
                confirmPasswordInputBox.Move(new Vector2(100, 400));

                submitButton.Move(new Vector2(100, 700));
                cancelButton.Move(new Vector2(100, 800));

                submitButton.OnClickAction = () => { AttemptingSignup(usernameInputBox.GetText(), passwordInputBox.GetText(), confirmPasswordInputBox.GetText()); };
            }
        }

        private void ClearInputBoxes()
        {
            Console.WriteLine("boxes cleared");
            usernameInputBox.SetText("");
            passwordInputBox.SetText("");
            confirmPasswordInputBox.SetText("");
            errorTextContents = "";
        }

        private void OnActiveInputBoxClick(GUI.InputBox nowActive)
        {
            foreach (var box in allInputBoxes)
            {
                if (box == nowActive) continue;
                else box.isActive = false;
            }
        }

        private void AttemptingLogin(string username, string password)
        {
            if (!UsernameFormatChecker(username) || !PasswordFormatChecker(password))
            {
                return;
            }

            bool isAuthenticated = dbFunctions.AuthenticateUser(username, password);

            if (!isAuthenticated)
            {
                errorTextContents = "Invalid username or password... Loser.";
                return;
            }

            bool isAdmin = dbFunctions.IsUserAdmin(username);
            game.LoggedInUsername = username;
            if (isAdmin)
            {
                game.ChangeState(GameState.AdminView); // Load the AdminView scene
            }
            else
            {
                game.ChangeState(GameState.DEBUG); // Regular view scene
            }
            Console.WriteLine("Login successful!!");
        }

        private void AttemptingSignup(string username, string password, string confirmPassword)
        {
            if (!UsernameFormatChecker(username) || !PasswordFormatChecker(password))
            {
                return;
            }

            if (password != confirmPassword)
            {
                errorTextContents = "Passwords do not match.";
                return;
            }

            bool isRegistered = dbFunctions.RegisterUser(username, password);
            if (!isRegistered)
            {
                errorTextContents = "Username is not original.";
                return;
            }

            Console.WriteLine("Signup successful!!");
            game.ChangeState(GameState.AdminView);
        }
        private bool UsernameFormatChecker(string username)
        {
            if (username.Length < 3)
            {
                errorTextContents = "Username must be at least 3 characters long.";
                return false;
            }
            return true;
        }

        private bool PasswordFormatChecker(string password)
        {
            if (password.Length < 8)
            {
                errorTextContents = "Password must be at least 8 characters long.";
                return false;
            }
            if (!password.Any(char.IsUpper))
            {
                errorTextContents = "Password must contain at least one uppercase letter.";
                return false;
            }
            if (!password.Any(char.IsLower))
            {
                errorTextContents = "Password must contain at least one lowercase letter.";
                return false;
            }
            if (!password.Any(char.IsDigit))
            {
                errorTextContents = "Password must contain at least one number.";
                return false;
            }
            return true;
        }

        public override void Shutdown()
        {
            loaded = false;

            titleText = null;
            loginButton = null;
            signupButton = null;
            quitButton = null;
            loginTitleText = null;
            signupTitleText = null;
            usernameInputBox = null;
            passwordInputBox = null;
            confirmPasswordInputBox = null;
            cancelButton = null;
            submitButton = null;

            activeInputBox = null;
            allInputBoxes = null;
        }
    }
}
