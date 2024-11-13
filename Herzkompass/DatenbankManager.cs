using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Herzkompass
{
    public class DatabaseManager
    {
        public bool Datenbankverbindung { get; private set; } = false;
        public MySqlConnection Connection { get; private set; }

        private class Datenbank
        {
            public string Host { get; set; } = "localhost";
            public string Username { get; set; } = "root";
            public string Password { get; set; } = "";
            //public string Password { get; set; } = "hallo12345678";
            public string Database { get; set; } = "herzkompass";
        }

        public void InitConnection()
        {
            Datenbank sql = new Datenbank();
            string SQLConnection = $"SERVER={sql.Host}; DATABASE={sql.Database}; UID={sql.Username}; Password={sql.Password}";
            Connection = new MySqlConnection(SQLConnection);
            try
            {
                Connection.Open();
                Datenbankverbindung = true;
                MessageBox.Show("MySQL Verbindung aufgebaut"); // nur für den Entwickler um zu sehen ob die Verbindung klappt
            }
            catch (Exception e)
            {
                Datenbankverbindung = false; 
                MessageBox.Show(e.ToString()); // nur für den Entwickler um zu sehen ob die Verbindung klappt
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(0);
            }
        }
    }
}
