using CompSci_NEA.Entities;
using CompSci_NEA.Tilemap;
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
        private Main game;
        private Player player;
        private Camera camera;
        private Tilemap.TileMapVisual tileMapVisual;
        private Tilemap.TileMapCollisions tileMapCollisions;
        private Tilemap.StructureTileMap structureTileMap;
        private GUI.SimplePerformance simplePerformance;
        private SpriteFont font;
        private Texture2D mapTexture;
        private Texture2D highlightTexture;
        private bool showMap = true;

        public MOVEDEBUGTEST(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            font = game.Content.Load<SpriteFont>("DefaultFont");
            simplePerformance = new GUI.SimplePerformance(font);

            // Initialize the tilemaps with the GraphicsDevice.
            tileMapVisual = new Tilemap.TileMapVisual(game.GraphicsDevice, 16, 12);
            tileMapCollisions = new Tilemap.TileMapCollisions(game.GraphicsDevice, 16, 12);
            structureTileMap = new Tilemap.StructureTileMap(game.GraphicsDevice, 16, 12);

            // Initialize the player at a valid position.
            player = new Player(game.GraphicsDevice, new Vector2(50, 50));
            camera = new Camera();

            // No need for tile generation here since it's handled in the BaseTileMap logic.
            game.pauseCurrentSceneUpdateing = false;

            highlightTexture = new Texture2D(game.GraphicsDevice, 1, 1); // 1x1 texture
            highlightTexture.SetData(new Color[] { Color.White }); // Set color to white

            mapTexture = tileMapVisual.GenerateMapTexture(game.GraphicsDevice, 1024, 1024);


        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Tab))
            {
                showMap = true;
            }
            else showMap = false;

            simplePerformance.Update(gameTime);
            player.Update(tileMapCollisions); // Update the player based on tile collisions
            camera.Update(player.Position); // Update the camera based on player position
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            game.GraphicsDevice.Clear(Color.DimGray);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);

            // Draw the visual representation of the tilemap, passing the player for chunk drawing.
            tileMapVisual.Draw(spriteBatch, player);
            player.Draw(spriteBatch);
            structureTileMap.DrawStructures(spriteBatch, player);
            spriteBatch.End();

            // Draw performance metrics.
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            if(showMap && mapTexture != null)
            {
                spriteBatch.Draw(mapTexture, new Vector2(448, 24), Color.White);
                Point playerChunk = tileMapVisual.GetChunkCoordinates((int)player.Position.X, (int)player.Position.Y);
                Vector2 highlightPosition = new Vector2(448, 24) + new Vector2(playerChunk.X * (1024 / 16), playerChunk.Y * (1024 / 16));
                spriteBatch.Draw(highlightTexture, highlightPosition, null, Color.White * 0.5f, 0f, Vector2.Zero, new Vector2(1024 / 16, 1024 / 16), SpriteEffects.None, 0f);
            }
            simplePerformance.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void Shutdown()
        {
            // Implement any necessary cleanup logic here
            throw new NotImplementedException();
        }


    }
}
