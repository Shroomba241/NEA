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
using System.IO;
using CompSci_NEA.Database;
using CompSci_NEA.WorldGeneration.Structures;

namespace CompSci_NEA.Scenes
{
    public class MOVEDEBUGTEST : Scene
    {
        public static int SEED = 4717;
        //public static int SEED;
        private Main game;
        private GameSave save;
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
        public List<Rectangle> ExtraColliders = new List<Rectangle>();

        public static bool ShowCollisionDebug = false;
        private float _mapRegenTimer = 0f;
        private const float REGEN_INTERVAL = 1.5f;

        private HUDManager _hudManager;
        public int Shmacks;
        private float _escapeCooldown = 0f;
        private bool _escapePressed = false;

        public bool UnlockMap,UnlockBridge1,UnlockBridge2 = false;

        public MOVEDEBUGTEST(Main game, GameSave save)
        {
            this.game = game;
            this.save = save;
            NoiseGenerator.SetSeed(SEED);
        }

        public override void LoadContent()
        {
            NoiseGenerator.SetSeed(SEED);
            _font = game.Content.Load<SpriteFont>("DefaultFont");
            _simplePerformance = new GUI.SimplePerformance(_font);

            _tileMapVisual = new Tilemap.VisualTileMap(game.GraphicsDevice, 24, 18, SEED);
            _tileMapCollisions = new Tilemap.CollisionTileMap(game.GraphicsDevice, 24, 18, SEED);
            _structureTileMap = new Tilemap.StructureTileMap(game.GraphicsDevice, 24, 18, SEED);

            _foliageManager = new FoliageManager(game, _tileMapVisual, SEED);
            _tileMapCollisions.ExtraColliders.AddRange(_structureTileMap.GetAllColliders());

            if (save.Minigames == null || save.Minigames.Minigames == null)
            {
                save.Minigames = new ExistingMinigames { Minigames = new List<MinigameInfo>() };
            }

            if (save.Minigames.Minigames.Count == 0)
            {
                _foliageManager.UseSavedMinigames = false;
                _foliageManager.ClearMinigameFoliage();
                save.Minigames.Minigames = _foliageManager.GenerateAllMinigameInfo();
            }
            else
            {
                _foliageManager.UseSavedMinigames = true;
                _foliageManager.ClearMinigameFoliage();
                _foliageManager.LoadMinigamesFromSave(save);
            }

            DbFunctions db = new DbFunctions();
            int userId = Main.LoggedInUserID;
            int worldId = 1; //assumes worldid is 1.
            var (locationX, locationY, coins, savePath) = db.GetUserWorldSave(userId, worldId);
            var (seed, creation_date, difficulty) = db.GetWorldData(worldId);
            Console.WriteLine($"9876 {save.PodiumGoalRemaining}, {difficulty}");
            if (save.PodiumGoalRemaining <= 0 || save.PodiumGoalRemaining > difficulty)
                save.PodiumGoalRemaining = difficulty;
            Console.WriteLine($"9876 {save.PodiumGoalRemaining}, {difficulty}");
            _structureTileMap.Podium.GoalAmount = save.PodiumGoalRemaining;
            Console.WriteLine($"DB State: location=({locationX},{locationY}), coins={coins}, savePath={savePath}");
            Shmacks = coins;
            if (locationX != 0 || locationY != 0)
                _player = new Player(game.GraphicsDevice, new Vector2(locationX, locationY), TextureManager.PlayerMoveAtlas);
            else
                _player = new Player(game.GraphicsDevice, new Vector2(150 * 48, 384 * 48), TextureManager.PlayerMoveAtlas);

            _camera = new Camera();
            game.pauseCurrentSceneUpdating = false;

            _highlightTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            _highlightTexture.SetData(new Color[] { Color.White });
            _tileMapVisual.InitializeMapTexture(game.GraphicsDevice, 1024, 768, _structureTileMap);
            _mapTexture = _tileMapVisual.UpdateMapTexture(game.GraphicsDevice);

            _hudManager = new HUDManager();
            _hudManager.LoadContent();

            _structureTileMap.Shop.Interact(game, save, true);

            if (save.ShopInventory != null && save.ShopInventory.BoughtItems != null)
            {
                foreach (var item in save.ShopInventory.BoughtItems)
                {
                    switch (item.Name.ToLower())
                    {
                        case "map":
                            UnlockMap = true;
                            break;
                        case "key 1":
                            UnlockBridge1 = true;
                            UnlockBridgeColliders(1);
                            break;
                        case "key 2":
                            UnlockBridge2 = true;
                            UnlockBridgeColliders(2);
                            break;
                    }
                }
            }

            _hudManager.SetShmackAmount(Shmacks);

            //game.StartMiniGame(SubGameState.Connect4);
        }

        public override void Update(GameTime gameTime)
        {
            _structureTileMap.Shop.DrawInForeground = (_player.Position.Y + 100 < _structureTileMap.Shop.GetInteractionPoint().Y);
            _structureTileMap.Podium.PlayerFeet = _player.Position.Y + 100;

            Rectangle podiumArea = new Rectangle((int)_structureTileMap.Podium.Position.X, (int)_structureTileMap.Podium.Position.Y, 48 * 3, 48 * 3);
            Rectangle playerFeetRect = new Rectangle((int)_player.Position.X, (int)(_player.Position.Y + 96), 10, 10);

            bool isOnPodium = podiumArea.Intersects(playerFeetRect);
            if (isOnPodium && !_structureTileMap.Podium.PlayerWasOnPodium && Shmacks > 0)
            {
                _structureTileMap.Podium.Donate(Shmacks);
                UpdateShmacks(-Shmacks);
                _structureTileMap.Podium.PlayerWasOnPodium = true;
            }
            else if (!isOnPodium)
            {
                _structureTileMap.Podium.PlayerWasOnPodium = false;
            }
            /*foreach (var s in _structureTileMap.Bridges)
            {
                if (s is WoodBridge wb)
                    _tileMapCollisions.ClearCollisionUnderBridge(wb);
            }*/

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Escape))
            {
                if (!_escapePressed && _escapeCooldown <= 0f)
                {
                    SaveGame();
                    _escapePressed = true;
                    _escapeCooldown = 1f;
                }
            }
            else
            {
                _escapePressed = false;
            }
            if (_escapeCooldown > 0f)
                _escapeCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            _showMap = ks.IsKeyDown(Keys.Tab);

            _foliageManager.UpdateMinigames(_player, save);
            _mapRegenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_mapRegenTimer >= REGEN_INTERVAL)
            {
                _mapRegenTimer = 0f;
                _mapTexture = _tileMapVisual.UpdateMapTexture(game.GraphicsDevice);
            }
            _simplePerformance.Update(gameTime);
            _player.Update(_tileMapCollisions, gameTime);
            _camera.Update(_player.Position);

            if (ks.IsKeyDown(Keys.E))
            {
                float distance = Vector2.Distance(_player.Position, _structureTileMap.Shop.GetInteractionPoint());
                if (distance < 100f)
                {
                    _structureTileMap.Shop.Interact(game, save, false);
                }
            }
        }

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
            if (!UnlockBridge1)
            {
                var pos = ((WoodBridge)_structureTileMap.Bridges[0]).Position;
                spriteBatch.Draw(TextureManager.Padlock, new Rectangle((int)pos.X + 144, (int)pos.Y + 24, 64, 64), Color.White);
            }
            if (!UnlockBridge2)
            {
                var pos = ((WoodBridge)_structureTileMap.Bridges[1]).Position;
                spriteBatch.Draw(TextureManager.Padlock, new Rectangle((int)pos.X + 144, (int)pos.Y + 24, 64, 64), Color.White);
            }
            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _hudManager.Draw(spriteBatch);

            if (_showMap && _mapTexture != null && UnlockMap)
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
            Shmacks += reward;
            _hudManager.SetShmackAmount(Shmacks);
            if (reward > 0)
            {
                DbFunctions db = new DbFunctions();
                List<string[]> data = db.GetAllUserData();
                foreach (var row in data)
                {
                    if (row[0] == Main.LoggedInUserID.ToString())
                    {
                        int current = int.Parse(row[3]);
                        db.UpdateUserData(Main.LoggedInUserID, "coins", (current + reward).ToString());
                        break;
                    }
                }
            }
            SaveGame();
        }

        public void SaveGame()
        {
            save.PodiumGoalRemaining = _structureTileMap.Podium.GoalAmount;
            string filePath = Path.Combine("Saves", Main.LoggedInUserID.ToString(), save.Slot);
            SaveManager.SaveGame(save, filePath);
            DbFunctions db = new DbFunctions();
            db.UpdateUserWorldSavesData(Main.LoggedInUserID, 1, (int)_player.Position.X, (int)_player.Position.Y, Shmacks, filePath);
            Console.WriteLine($"location=({_player.Position.X}, {_player.Position.Y}), coins={Shmacks}, save_path={filePath}");
        }

        /*public void UpdateGameSaveMinigames(List<MinigameInfo> newMinigames)
        {
            save.Minigames.Minigames = newMinigames;
        }*/

        public void UnlockBridgeColliders(int n)
        {
            _tileMapCollisions.ClearCollisionUnderBridge((WoodBridge)_structureTileMap.Bridges[n-1]);
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
