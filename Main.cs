using CompSci_NEA.Core;
using CompSci_NEA.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Data.SqlClient;
using System.Linq;

namespace CompSci_NEA
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public Core.GameState currentState;
        private Scenes.Scene currentScene;
        public bool pauseCurrentSceneUpdateing;
        public string LoggedInUsername { get; set; }

        //idk if these need to be here or maybe i could scene them somewhere.
        private Database.DbFunctions _dbFunctions;
        private Database.CreateDB _createDB;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }


        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false; //boarderless windowed
            //_graphics.SynchronizeWithVerticalRetrace = false; // Disable VSync
            //IsFixedTimeStep = false;
            _graphics.ApplyChanges();
            

            currentState = Core.GameState.AdminView;
            _createDB = new Database.CreateDB();
            _createDB.CreateDatabase();
            //_dbFunctions = new Database.DbFunctions();

            //Console.WriteLine(_dbFunctions.AuthenticateUser("TestUser", "password123"));
            //_dbFunctions.AddUser("Shroomba", "Password123");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            TextureManager.LoadContent(Content);
            //Enumerable.Range(0, 10).ToList().ForEach(_ => Console.WriteLine(_dbFunctions.GenerateRandomSalt()));
            ChangeState(currentState);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (!pauseCurrentSceneUpdateing) currentScene.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            if (!pauseCurrentSceneUpdateing) currentScene.Draw(_spriteBatch);

            base.Draw(gameTime);
        }

        public void ChangeState(GameState newState)
        {
            pauseCurrentSceneUpdateing = true;
            currentScene?.Shutdown();

            currentState = newState;
            switch (newState)
            {
                case GameState.Login:
                    currentScene = new LoginScene(this);
                    break;
                case GameState.DEBUG:
                    currentScene = new MOVEDEBUGTEST(this);
                    break;
                case GameState.ZoneATest:
                    currentScene = new ZoneABiomeTesting(this);
                    break;
                case GameState.AdminView:
                    LoggedInUsername = "Shroomba";
                    currentScene = new AdminView(this);
                    break;
            }
            currentScene.LoadContent();
        }
    }
}