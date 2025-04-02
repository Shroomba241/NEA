using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Data.SQLite;
using CompSci_NEA.Database;

//This file is super patchy. its basically just the data table used in the AdminView, but manipulated in a way so that it looks like a leaderboard.
//Later, move the LoadData to its right position within the minigame, and make it so it will work for future minigmes if i choose to put leaderboards in them depending on what i do.

namespace CompSci_NEA.GUI
{
    public class Leaderboard : DataTable
    {
        public Leaderboard(GraphicsDevice graphicsDevice, SpriteFont font, Vector2 position, float uiScale = 1.4f)
            : base(graphicsDevice, font, position, new string[] { "Score", "Username", "Age" }, LoadData(), uiScale)
        {
            //manipulating the DataTable for Leaderboard's purposes
            this.IgnoreMouseClicks = true;
            this.rowsPerPage = 15;
            this.previousButton.Move(new Vector2(-1000, -1000));
            this.nextButton.Move(new Vector2(-1000, -1000));
            this.pageIndicatorText.UpdateContent(string.Empty);
        }

        public static List<string[]> LoadData()
        {
            List<string[]> rows = new List<string[]>();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT score, username, session_time FROM TetrisSessions ORDER BY score DESC LIMIT 15";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int score = Convert.ToInt32(reader["score"]);
                        string username = reader["username"].ToString();
                        string sessionTimeStr = reader["session_time"].ToString();
                        DateTime sessionTime;
                        if (!DateTime.TryParse(sessionTimeStr, out sessionTime))
                            sessionTime = DateTime.Now;
                        TimeSpan age = DateTime.Now - sessionTime;
                        string ageStr = FormatTimeSpan(age);
                        rows.Add(new string[] { score.ToString(), username, ageStr });
                    }
                }
            }
            return rows;
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalDays >= 1)
                return $"{(int)ts.TotalDays} days";
            else if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours} hours";
            else if (ts.TotalMinutes >= 1)
                return $"{(int)ts.TotalMinutes} minutes";
            else
                return "Just now";
        }
    }
}
