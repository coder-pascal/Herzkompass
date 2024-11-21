using Bcrypt;
using MySqlConnector;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Herzkompass
{
    /// <summary>
    /// Interaktionslogik für RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private DatabaseManager dbManager; // Instanz von DatabaseManager zur Verwaltung der DB-Verbindung

        public RegisterPage()
        {
            InitializeComponent(); // Initialisiert die UI-Komponenten
            dbManager = new DatabaseManager(); // Erstellt eine neue Instanz des DatabaseManagers
            dbManager.InitConnection(); // Stellt die Verbindung zur Datenbank her
        }

        // Event-Handler für den Registrieren-Button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Überprüft, ob alle erforderlichen Felder (E-Mail, Benutzername und Passwörter) ausgefüllt sind
            if (!string.IsNullOrEmpty(txtEmail.Text) && !string.IsNullOrEmpty(txtBenutzername.Text) && !string.IsNullOrEmpty(txtPasswort.Password) && !string.IsNullOrEmpty(txtPasswort2.Password))
            {
                // Überprüft, ob die Passwörter übereinstimmen und nicht leer sind
                if (txtPasswort.Password == txtPasswort2.Password && txtPasswort2.Password.Length > 0)
                {
                    try
                    {
                        // Überprüft, ob der Benutzername oder die E-Mail bereits in der Datenbank existieren
                        MySqlCommand checkCommand = dbManager.Connection.CreateCommand();
                        checkCommand.CommandText = "SELECT COUNT(*) FROM accounts WHERE benutzername = @benutzername OR email = @email";
                        checkCommand.Parameters.AddWithValue("@benutzername", txtBenutzername.Text); // Benutzername als Parameter
                        checkCommand.Parameters.AddWithValue("@email", txtEmail.Text); // E-Mail als Parameter

                        int count = Convert.ToInt32(checkCommand.ExecuteScalar()); // Führt die Abfrage aus und gibt die Anzahl der gefundenen Datensätze zurück

                        if (count > 0) // Wenn der Benutzername oder die E-Mail bereits existiert
                        {
                            MessageBox.Show("Benutzername oder E-Mail-Adresse ist bereits registriert.", "Fehler");
                            return;
                        }

                        // Hashen des Passworts mit BCrypt
                        string hashedPassword = BCrypt.HashPassword(txtPasswort.Password, BCrypt.GenerateSalt()); // Passwort verschlüsseln

                        // SQL-Befehl zum Einfügen der neuen Benutzerdaten
                        MySqlCommand command = dbManager.Connection.CreateCommand();
                        command.CommandText = "INSERT INTO accounts (benutzername, email, passwort) VALUES (@benutzername, @email, @passwort)";
                        command.Parameters.AddWithValue("@benutzername", txtBenutzername.Text); // Benutzername
                        command.Parameters.AddWithValue("@email", txtEmail.Text); // E-Mail-Adresse
                        command.Parameters.AddWithValue("@passwort", hashedPassword); // Verschlüsseltes Passwort

                        command.ExecuteNonQuery(); // Führt den SQL-Befehl aus und fügt den Datensatz hinzu

                        MessageBox.Show("Willkommen bei Herzkompass!", "Registrierung erfolgreich"); // Erfolgsnachricht anzeigen
                    }
                    catch (Exception ex)
                    {
                        // Fehlermeldung, wenn ein Fehler bei der Registrierung auftritt
                        MessageBox.Show("Fehler bei der Registrierung: " + ex.Message, "Fehler");
                    }
                    finally
                    {
                        // Sicherstellen, dass die Datenbankverbindung geschlossen wird
                        if (dbManager.Connection.State == System.Data.ConnectionState.Open)
                        {
                            dbManager.Connection.Close(); // Schließt die Verbindung zur Datenbank
                        }
                    }
                }
                else
                {
                    // Fehlermeldung, wenn die Passwörter nicht übereinstimmen oder leer sind
                    MessageBox.Show("Die Passwörter stimmen nicht überein oder sind leer.", "Fehler");
                }
            }
            else
            {
                // Fehlermeldung, wenn nicht alle Felder ausgefüllt sind
                MessageBox.Show("Bitte füllen Sie zuerst alle Felder aus.", "Fehler");
            }
        }

        // Event-Handler für den Zurück-Button (zurück zur Anmeldeseite)
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Navigiert zurück zur Anmeldeseite, indem der Frame-Inhalt zurückgesetzt wird
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                // Leert den Frame, um zur ursprünglichen Ansicht (MainWindow) zurückzukehren
                mainWindow.MainFrame.Content = null;
            }
        }
    }
}
