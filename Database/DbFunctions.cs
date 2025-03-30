using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CompSci_NEA.Database
{
    public class DbFunctions
    {
        private const string ConnectionString = "Data Source=database.db;Version=3;";

        public DbFunctions()
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                EnsureUser1IsAdmin();
            }

            Console.WriteLine("Done!");
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        private string ComputeHashPass(string username, string password, SQLiteConnection conn)
        {
            Console.WriteLine("hashpass inside con");
            string salt = "";
            string query = "SELECT salt FROM Users WHERE username = @username";
            using (var cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        salt = reader.GetString(0);
                    }
                }
            }
            //Console.WriteLine("hashpass after con");
            if (string.IsNullOrEmpty(salt))
                return "0";
            //Console.WriteLine(salt);
            return Hasher(password + salt);
        }

        public int GetUserIDFromUsername(string username)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "SELECT user_id FROM Users WHERE username = @username";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader["user_id"]);
                        }
                    }
                }
            }
            return -1;
        }


        public bool AuthenticateUser(string username, string password)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string passwordHash = ComputeHashPass(username, password, conn);
                string query = "SELECT COUNT(*) FROM Users WHERE username = @username AND password_hash = @passwordHash";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@passwordHash", passwordHash);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool IsUserAdmin(string username)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "SELECT admin FROM Users WHERE username = @username";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        int adminFlag = Convert.ToInt32(result);
                        return adminFlag == 1;
                    }
                }
            }
            return false;
        }

        public bool RegisterUser(string username, string password)
        {
            try
            {
                using (SQLiteConnection conn = OpenConnection())
                {
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE username = @username";
                    using (SQLiteCommand cmd = new SQLiteCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        int userCount = Convert.ToInt32(cmd.ExecuteScalar());
                        if (userCount > 0)
                        {
                            return false; //username isn't unique
                        }
                    }

                    AddUser(username, password);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RegisterUser: " + ex.Message);
                return false;
            }
        }

        public void AddUser(string username, string password)
        {
            try
            {
                using (SQLiteConnection conn = OpenConnection())
                {
                    var x = CreateHashPass(username, password, conn);
                    string passwordHash = x[0];
                    string salt = x[1];
                    string insertNewUser = @"
                    INSERT INTO Users (username, password_hash, salt) 
                    VALUES (@username, @passwordHash, @salt)
                    ";

                    using (SQLiteCommand cmd = new SQLiteCommand(insertNewUser, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@salt", salt);

                        cmd.ExecuteNonQuery();
                        DisplayAllUsers(conn);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Adduser" + ex.Message);
            }
        }

        private void EnsureUser1IsAdmin()
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string updateQuery = "UPDATE Users SET admin = 1 WHERE user_id = 1";
                using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddTetrisEntry(int userId, string username, int score)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = @"
                    INSERT INTO TetrisSessions (user_id, username, score)
                    VALUES (@userId, @username, @score);
                ";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<string[]> GetAllUserData()
        {
            List<string[]> data = new List<string[]>();

            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "SELECT user_id, username, admin, coins FROM Users";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string userId = reader["user_id"].ToString();
                        string username = reader["username"].ToString();
                        string admin = reader["admin"].ToString();
                        string coins = reader["coins"].ToString();
                        data.Add(new string[] { userId, username, admin, coins });
                    }
                }
            }
            //Console.WriteLine(string.Join(" | ", data.Select(row => string.Join(", ", row))));

            return data;
        }

        public List<string[]> GetTetrisData(int userId)
        {
            List<string[]> data = new List<string[]>();

            using (SQLiteConnection conn = OpenConnection())
            {
                string query = @"
                    SELECT session_id, user_id, username, score 
                    FROM TetrisSessions 
                    WHERE user_id = @userId 
                    ORDER BY score DESC;
                ";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string sessionId = reader["session_id"].ToString();
                            string uid = reader["user_id"].ToString();
                            string username = reader["username"].ToString();
                            string score = reader["score"].ToString();
                            data.Add(new string[] { sessionId, uid, username, score});
                        }
                    }
                }
            }

            return data;
        }

        public void UpdateUserData(int userId, string columnName, string newValue)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = $"UPDATE Users SET {columnName} = @newValue WHERE user_id = @userId";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@newValue", newValue);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateUserWorldSavesData(int userId, int worldId, int location_x, int location_y, int coins, string savePath)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "INSERT OR REPLACE INTO UserWorldSaves (user_id, world_id, last_played, location_x, location_y, coins, save_path) " +
                               "VALUES (@userId, @worldId, CURRENT_TIMESTAMP, @location_x, @location_y, @coins, @savePath)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@worldId", worldId);
                    cmd.Parameters.AddWithValue("@location_x", location_x);
                    cmd.Parameters.AddWithValue("@location_y", location_y);
                    cmd.Parameters.AddWithValue("@coins", coins);
                    cmd.Parameters.AddWithValue("@savePath", savePath);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public (int locationX, int locationY, int coins, string savePath) GetUserWorldSave(int userId, int worldId)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "SELECT location_x, location_y, coins, save_path FROM UserWorldSaves WHERE user_id = @userId AND world_id = @worldId";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@worldId", worldId);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int locationX = Convert.ToInt32(reader["location_x"]);
                            int locationY = Convert.ToInt32(reader["location_y"]);
                            int coins = Convert.ToInt32(reader["coins"]);
                            string savePath = reader["save_path"].ToString();
                            return (locationX, locationY, coins, savePath);
                        }
                    }
                }
            }
            return (0, 0, 0, null);
        }

        public void DeleteUser(int userId)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "DELETE FROM Users WHERE user_id = @userId";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTetrisSession(int sessionId)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "DELETE FROM TetrisSessions WHERE session_id = @sessionId";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@sessionId", sessionId);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        //-------------------------------------------------------------------------------------------
        static void AddTestEntry(SQLiteConnection conn) //DEBUG
        {
            string insertTestUser = @"
                INSERT INTO Users (username, password_hash, admin, coins) 
                VALUES ('TestUser', '@pass', 1, 15)
            ";
            string insertTestWorld = @"
                INSERT INTO Worlds (seed, difficulty)
                VALUES ('123456789', '240107')
            ";
            string insertTestUserWorldSave = @"
                INSERT INTO UserWorldSaves (user_id, world_id, location_x, location_y)
                VALUES (1, 1, 12, 34)
            ";

            using (SQLiteCommand cmd = new SQLiteCommand(insertTestUser, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("added test user");
            }
            using (SQLiteCommand cmd = new SQLiteCommand(insertTestWorld, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("added test world");
            }
            using (SQLiteCommand cmd = new SQLiteCommand(insertTestUserWorldSave, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("added test user world save");
            }
        }

        static void DisplayAllUsers(SQLiteConnection conn) //DEBUG
        {
            string query = @"
                SELECT 
                    u.user_id, u.username, u.password_hash, u.salt, u.admin, u.coins,
                    w.world_id, w.seed, w.creation_date, w.difficulty,
                    uw.last_played, uw.location_x, uw.location_y
                FROM Users u
                LEFT JOIN UserWorldSaves uw ON u.user_id = uw.user_id
                LEFT JOIN Worlds w ON uw.world_id = w.world_id;";

            using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine("User Information:");
                    Console.WriteLine($"ID: {reader["user_id"]}, Username: {reader["username"]}, Coins: {reader["coins"]}");
                    Console.WriteLine($"Password Hash: {reader["password_hash"]}");
                    Console.WriteLine($"Salt: {reader["salt"]}, Is Admin: {reader["admin"]}");

                    Console.WriteLine("\nWorld Information:");
                    Console.WriteLine($"World ID: {reader["world_id"]}, Seed: {reader["seed"]}");
                    Console.WriteLine($"Created: {reader["creation_date"]}, Difficulty: {reader["difficulty"]}");

                    Console.WriteLine("\nSave Data:");
                    Console.WriteLine($"Last Played: {reader["last_played"]}");
                    Console.WriteLine($"Location: X = {reader["location_x"]}, Y = {reader["location_y"]}");
                    Console.WriteLine("------------------------------------------------------\n");
                }
            }
        }
        //--------------------------------------------------------------------------------------------------

        //password things start here
        public string HashPass(string username, string password)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                return ComputeHashPass(username, password, conn);
            }
        }

        public string[] CreateHashPass(string username, string password, SQLiteConnection conn)
        {
            string salt = GenerateRandomSalt();
            string hashedPassword = Hasher(password + salt);
            return new string[] { hashedPassword, salt };
        }

        public string GenerateRandomSalt(int length = 32)
        {
            byte[] saltBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes).Replace("=", "");
        }

        public string Hasher(string input)
        {
            Console.WriteLine(input);

            byte[] buf = Encoding.UTF8.GetBytes(input);
            // Split buf into p1 and p2
            int mid = buf.Length / 2;
            byte[] p1 = new byte[mid];
            byte[] p2 = new byte[buf.Length - mid];
            Array.Copy(buf, 0, p1, 0, mid);
            Array.Copy(buf, mid, p2, 0, p2.Length);
            //xor p1 and p2, but shift just p2
            for (int i = 0; i < p1.Length; i++)
            {
                p1[i] ^= p2[i];
                p2[i] >>= 1;
            }
            //arithmetic to scramble the data but determanistic, so should be difficult to reverse
            for (int i = 0; i < p1.Length; i++)
            {
                p1[i] = (byte)(p1[i] * 6653 * i);
                p2[i] = (byte)(9091 - p2[i] * 37 * i * 2);
            }
            //interleave p1 and p2 into mix. alternate bytes from each
            List<byte> mix = new List<byte>();
            int min = Math.Min(p1.Length, p2.Length);
            for (int i = 0; i < min; i++)
            {
                mix.Add(p1[i]);
                mix.Add(p2[i]);
            }
            if (p1.Length > min)
                mix.AddRange(p1.Skip(min));
            if (p2.Length > min)
                mix.AddRange(p2.Skip(min));
            byte[] mixArr = mix.ToArray();
            //split mixArr into two segments to prep for following xor
            //seg1 is the first 48 bytes; seg2 is built from the remaining bytes which are repeated
            byte[] seg1 = mixArr.Take(48).ToArray();
            byte[] rem = mixArr.Skip(48).ToArray();
            List<byte> seg2List = new List<byte>(rem);
            while (seg2List.Count < 48)
                seg2List.AddRange(rem);
            byte[] seg2 = seg2List.Take(48).ToArray();

            //xor first against second into a 36  byte result
            byte[] hash = new byte[36];
            for (int i = 0; i < 36; i++)
            {
                hash[i] = (byte)(seg1[i] ^ seg2[i]);
            }
            //convert byte hash to b64 to be returned
            string b64 = Convert.ToBase64String(hash);
            Console.WriteLine(b64);
            Console.WriteLine(b64.Length);

            return b64;
        }
    }
}
