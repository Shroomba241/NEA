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

        private bool _loaded = false;
        private UIState _currentState = UIState.Welcome;
        private Vector2 _offScreen = new Vector2(-500, -500);

        private SpriteFont _font;

        private GUI.Button _quitButton;
        // welcome
        private GUI.Text _titleText;
        private GUI.Button _loginButton;
        private GUI.Button _signupButton;
        // login specific
        private GUI.Text _loginTitleText;
        // signup specific
        private GUI.Text _signupTitleText;
        private GUI.InputBox _confirmPasswordInputBox;
        // login and signup
        private GUI.Text _errorText;
        private GUI.InputBox _usernameInputBox;
        private GUI.InputBox _passwordInputBox;
        private GUI.Button _submitButton;
        private GUI.Button _cancelButton;

        private Database.DbFunctions _dbFunctions;

        private GUI.InputBox _activeInputBox;
        private GUI.InputBox[] _allInputBoxes;

        private string _errorTextContents = "";

        private RenderTarget2D _renderTarget;
        private Effect _crtEffect;

        private Main game;

        public LoginScene(Main game)
        {
            this.game = game;
            _dbFunctions = new Database.DbFunctions();
        }

        public override void LoadContent()
        {
            _loaded = true;
            _font = game.Content.Load<SpriteFont>("DefaultFont");

            _titleText = new GUI.Text(_font, "Welcome to my Rubbish Game", new Vector2(100, 50), Color.White, 3.0f);
            _loginButton = new GUI.Button(game.GraphicsDevice, _font, "Login", new Vector2(100, 600), 400, 90);
            _signupButton = new GUI.Button(game.GraphicsDevice, _font, "Sign-Up", new Vector2(100, 700), 400, 90);
            _quitButton = new GUI.Button(game.GraphicsDevice, _font, "Quit", new Vector2(100, 800), 400, 90);

            _loginTitleText = new GUI.Text(_font, "Login", _offScreen, Color.White, 3.0f);
            _signupTitleText = new GUI.Text(_font, "Sign-Up", _offScreen, Color.White, 3.0f);
            _errorText = new GUI.Text(_font, _errorTextContents, _offScreen, Color.Red, 2.0f);
            _usernameInputBox = new GUI.InputBox(game.GraphicsDevice, _font, _offScreen, 800, 90, false, 15);
            _passwordInputBox = new GUI.InputBox(game.GraphicsDevice, _font, _offScreen, 800, 90, true, 15);
            _confirmPasswordInputBox = new GUI.InputBox(game.GraphicsDevice, _font, _offScreen, 800, 90, true, 15);

            _cancelButton = new GUI.Button(game.GraphicsDevice, _font, "Cancel", _offScreen, 400, 90);
            _submitButton = new GUI.Button(game.GraphicsDevice, _font, "Submit", _offScreen, 400, 90);

            _loginButton.OnClickAction = () => SwitchState(UIState.Login);
            _signupButton.OnClickAction = () => SwitchState(UIState.Signup);
            _quitButton.OnClickAction = () => game.Exit();

            _cancelButton.OnClickAction = () => SwitchState(UIState.Welcome);
            _submitButton.OnClickAction = () => { AttemptingLogin(_usernameInputBox.GetText(), _passwordInputBox.GetText()); };

            _usernameInputBox.OnClickAction = () => OnActiveInputBoxClick(_usernameInputBox);
            _passwordInputBox.OnClickAction = () => OnActiveInputBoxClick(_passwordInputBox);
            _confirmPasswordInputBox.OnClickAction = () => OnActiveInputBoxClick(_confirmPasswordInputBox);

            _allInputBoxes = new GUI.InputBox[] { _usernameInputBox, _passwordInputBox, _confirmPasswordInputBox };

            _renderTarget = new RenderTarget2D(game.GraphicsDevice, game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                        game.GraphicsDevice.PresentationParameters.BackBufferHeight);

            _crtEffect = game.Content.Load<Effect>("CRTEffect");

            game.pauseCurrentSceneUpdating = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!_loaded) return;
            try
            {
                _loginButton.Update();
                _signupButton.Update();
                _quitButton.Update();

                _cancelButton.Update();
                _submitButton.Update();

                _usernameInputBox.Update(gameTime);
                _passwordInputBox.Update(gameTime);
                _confirmPasswordInputBox.Update(gameTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_loaded) return;
            game.GraphicsDevice.SetRenderTarget(_renderTarget);
            game.GraphicsDevice.Clear(Color.Blue);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _titleText.Draw(spriteBatch);
            _loginButton.Draw(spriteBatch);
            _signupButton.Draw(spriteBatch);
            _quitButton.Draw(spriteBatch);

            _loginTitleText.Draw(spriteBatch);
            _signupTitleText.Draw(spriteBatch);
            _errorText.Draw(spriteBatch);

            _usernameInputBox.Draw(spriteBatch);
            _passwordInputBox.Draw(spriteBatch);
            _confirmPasswordInputBox.Draw(spriteBatch);

            _cancelButton.Draw(spriteBatch);
            _submitButton.Draw(spriteBatch);

            _errorText.UpdateContent(_errorTextContents);

            spriteBatch.End();

            if (_crtEffect.Parameters["Resolution"] != null)
                _crtEffect.Parameters["Resolution"].SetValue(new Vector2(_renderTarget.Width, _renderTarget.Height));

            game.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(effect: _crtEffect, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(_renderTarget, new Rectangle(0, 0, _renderTarget.Width, _renderTarget.Height), Color.White);
            spriteBatch.End();
        }

        private void SwitchState(UIState newState)
        {
            ClearInputBoxes();

            _currentState = newState;

            if (newState == UIState.Welcome)
            {
                _titleText.position = new Vector2(100, 50);

                _loginButton.Move(new Vector2(100, 600));
                _signupButton.Move(new Vector2(100, 700));
                _quitButton.Move(new Vector2(100, 800));

                _loginTitleText.position = _offScreen;
                _signupTitleText.position = _offScreen;
                _errorText.position = _offScreen;

                _usernameInputBox.Move(_offScreen);
                _passwordInputBox.Move(_offScreen);
                _confirmPasswordInputBox.Move(_offScreen);

                _cancelButton.Move(_offScreen);
                _submitButton.Move(_offScreen);

                _submitButton.OnClickAction = () => { AttemptingLogin(_usernameInputBox.GetText(), _passwordInputBox.GetText()); };
            }
            else if (newState == UIState.Login)
            {
                _titleText.position = _offScreen;
                _loginButton.Move(_offScreen);
                _signupButton.Move(_offScreen);
                _quitButton.Move(_offScreen);

                _loginTitleText.position = new Vector2(100, 50);
                _signupTitleText.position = _offScreen;
                _errorText.position = new Vector2(100, 400);

                _usernameInputBox.Move(new Vector2(100, 200));
                _passwordInputBox.Move(new Vector2(100, 300));
                _confirmPasswordInputBox.Move(_offScreen);

                _submitButton.Move(new Vector2(100, 700));
                _cancelButton.Move(new Vector2(100, 800));

                _submitButton.OnClickAction = () => { AttemptingLogin(_usernameInputBox.GetText(), _passwordInputBox.GetText()); };
            }
            else if (newState == UIState.Signup)
            {
                _titleText.position = _offScreen;
                _loginButton.Move(_offScreen);
                _signupButton.Move(_offScreen);
                _quitButton.Move(_offScreen);

                _signupTitleText.position = new Vector2(100, 50);
                _loginTitleText.position = _offScreen;
                _errorText.position = new Vector2(100, 500);

                _usernameInputBox.Move(new Vector2(100, 200));
                _passwordInputBox.Move(new Vector2(100, 300));
                _confirmPasswordInputBox.Move(new Vector2(100, 400));

                _submitButton.Move(new Vector2(100, 700));
                _cancelButton.Move(new Vector2(100, 800));

                _submitButton.OnClickAction = () => { AttemptingSignup(_usernameInputBox.GetText(), _passwordInputBox.GetText(), _confirmPasswordInputBox.GetText()); };
            }
        }

        private void ClearInputBoxes()
        {
            Console.WriteLine("boxes cleared");
            _usernameInputBox.SetText("");
            _passwordInputBox.SetText("");
            _confirmPasswordInputBox.SetText("");
            _errorTextContents = "";
        }

        private void OnActiveInputBoxClick(GUI.InputBox nowActive)
        {
            foreach (var box in _allInputBoxes)
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

            bool isAuthenticated = _dbFunctions.AuthenticateUser(username, password);

            if (!isAuthenticated)
            {
                _errorTextContents = "Invalid username or password... Loser.";
                return;
            }

            bool isAdmin = _dbFunctions.IsUserAdmin(username);
            Main.LoggedInUsername = username;
            Main.LoggedInUserID = _dbFunctions.GetUserIDFromUsername(username);
            if (isAdmin)
            {
                game.ChangeState(GameState.AdminView);
            }
            else
            {
                game.ChangeState(GameState.MainMenu);
            }
        }

        private void AttemptingSignup(string username, string password, string confirmPassword)
        {
            if (!UsernameFormatChecker(username) || !PasswordFormatChecker(password))
            {
                return;
            }

            if (password != confirmPassword)
            {
                _errorTextContents = "Passwords do not match.";
                return;
            }

            bool isRegistered = _dbFunctions.RegisterUser(username, password);
            if (!isRegistered)
            {
                _errorTextContents = "Username is not original.";
                return;
            }

            Main.LoggedInUsername = username;
            Main.LoggedInUserID = _dbFunctions.GetUserIDFromUsername(username);

            game.ChangeState(GameState.MainMenu);
        }

        private bool UsernameFormatChecker(string username)
        {
            if (username.Length < 3)
            {
                _errorTextContents = "Username must be at least 3 characters long.";
                return false;
            }
            return true;
        }

        private bool PasswordFormatChecker(string password)
        {
            if (password.Length < 8)
            {
                _errorTextContents = "Password must be at least 8 characters long.";
                return false;
            }
            if (!password.Any(char.IsUpper))
            {
                _errorTextContents = "Password must contain at least one uppercase letter.";
                return false;
            }
            if (!password.Any(char.IsLower))
            {
                _errorTextContents = "Password must contain at least one lowercase letter.";
                return false;
            }
            if (!password.Any(char.IsDigit))
            {
                _errorTextContents = "Password must contain at least one number.";
                return false;
            }
            return true;
        }

        public override void Shutdown()
        {
            _loaded = false;

            _titleText = null;
            _loginButton = null;
            _signupButton = null;
            _quitButton = null;
            _loginTitleText = null;
            _signupTitleText = null;
            _usernameInputBox = null;
            _passwordInputBox = null;
            _confirmPasswordInputBox = null;
            _cancelButton = null;
            _submitButton = null;

            _activeInputBox = null;
            _allInputBoxes = null;
        }
    }
}
