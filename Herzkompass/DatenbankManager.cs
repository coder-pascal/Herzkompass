using MySqlConnector;
using System;
using System.Windows;

namespace Herzkompass
{
    public class DatabaseManager
    {
        // Gibt an, ob eine Verbindung zur Datenbank besteht (true = verbunden, false = nicht verbunden)
        public bool Datenbankverbindung { get; private set; } = false;

        // Speichert die MySQL-Verbindungsinstanz
        public MySqlConnection Connection { get; private set; }

        // Interne Klasse zur Verwaltung der Datenbankkonfigurationsdaten
        private class Datenbank
        {
            public string Host { get; set; } = "localhost"; // Hostname des Datenbankservers
            public string Username { get; set; } = "root"; // Benutzername für die Datenbank
            public string Password { get; set; } = ""; // Passwort für die Datenbank
            public string Database { get; set; } = "herzkompass"; // Name der zu verwendenden Datenbank
        }

        // Initialisiert die Verbindung zur MySQL-Datenbank
        public void InitConnection()
        {
            // Erstellt eine Instanz der Datenbankkonfiguration
            Datenbank sql = new Datenbank();

            // Baut die Verbindungszeichenkette basierend auf den Konfigurationsdaten
            string SQLConnection = $"SERVER={sql.Host}; DATABASE={sql.Database}; UID={sql.Username}; Password={sql.Password}";

            // Erstellt eine neue MySQL-Verbindung mit der Verbindungszeichenkette
            Connection = new MySqlConnection(SQLConnection);

            try
            {
                // Versucht, die Verbindung zu öffnen
                Connection.Open();
                Datenbankverbindung = true; // Setzt den Verbindungsstatus auf "verbunden"
                // MessageBox.Show("MySQL Verbindung aufgebaut"); // Debugging: Gibt Erfolgsmeldung aus (auskommentiert)
            }
            catch (Exception e)
            {
                Datenbankverbindung = false; // Setzt den Verbindungsstatus auf "nicht verbunden"
                MessageBox.Show(e.ToString()); // Zeigt den Fehler an (nur für Debugging)
                System.Threading.Thread.Sleep(5000); // Wartet 5 Sekunden
                Environment.Exit(0); // Beendet das Programm bei einem Verbindungsfehler
            }
        }
    }
}
