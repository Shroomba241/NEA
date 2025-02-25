using CompSci_NEA.Core;
using CompSci_NEA.Entities;
using CompSci_NEA.GUI;
using CompSci_NEA.Tilemap;
using CompSci_NEA.WorldGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.Scenes
{
    public class MOVEDEBUGTEST : Scene
    {
        public static int SEED = 4717;
        private Main game;
        private Player _player;
        private Camera _camera;
        private Tilemap.VisualTileMap _tileMapVisual;
        private Tilemap.CollisionTileMap _tileMapCollisions;
        private Tilemap.StructureTileMap _structureTileMap;
        private FoliageManager _foliageManager;
        private GUI.SimplePerformance _simplePerformance;
        private SpriteFont _font;
        private Texture2D _mapTexture;
        private Texture2D _highlightTexture;
        private bool _showMap = true;
        private float _chunkWidth = 1024f / 24f;
        private float _chunkHeight = 768f / 18f;
        public List<Rectangle> ExtraColliders { get; set; } = new List<Rectangle>();

        public static bool ShowCollisionDebug = false;
        private float _mapRegenTimer = 0f;
        private const float REGEN_INTERVAL = 3f;

        private HUDManager _hudManager;

        public MOVEDEBUGTEST(Main game)
        {
            this.game = game;
            NoiseGenerator.SetSeed(SEED);
        }

        public override void LoadContent()
        {
            _font = game.Content.Load<SpriteFont>("DefaultFont");
            _simplePerformance = new GUI.SimplePerformance(_font);

            _tileMapVisual = new Tilemap.VisualTileMap(game.GraphicsDevice, 24, 18, SEED);
            _tileMapCollisions = new Tilemap.CollisionTileMap(game.GraphicsDevice, 24, 18, SEED);
            _structureTileMap = new Tilemap.StructureTileMap(game.GraphicsDevice, 24, 18, SEED);

            _foliageManager = new FoliageManager(game, _tileMapVisual, SEED);

            _tileMapCollisions.ExtraColliders.AddRange(_structureTileMap.StoneBridgeColliders);

            _player = new Player(game.GraphicsDevice, new Vector2(150 * 48, 384 * 48));
            _camera = new Camera();

            game.pauseCurrentSceneUpdating = false;

            _highlightTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            _highlightTexture.SetData(new Color[] { Color.White });
            _tileMapVisual.InitializeMapTexture(game.GraphicsDevice, 1024, 768, _structureTileMap);
            _mapTexture = _tileMapVisual.UpdateMapTexture(game.GraphicsDevice);

            _hudManager = new HUDManager();
            _hudManager.LoadContent();

            game.StartMiniGame(SubGameState.Connect4);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            _showMap = ks.IsKeyDown(Keys.Tab);

            _foliageManager.UpdateMinigames(_player);
            _mapRegenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_mapRegenTimer >= REGEN_INTERVAL)
            {
                _mapRegenTimer = 0f;
                _mapTexture = _tileMapVisual.UpdateMapTexture(game.GraphicsDevice);
            }

            /*if (!game.InMiniGame && ks.IsKeyDown(Keys.E))
            {
                game.StartMiniGame(SubGameState.Tetris);
                game.InMiniGame = true;
            }*/

            _simplePerformance.Update(gameTime);
            _player.Update(_tileMapCollisions);
            _camera.Update(_player.Position);
        }

        /*private void RegenerateMap()
        {
            SEED = new Random().Next();
            NoiseGenerator.SetSeed(SEED);

            _tileMapVisual = new Tilemap.VisualTileMap(game.GraphicsDevice, 24, 18, SEED);
            _tileMapCollisions = new Tilemap.CollisionTileMap(game.GraphicsDevice, 24, 18, SEED);
            _structureTileMap = new Tilemap.StructureTileMap(game.GraphicsDevice, 24, 18, SEED);

            _tileMapCollisions.ExtraColliders.Clear();
            _tileMapCollisions.ExtraColliders.AddRange(_structureTileMap.StoneBridgeColliders);

            _mapTexture = _tileMapVisual.GenerateMapTexture(game.GraphicsDevice, 1024, 768, _structureTileMap);
        }*/

        public override void Draw(SpriteBatch spriteBatch)
        {
            game.GraphicsDevice.Clear(Color.DimGray);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.Transform);
            _tileMapVisual.Draw(spriteBatch, _player);
            _structureTileMap.DrawBackgroundLayer(spriteBatch, _player);
            _foliageManager.DrawBehind(spriteBatch, _player);
            _player.Draw(spriteBatch);
            _foliageManager.DrawInFront(spriteBatch, _player);
            _structureTileMap.DrawForegroundLayer(spriteBatch, _player);
            if (ShowCollisionDebug)
            {
                _tileMapCollisions.DrawDebug(spriteBatch, TextureManager.DEBUG_Collider, _player.Position);
            }
            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _hudManager.Draw(spriteBatch);

            if (_showMap && _mapTexture != null)
            {
                spriteBatch.Draw(_mapTexture, new Vector2(448, 24), Color.White);
                Point playerChunk = _tileMapVisual.GetChunkCoordinates((int)_player.Position.X, (int)_player.Position.Y);
                Vector2 highlightPosition = new Vector2(448, 24) +
                                             new Vector2(playerChunk.X * _chunkWidth, playerChunk.Y * _chunkHeight);
                spriteBatch.Draw(_highlightTexture, highlightPosition, null, Color.White * 0.5f,
                                 0f, Vector2.Zero, new Vector2(_chunkWidth, _chunkHeight), SpriteEffects.None, 0f);
            }

            _simplePerformance.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void UpdateShmacks(int reward)
        {
            _hudManager.IncreaseShmackAmount(reward);

            Console.WriteLine($"updated by {reward}");
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
