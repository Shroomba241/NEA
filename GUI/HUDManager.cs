using CompSci_NEA.Core;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace CompSci_NEA.GUI
{
    public class HUDManager
    {
        public enum HUDState
        {
            Off,
            Roaming,
            Map
        }
        private Vector2 _offscreen = new Vector2(-500, -500);
        private Icon _shmacks;
        private string _shmackNumber = "5";
        public HUDState currentState = HUDState.Roaming;
        private CompSci_NEA.Database.DbFunctions _dbFunctions;

        public HUDManager()
        {
            _dbFunctions = new CompSci_NEA.Database.DbFunctions();
        }

        public void LoadContent()
        {
            List<string[]> userData = _dbFunctions.GetAllUserData();
            string coins = "0";
            foreach (var row in userData)
            {
                if (row[0] == Main.LoggedInUserID.ToString()) //check the user data has been grabbed correctly
                {
                    coins = row[3];
                    break;
                }
            }
            _shmackNumber = coins;
            _shmacks = new Icon(TextureManager.Shmacks, new Vector2(1400, 50), Color.White, 3f, _shmackNumber, new Vector2(50, 15));
        }

        public void IncreaseShmackAmount(int increase)
        {
            int currentCoins = int.Parse(_shmackNumber); //get current number of smacks
            int newCoins = currentCoins + increase;

            Console.WriteLine($"{currentCoins} - {increase} - {newCoins}");
            _shmackNumber = newCoins.ToString(); //update hud of smack number
            _shmacks.UpdateText(_shmackNumber);
            _dbFunctions.UpdateUserData(Main.LoggedInUserID, "coins", newCoins.ToString()); //commit shmack number to the db
            Console.WriteLine($"updated by - {increase}");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _shmacks.Draw(spriteBatch);
        }
    }
}
