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
        private Player player;
        private Camera camera;
        private Tilemap.VisualTileMap tileMapVisual;
        private Tilemap.CollisionTileMap tileMapCollisions;
        private Tilemap.StructureTileMap structureTileMap;
        private FoliageManager foliageManager;
        private GUI.SimplePerformance simplePerformance;
        private SpriteFont font;
        private Texture2D mapTexture;
        private Texture2D highlightTexture;
        private bool showMap = true;
        private float chunkWidth = 1024f / 24f;
        private float chunkHeight = 768f / 18f;
        public List<Rectangle> ExtraColliders { get; set; } = new List<Rectangle>();

        public static bool ShowCollisionDebug = false;

        // Timer to track map regeneration
        private float mapRegenTimer = 0f;
        private const float REGEN_INTERVAL = 3f;

        private HUDManager HUDManager;

        public MOVEDEBUGTEST(Main game)
        {
            this.game = game;
            NoiseGenerator.SetSeed(SEED);
        }

        public override void LoadContent()
        {
            font = game.Content.Load<SpriteFont>("DefaultFont");
            simplePerformance = new GUI.SimplePerformance(font);

            tileMapVisual = new Tilemap.VisualTileMap(game.GraphicsDevice, 24, 18, SEED);
            tileMapCollisions = new Tilemap.CollisionTileMap(game.GraphicsDevice, 24, 18, SEED);
            structureTileMap = new Tilemap.StructureTileMap(game.GraphicsDevice, 24, 18, SEED);

            foliageManager = new FoliageManager(game, tileMapVisual, SEED);

            tileMapCollisions.ExtraColliders.AddRange(structureTileMap.StoneBridgeColliders);

            player = new Player(game.GraphicsDevice, new Vector2(150 * 48, 384 * 48));
            camera = new Camera();

            game.pauseCurrentSceneUpdateing = false;

            highlightTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            highlightTexture.SetData(new Color[] { Color.White });
            tileMapVisual.InitializeMapTexture(game.GraphicsDevice, 1024, 768, structureTileMap);
            mapTexture = tileMapVisual.UpdateMapTexture(game.GraphicsDevice);

            HUDManager = new HUDManager();
            HUDManager.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            showMap = ks.IsKeyDown(Keys.Tab);

            foliageManager.UpdateMinigames(player);

            // Increment the timer and regenerate the map every 5 seconds.
            mapRegenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (mapRegenTimer >= REGEN_INTERVAL)
            {
                mapRegenTimer = 0f;
                mapTexture = tileMapVisual.UpdateMapTexture(game.GraphicsDevice);
            }

            /*if (!game.InMiniGame && ks.IsKeyDown(Keys.E))
            {
                game.StartMiniGame(SubGameState.Tetris);
                game.InMiniGame = true;
            }*/

            simplePerformance.Update(gameTime);
            player.Update(tileMapCollisions);
            camera.Update(player.Position);
        }

        /*private void RegenerateMap()
        {
            SEED = new Random().Next();
            NoiseGenerator.SetSeed(SEED);

            tileMapVisual = new Tilemap.VisualTileMap(game.GraphicsDevice, 24, 18, SEED);
            tileMapCollisions = new Tilemap.CollisionTileMap(game.GraphicsDevice, 24, 18, SEED);
            structureTileMap = new Tilemap.StructureTileMap(game.GraphicsDevice, 24, 18, SEED);

            tileMapCollisions.ExtraColliders.Clear();
            tileMapCollisions.ExtraColliders.AddRange(structureTileMap.StoneBridgeColliders);

            mapTexture = tileMapVisual.GenerateMapTexture(game.GraphicsDevice, 1024, 768, structureTileMap);
        }*/

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Clear the screen.
            game.GraphicsDevice.Clear(Color.DimGray);

            // Draw world elements with the camera transform.
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);
            tileMapVisual.Draw(spriteBatch, player);
            structureTileMap.DrawBackgroundLayer(spriteBatch, player);
            foliageManager.DrawBehind(spriteBatch, player);
            player.Draw(spriteBatch);
            foliageManager.DrawInFront(spriteBatch, player);
            structureTileMap.DrawForegroundLayer(spriteBatch, player);
            if (ShowCollisionDebug)
            {
                tileMapCollisions.DrawDebug(spriteBatch, TextureManager.DEBUG_Collider, player.Position);
            }
            spriteBatch.End();

            // Draw HUD elements in screen space (without the camera transform).
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            HUDManager.Draw(spriteBatch);

            // Optionally draw other HUD items (like the map).
            if (showMap && mapTexture != null)
            {
                spriteBatch.Draw(mapTexture, new Vector2(448, 24), Color.White);
                Point playerChunk = tileMapVisual.GetChunkCoordinates((int)player.Position.X, (int)player.Position.Y);
                Vector2 highlightPosition = new Vector2(448, 24) +
                                             new Vector2(playerChunk.X * chunkWidth, playerChunk.Y * chunkHeight);
                spriteBatch.Draw(highlightTexture, highlightPosition, null, Color.White * 0.5f,
                                 0f, Vector2.Zero, new Vector2(chunkWidth, chunkHeight), SpriteEffects.None, 0f);
            }

            // Draw performance metrics.
            simplePerformance.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void UpdateShmacks(int reward)
        {
            HUDManager.IncreaseShmackAmount(reward);

            Console.WriteLine($"updated by {reward}");
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
