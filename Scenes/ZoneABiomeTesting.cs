using CompSci_NEA.Core;
using CompSci_NEA.Entities;
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
    public class ZoneABiomeTesting : Scene
    {
        public static int SEED = 4741;
        private Main game;
        private Player player;
        private Camera camera;
        private Tilemap.TileMapVisual tileMapVisual;
        private Tilemap.TileMapCollisions tileMapCollisions;
        private Tilemap.StructureTileMap structureTileMap;
        private GUI.SimplePerformance simplePerformance;
        private SpriteFont font;
        private Texture2D mapTexture;
        private Texture2D tempMapTexture;
        private Texture2D moistMapTexture;
        private Texture2D highlightTexture;
        private bool showMap = true;
        private float chunkWidth = 1024f / 24f;
        private float chunkHeight = 768f / 18f;
        public List<Rectangle> ExtraColliders { get; set; } = new List<Rectangle>();

        public static bool ShowCollisionDebug = false;

        public ZoneABiomeTesting(Main game)
        {
            this.game = game;
            NoiseGenerator.SetSeed(SEED);
        }

        public override void LoadContent()
        {
            font = game.Content.Load<SpriteFont>("DefaultFont");
            simplePerformance = new GUI.SimplePerformance(font);

            tileMapVisual = new Tilemap.TileMapVisual(game.GraphicsDevice, 12, 9, SEED);

            
            player = new Player(game.GraphicsDevice, new Vector2(150 * 48, 384 * 48));
            camera = new Camera();

            game.pauseCurrentSceneUpdateing = false;

            highlightTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            highlightTexture.SetData(new Color[] { Color.White });

            mapTexture = tileMapVisual.GenerateMapTexture(game.GraphicsDevice, 512, 384, structureTileMap);
            tempMapTexture = tileMapVisual.GenerateTemperatureTexture(game.GraphicsDevice, 384, 384);
            moistMapTexture = tileMapVisual.GenerateMoistureTexture(game.GraphicsDevice, 384, 384);

        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            showMap = ks.IsKeyDown(Keys.Tab);

            if (ks.IsKeyDown(Keys.F5))
            {
                SEED = new Random().Next();
                NoiseGenerator.SetSeed(SEED);

                tileMapVisual = new Tilemap.TileMapVisual(game.GraphicsDevice, 12, 9, SEED);

                mapTexture = tileMapVisual.GenerateMapTexture(game.GraphicsDevice, 512, 384, structureTileMap);
                tempMapTexture = tileMapVisual.GenerateTemperatureTexture(game.GraphicsDevice, 384, 384);
                moistMapTexture = tileMapVisual.GenerateMoistureTexture(game.GraphicsDevice, 384, 384);
            }

            simplePerformance.Update(gameTime);
            player.Update(tileMapCollisions);
            camera.Update(player.Position);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            game.GraphicsDevice.Clear(Color.DimGray);

            //spiritebatch affected by zoom
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);
            tileMapVisual.Draw(spriteBatch, player);
            //structureTileMap.DrawBackgroundLayer(spriteBatch, player);
            player.Draw(spriteBatch);
            //structureTileMap.DrawForegroundLayer(spriteBatch, player);
            /*if (ShowCollisionDebug)
            {
                tileMapCollisions.DrawDebug(spriteBatch, TextureManager.DEBUG_Collider, player.Position);
            }*/
            spriteBatch.End();

            //spritebatch not affected by zoom (HUD, map, etc)
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            if (showMap && mapTexture != null)
            {
                spriteBatch.Draw(tempMapTexture, new Vector2(24, 24), Color.White);
                spriteBatch.Draw(moistMapTexture, new Vector2(24, 424), Color.White);
                spriteBatch.Draw(mapTexture, new Vector2(550, 24), Color.White);
                Point playerChunk = tileMapVisual.GetChunkCoordinates((int)player.Position.X, (int)player.Position.Y);
                Vector2 highlightPosition = new Vector2(550, 24) +
                                             new Vector2(playerChunk.X * chunkWidth, playerChunk.Y * chunkHeight);
                spriteBatch.Draw(highlightTexture, highlightPosition, null, Color.White * 0.5f,
                                 0f, Vector2.Zero, new Vector2(chunkWidth, chunkHeight), SpriteEffects.None, 0f);
            }

            simplePerformance.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }


    }
}

