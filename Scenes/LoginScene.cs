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
        //welcome
        private GUI.Text titleText;
        private GUI.Button loginButton;
        private GUI.Button signupButton;
        //login specific
        private GUI.Text loginTitleText;
        //signup specific
        private GUI.Text signupTitleText;
        //login and signup
        private GUI.Text errorText;
        private GUI.InputBox usernameInputBox;
        private GUI.InputBox passwordInputBox;
        private GUI.Button submitButton;
        private GUI.Button cancelButton;

        Database.DbFunctions dbFunctions;

        private GUI.InputBox activeInputBox;
        private GUI.InputBox[] allInputBoxes;

        private string errorTextContents = "";


        private Main game;

        public LoginScene(Main game)
        {
            this.game = game;

            dbFunctions = new Database.DbFunctions();
        }

        public override void LoadContent() //login init setup for welcome state
        {
            loaded = true;
            font = game.Content.Load<SpriteFont>("DefaultFont");

            titleText = new GUI.Text(font, "Welcome to my Rubbish Game", new Vector2(100, 50), Color.White, 3.0f);
            loginButton = new GUI.Button(game.GraphicsDevice, font, "Login", new Vector2(100, 600), 400, 90);
            signupButton = new GUI.Button(game.GraphicsDevice, font, "Sign-Up", new Vector2(100, 700), 400, 90);
            quitButton = new GUI.Button(game.GraphicsDevice, font, "Quit", new Vector2(100, 800), 400, 90);

            //start offscreen
            loginTitleText = new GUI.Text(font, "Login", offScreen, Color.White, 3.0f);
            signupTitleText = new GUI.Text(font, "Sign-Up", offScreen, Color.White, 3.0f);
            errorText = new GUI.Text(font, errorTextContents, offScreen, Color.Red, 2.0f);
            usernameInputBox = new GUI.InputBox(game.GraphicsDevice, font, offScreen, 800, 90, false, 15);
            passwordInputBox = new GUI.InputBox(game.GraphicsDevice, font, offScreen, 800, 90, true, 15);
            cancelButton = new GUI.Button(game.GraphicsDevice, font, "Cancel", offScreen, 400, 90);
            submitButton = new GUI.Button(game.GraphicsDevice, font, "Submit", offScreen, 400, 90);

            loginButton.OnClickAction = () => SwitchState(UIState.Login);
            signupButton.OnClickAction = () => SwitchState(UIState.Signup);
            quitButton.OnClickAction = () => game.Exit();

            cancelButton.OnClickAction = () => SwitchState(UIState.Welcome);
            submitButton.OnClickAction = () => { AttemptingLogin(usernameInputBox.GetText(), passwordInputBox.GetText()); };

            usernameInputBox.OnClickAction = () => OnActiveInputBoxClick(usernameInputBox);
            passwordInputBox.OnClickAction = () => OnActiveInputBoxClick(passwordInputBox);

            allInputBoxes = new GUI.InputBox[] { usernameInputBox, passwordInputBox, };

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!loaded) return;
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

            cancelButton.Draw(spriteBatch);
            submitButton.Draw(spriteBatch);

            errorText.UpdateContent(errorTextContents);

            spriteBatch.End();
        }

        private void SwitchState(UIState newState)
        {
            Console.WriteLine(loaded);
            if (!loaded) return;
            Console.WriteLine("updateing");
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

                cancelButton.Move(offScreen);
                submitButton.Move(offScreen);

            }
            else if (newState == UIState.Login)
            {
                titleText.position = offScreen;
                loginButton.Move(offScreen);
                signupButton.Move(offScreen);
                quitButton.Move(offScreen);

                loginTitleText.position = new Vector2(100, 50);
                errorText.position = new Vector2(100, 400);
                usernameInputBox.Move(new Vector2(100, 200));
                passwordInputBox.Move(new Vector2(100, 300));
                submitButton.Move(new Vector2(100, 700));
                cancelButton.Move(new Vector2(100, 800));
            }
            else if (newState == UIState.Signup)
            {
                titleText.position = offScreen;
                loginButton.Move(offScreen);
                signupButton.Move(offScreen);
                quitButton.Move(offScreen);

                signupTitleText.position = new Vector2(100, 50);
                errorText.position = new Vector2(100, 400);
                submitButton.Move(new Vector2(100, 700));
                cancelButton.Move(new Vector2(100, 800));
            }
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

            //put stwitchstate here
            game.ChangeState(GameState.DEBUG);
            Console.WriteLine("sucess!!");
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
            // Unload specific resources here
            font = null;

            // Set GUI elements to null
            titleText = null;
            loginButton = null;
            signupButton = null;
            quitButton = null;
            loginTitleText = null;
            signupTitleText = null;
            errorText = null;
            usernameInputBox = null;
            passwordInputBox = null;
            cancelButton = null;
            submitButton = null;

            // Set other resources to null or dispose them if necessary
            dbFunctions = null;
            activeInputBox = null;
            allInputBoxes = null;
        }
    }
}
