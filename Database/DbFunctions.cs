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
            using (SQLiteConnection conn = CreateAndOpenConnection())
            {
                //AddTestEntry(conn);
                EnsureUser1IsAdmin();
                //DisplayAllUsers(conn);
            }

            Console.WriteLine("Done!");
        }

        private SQLiteConnection CreateAndOpenConnection()
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
            Console.WriteLine("hashpass after con");
            if (string.IsNullOrEmpty(salt))
                return "0";
            Console.WriteLine(salt);
            return Hasher(password + salt);
        }

        public bool AuthenticateUser(string username, string password)
        {
            using (SQLiteConnection conn = CreateAndOpenConnection())
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
            using (SQLiteConnection conn = CreateAndOpenConnection())
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
                using (SQLiteConnection conn = CreateAndOpenConnection())
                {
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE username = @username";
                    using (SQLiteCommand cmd = new SQLiteCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        int userCount = Convert.ToInt32(cmd.ExecuteScalar());
                        if (userCount > 0)
                        {
                            return false; // username isn't unique
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
            using (SQLiteConnection conn = CreateAndOpenConnection())
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

        private void EnsureUser1IsAdmin()
        {
            using (SQLiteConnection conn = CreateAndOpenConnection())
            {
                string updateQuery = "UPDATE Users SET admin = 1 WHERE user_id = 1";
                using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //-------------------------------------------------------------------------------------------
        static void AddTestEntry(SQLiteConnection conn) // DEBUG
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

        static void DisplayAllUsers(SQLiteConnection conn) // DEBUG
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

        public string HashPass(string username, string password)
        {
            using (SQLiteConnection conn = CreateAndOpenConnection())
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
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            int middle = inputBytes.Length / 2;
            byte[] p1 = new byte[middle];
            byte[] p2 = new byte[inputBytes.Length - middle];
            Array.Copy(inputBytes, 0, p1, 0, middle);
            Array.Copy(inputBytes, middle, p2, 0, p2.Length);
            for (int i = 0; i < p1.Length; i++)
            {
                p1[i] ^= p2[i];
                p2[i] >>= 1;
            }
            for (int i = 0; i < p1.Length; i++)
            {
                p1[i] = (byte)(p1[i] * 6653 * i);
                p2[i] = (byte)(9091 - p2[i] * 37 * i * 2);
            }
            List<byte> interleaved = new List<byte>();
            int minLength = Math.Min(p1.Length, p2.Length);

            for (int i = 0; i < minLength; i++)
            {
                interleaved.Add(p1[i]);
                interleaved.Add(p2[i]);
            }

            if (p1.Length > minLength)
                interleaved.AddRange(p1.Skip(minLength));

            if (p2.Length > minLength)
                interleaved.AddRange(p2.Skip(minLength));

            byte[] interleavedArray = interleaved.ToArray();
            byte[] part1 = interleavedArray.Take(48).ToArray();
            byte[] remaining = interleavedArray.Skip(48).ToArray();

            List<byte> part2List = new List<byte>(remaining);
            while (part2List.Count < 48)
            {
                part2List.AddRange(remaining);
            }
            byte[] part2 = part2List.Take(48).ToArray();

            byte[] xorResult = new byte[36];
            for (int i = 0; i < 36; i++)
            {
                xorResult[i] = (byte)(part1[i] ^ part2[i]);
            }

            string base64String = Convert.ToBase64String(xorResult.ToArray());
            Console.WriteLine(base64String);
            Console.WriteLine(base64String.Length);

            return base64String;
        }


        public List<string[]> GetAllUserData()
        {
            List<string[]> data = new List<string[]>();

            using (SQLiteConnection conn = CreateAndOpenConnection())
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

        public void UpdateUserData(int userId, string columnName, string newValue)
        {
            using (SQLiteConnection conn = CreateAndOpenConnection())
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
    }
}
