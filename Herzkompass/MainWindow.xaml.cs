using Bcrypt;
using MySqlConnector;
using System.Windows;

namespace Herzkompass
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseManager dbManager; // Instanz von DatabaseManager zur Verwaltung der DB-Verbindung

        // Aktuelle BenutzerID speichern
        private int currentUserId;

        public MainWindow()
        {
            InitializeComponent(); // Initialisiert die UI-Komponenten
            dbManager = new DatabaseManager(); // Erstellt eine neue Instanz des DatabaseManagers
            dbManager.InitConnection(); // Stellt die Verbindung zur Datenbank her
        }

        // Event-Handler für den Anmelde-Button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Überprüft, ob beide Eingabefelder (E-Mail und Passwort) ausgefüllt sind
            if (!string.IsNullOrEmpty(txtEmail.Text) && !string.IsNullOrEmpty(txtPasswort.Password))
            {
                string passwort = ""; // Variable zum Speichern des abgerufenen Passworts
                try
                {
                    MySqlCommand command = dbManager.Connection.CreateCommand(); // Erstellt den SQL-Befehl

                    // SQL-Abfrage, um sowohl die `id` als auch das `passwort` der Benutzer anhand der E-Mail zu erhalten
                    command.CommandText = "SELECT id, passwort FROM accounts WHERE email = @email LIMIT 1";
                    command.Parameters.AddWithValue("@email", txtEmail.Text); // Parameter für die E-Mail setzen

                    // Führt den Befehl aus und liest das Ergebnis
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // Überprüft, ob die E-Mail existiert
                        {
                            reader.Read(); // Liest die erste Zeile
                            passwort = reader.GetString("passwort");  // Speichert das Passwort aus der Datenbank
                            currentUserId = reader.GetInt32("id");    // Speichert die Benutzer-ID
                        }
                        else
                        {
                            // Zeigt eine Fehlermeldung an, wenn die E-Mail nicht existiert
                            MessageBox.Show("Diese E-Mail-Adresse existiert nicht.", "Fehler");
                            return;
                        }
                    }

                    // Überprüft, ob das eingegebene Passwort mit dem in der Datenbank übereinstimmt
                    if (BCrypt.CheckPassword(txtPasswort.Password, passwort))
                    {
                        // Speichert die Benutzer-ID in der Session, wenn das Passwort korrekt ist
                        UserSession.UserId = currentUserId;
                        MessageBox.Show("Login erfolgreich. \nSie werden automatisch auf die Startseite umgeleitet." + currentUserId, "Erfolg");
                        MainFrame.Navigate(new HomePage()); // Navigiert zur Startseite
                    }
                    else
                    {
                        // Zeigt eine Fehlermeldung an, wenn das Passwort falsch ist
                        MessageBox.Show("Falsches Passwort.", "Fehler");
                    }
                }
                catch (MySqlException ex)
                {
                    // Zeigt eine Fehlermeldung an, wenn ein Datenbankfehler auftritt
                    MessageBox.Show("Datenbankfehler: " + ex.Message, "Fehler");
                }
                catch (Exception ex)
                {
                    // Zeigt eine allgemeine Fehlermeldung an, wenn ein unerwarteter Fehler auftritt
                    MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
                }
                finally
                {
                    // Stellt sicher, dass die Datenbankverbindung immer geschlossen wird
                    if (dbManager.Connection.State == System.Data.ConnectionState.Open)
                    {
                        dbManager.Connection.Close(); // Schließt die Verbindung zur Datenbank
                    }
                }
            }
            else
            {
                // Zeigt eine Fehlermeldung an, wenn eines der Felder leer ist
                MessageBox.Show("Bitte füllen Sie zuerst alle Felder aus.", "Fehler");
            }
        }

        // Event-Handler für den Registrier-Button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Navigiert zur Registrierseite im Frame
            MainFrame.Navigate(new RegisterPage());
        }
    }
}
