using CompSci_NEA.Entities;
using CompSci_NEA.Tilemap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private Texture2D playerTexture;
        private Tilemap.TileMapVisual tileMapVisual;
        private Tilemap.TileMapCollisions tileMapCollisions;

        public MOVEDEBUGTEST(Main game)
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            tileMapVisual = new Tilemap.TileMapVisual(game.GraphicsDevice, 20, 30);
            tileMapCollisions = new Tilemap.TileMapCollisions(game.GraphicsDevice, 5, 5);
            player = new Player(game.GraphicsDevice, new Vector2(-50, -50));
            camera = new Camera();

            tileMapVisual.GenerateTiles(game.GraphicsDevice);
            tileMapCollisions.GenerateTiles(game.GraphicsDevice);

            //tileMapVisual.FillSolidArea(game.GraphicsDevice, 0, 0, 10, 10, Color.Blue);

            //tileMapCollisions.FillSolidArea(game.GraphicsDevice, 0, 0, 10, 10, Color.Transparent);

            game.pauseCurrentSceneUpdateing = false;
        }

        public override void Update(GameTime gameTime)
        {
            player.Update(tileMapCollisions);
            camera.Update(player.Position);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            game.GraphicsDevice.Clear(Color.DimGray);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);
            tileMapVisual.Draw(spriteBatch);
            player.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
