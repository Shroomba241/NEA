using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompSci_NEA.Core;
using Newtonsoft.Json;

namespace CompSci_NEA.Database
{
    public static class SaveManager
    {
        public static void SaveGame(GameSave save, string filePath)
        {
            string json = JsonConvert.SerializeObject(save, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static GameSave LoadGame(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            string json = File.ReadAllText(filePath);
            GameSave save = JsonConvert.DeserializeObject<GameSave>(json);
            return save;
        }
    }
}
