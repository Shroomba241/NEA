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
        private TileMap tileMap;

        public MOVEDEBUGTEST(Main game) 
        {
            this.game = game;
        }

        public override void LoadContent()
        {
            tileMap = new TileMap(game.GraphicsDevice, 20, 30);
            player = new Player(game.GraphicsDevice, new Vector2(-50, -50)); 
            camera = new Camera();
        }

        public override void Update(GameTime gameTime) 
        {
            player.Update(tileMap);
            camera.Update(player.Position);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            game.GraphicsDevice.Clear(Color.DimGray);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);
            tileMap.Draw(spriteBatch);
            player.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
