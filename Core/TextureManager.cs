using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.Core
{
    public static class TextureManager
    {
        public static Texture2D TESTGRASS { get; private set; }
        public static Texture2D TESTWATER { get; private set; }
        public static Texture2D ATLAS { get; private set; }
        public static Texture2D DEBUG_Collider { get; private set; }
        public static Texture2D MainMenuBG { get; private set; }
        public static Texture2D MainMenuClouds { get; private set; }
        public static Song MainMenuMusic { get; private set; }
        public static Texture2D Shoreline { get; private set; }
        public static Texture2D ShorelineBG { get; private set; }
        public static Texture2D FoliageAtlas { get; private set; }
        public static SpriteFont DefaultFont { get; private set; }
        public static Texture2D Tetromino_texture { get; private set; }
        public static Texture2D Shmacks { get; private set; }
        public static Texture2D Disc {  get; private set; }


        public static void LoadContent(ContentManager content)
        {
            Console.WriteLine("loading textures");
            TESTGRASS = content.Load<Texture2D>("TESTGRASS");
            TESTWATER = content.Load<Texture2D>("TESTWATER");
            ATLAS = content.Load<Texture2D>("ATLASV9");
            DEBUG_Collider = content.Load<Texture2D>("DEBUG_Collide");
            MainMenuBG = content.Load<Texture2D>("MainMenuBG");
            MainMenuClouds = content.Load<Texture2D>("MenuClouds");
            MainMenuMusic = content.Load<Song>("MainMenuMusic");
            Shoreline = content.Load<Texture2D>("ShorelineTitle");
            ShorelineBG = content.Load<Texture2D>("ShorelineTitleBG");
            FoliageAtlas = content.Load<Texture2D>("FoliageAtlas");
            DefaultFont = content.Load<SpriteFont>("DefaultFont");
            Tetromino_texture = content.Load<Texture2D>("tetromino_texture");
            Shmacks = content.Load<Texture2D>("Shmacks");
            Disc = content.Load<Texture2D>("DiscTexture");


            if (TESTGRASS == null) throw new Exception("Failed to load TESTGRASS texture.");
            if (TESTWATER == null) throw new Exception("Failed to load TESTWATER texture.");
            if (ATLAS == null) throw new Exception("Failed to load ATLAS texture.");
            if (DEBUG_Collider == null) throw new Exception("Failed to load DEBUG_Collider texture.");
            if (MainMenuBG == null) throw new Exception("Failed to load MainMenuBG texture.");
            if (MainMenuClouds == null) throw new Exception("Failed to load MainMenuClouds texture.");
            if (MainMenuMusic == null) throw new Exception("Failed to load MainMenuMusic song");
            if (FoliageAtlas == null) throw new Exception("Failed to load Foliage Atlas");

        }
    }
}
