using CompSci_NEA.Core;
using CompSci_NEA.GUI;
using CompSci_NEA.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Data.SqlClient;

namespace CompSci_NEA
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public Core.GameState currentState;
        public bool pauseCurrentSceneUpdateing;
        public static string LoggedInUsername = "Shroomba";
        public static int LoggedInUserID = 1;
        public bool InMiniGame = false;

        // Scene stack system
        private SceneStack sceneStack;

        // Database functionality (if still needed)
        private Database.DbFunctions _dbFunctions;
        private Database.CreateDB _createDB;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            sceneStack = new SceneStack(this);
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false;
            _graphics.ApplyChanges();

            currentState = Core.GameState.DEBUG;
            _createDB = new Database.CreateDB();
            _createDB.CreateDatabase();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            TextureManager.LoadContent(Content);
            ChangeState(currentState);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            sceneStack.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            sceneStack.Draw(_spriteBatch);
            base.Draw(gameTime);
        }


        public void ChangeState(GameState newState)
        {
            pauseCurrentSceneUpdateing = true;

            Scene newScene = newState switch
            {
                GameState.Login => new LoginScene(this),
                GameState.DEBUG => new MOVEDEBUGTEST(this),
                GameState.AdminView => new AdminView(this),
                GameState.MainMenu => new MainMenu(this),
                _ => null
            };

            if (newScene != null)
                sceneStack.ChangeScene(newScene);
        }

        public void StartMiniGame(SubGameState newState)
        {
            Scene subScene = newState switch
            {
                SubGameState.Tetris => new TetrisGame(this),
                _ => null
            };
            sceneStack.PushScene(subScene);
        }

        public void CloseMiniGame(int reward)
        {
            sceneStack.PopScene();

            ((MOVEDEBUGTEST)sceneStack.CurrentScene).UpdateShmacks(reward);
        }
    }
}
