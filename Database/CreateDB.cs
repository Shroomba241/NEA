using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace CompSci_NEA.Database
{
    public class CreateDB
    {
        public void CreateDatabase()
        {
            string connectionString = "Data Source=database.db;Version=3;";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string createUserTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT UNIQUE NOT NULL,
                        password_hash TEXT NOT NULL,
                        salt TEXT NOT NULL,
                        admin BOOL DEFAULT 0,
                        coins INTEGER DEFAULT 0
                    );";

                string createWorldsTable = @"
                    CREATE TABLE IF NOT EXISTS Worlds (
                        world_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        seed TEXT UNIQUE NOT NULL,
                        creation_date TEXT DEFAULT CURRENT_TIMESTAMP,
                        difficulty INTEGER DEFAULT 10
                    );";

                string createUserWorldSaveTable = @"
                CREATE TABLE IF NOT EXISTS UserWorldSaves (
                    user_id INTEGER,
                    world_id INTEGER,
                    last_played TEXT DEFAULT CURRENT_TIMESTAMP,
                    location_x INTEGER DEFAULT 0,
                    location_y INTEGER DEFAULT 0,
                    PRIMARY KEY (user_id, world_id),
                    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE,
                    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(createUserTable, conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Users table created");
                }

                using (SQLiteCommand cmd = new SQLiteCommand(createWorldsTable, conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Worlds table created");
                }
                using (SQLiteCommand cmd = new SQLiteCommand(createUserWorldSaveTable, conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("User World Save table created");
                }
            }
        }
    }
}
