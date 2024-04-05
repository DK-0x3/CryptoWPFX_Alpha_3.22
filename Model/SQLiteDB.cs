using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWPFX.Class
{
    public class SQLiteDB
    {
        public SQLiteDB()
        {
            var connection = new SqliteConnection("Data Source=CryptoData.db");
            connection.Open();
            SqliteCommand command = new SqliteCommand();
            try
            {
                command.Connection = connection;
                command.CommandText = "CREATE TABLE Favorites(_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, TokenId TEXT NOT NULL)";
                command.ExecuteNonQuery();
            }
            catch { }
        }

        private SqliteConnection Conn()
        {
            var connection = new SqliteConnection("Data Source=CryptoData.db");
            connection.Open();
            return connection;
        }

        public void AddFavorites(string TokenID)
        {
            SqliteCommand command = new SqliteCommand();
            command.Connection = Conn();
            command.CommandText = $"INSERT INTO Favorites (TokenId) VALUES ('{TokenID}')";
            command.ExecuteNonQuery();
        }

        public void DelFavorites(string TokenID)
        {
            SqliteCommand command = new SqliteCommand();
            command.Connection = Conn();
            command.CommandText = $"DELETE FROM Favorites WHERE TokenId = '{TokenID}'";
            command.ExecuteNonQuery();
        }

        public List<string> GetListFavorites()
        {
            var listF = new List<string>();

            SqliteCommand command = new SqliteCommand("SELECT * FROM Favorites", Conn());

            using SqliteDataReader reader = command.ExecuteReader();
            if (reader.HasRows) // если есть данные
            {
                while (reader.Read())   // построчно считываем данные
                {
                    listF.Add(reader["TokenId"].ToString());
                }
                return listF;
            }
            else
            {
                return listF;
            }
        }
    }
}
