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
        public Core.GameState CurrentState;
        public bool PauseCurrentSceneUpdateing;
        public static string LoggedInUsername = "Shroomba";
        public static int LoggedInUserID = 1;
        public static int CurrentWorldID;
        public bool InMiniGame = false;
        public SceneStack SceneStack;

        private Database.DbFunctions _dbFunctions;
        private Database.CreateDB _createDB;

        public bool pauseCurrentSceneUpdating;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            SceneStack = new SceneStack(this);
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false;
            _graphics.ApplyChanges();

            CurrentState = Core.GameState.MainMenu;
            _createDB = new Database.CreateDB();
            _createDB.CreateDatabase();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            TextureManager.LoadContent(Content);
            ChangeState(CurrentState);
        }

        protected override void Update(GameTime gameTime)
        {
            /*if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();*/

            SceneStack.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SceneStack.Draw(_spriteBatch);
            base.Draw(gameTime);
        }

        public void ChangeState(GameState newState, GameSave save = null)
        {
            PauseCurrentSceneUpdateing = true;

            Scene newScene = newState switch
            {
                GameState.Login => new LoginScene(this),
                GameState.DEBUG => new MOVEDEBUGTEST(this, save),
                GameState.AdminView => new AdminView(this),
                GameState.MainMenu => new MainMenu(this),
                _ => null
            };

            if (newScene != null)
                SceneStack.ChangeScene(newScene);
        }

        public void StartMiniGame(SubGameState newState, GameSave save = null)
        {
            Scene subScene = newState switch
            {
                SubGameState.Tetris => new Minigames.Tetris.TetrisGame(this),
                SubGameState.Connect4 => new Minigames.Connect4.Connect4Game(this),
                SubGameState.Maze => new Minigames.Maze.MazeGame(this),
                SubGameState.ShopMenu => new ShopMenu(this, save),
                _ => null
            };
            SceneStack.PushScene(subScene);
        }

        public void CloseMiniGame(int reward)
        {
            SceneStack.PopScene();

            ((MOVEDEBUGTEST)SceneStack.CurrentScene).UpdateShmacks(reward);
        }
    }
}
