using Bcrypt;
using MySqlConnector;
using System.Windows;

namespace Herzkompass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseManager dbManager;

        // Aktuelle BenutzerID speichern
        private int currentUserId;

        public MainWindow()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection();
        }

        // Anmelde Button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEmail.Text) && !string.IsNullOrEmpty(txtPasswort.Password))
            {
                string passwort = "";
                try
                {
                    MySqlCommand command = dbManager.Connection.CreateCommand();

                    // SQL-Abfrage, um sowohl die `id` als auch das `passwort` abzurufen
                    command.CommandText = "SELECT id, passwort FROM accounts WHERE email = @email LIMIT 1";
                    command.Parameters.AddWithValue("@email", txtEmail.Text);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // Überprüfen, ob die E-Mail existiert
                        {
                            reader.Read();
                            passwort = reader.GetString("passwort");  // Passwort abrufen
                            currentUserId = reader.GetInt32("id");    // Benutzer-ID abrufen
                        }
                        else
                        {
                            // Falls die E-Mail nicht existiert
                            MessageBox.Show("Diese E-Mail-Adresse existiert nicht.", "Fehler");
                            return;
                        }
                    }

                    // Wenn die E-Mail existiert, Passwort überprüfen
                    if (BCrypt.CheckPassword(txtPasswort.Password, passwort))
                    {
                        UserSession.UserId = currentUserId; // Benutzer-ID in Session speichern
                        MessageBox.Show("Login erfolgreich. \nSie werden automatisch auf die Startseite umgeleitet." + currentUserId, "Erfolg");
                        MainFrame.Navigate(new HomePage());
                    }
                    else
                    {
                        MessageBox.Show("Falsches Passwort.", "Fehler");
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Datenbankfehler: " + ex.Message, "Fehler");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
                }
                finally
                {
                    if (dbManager.Connection.State == System.Data.ConnectionState.Open)
                    {
                        dbManager.Connection.Close(); // Verbindung zur Datenbank schließen
                    }
                }
            }
            else
            {
                MessageBox.Show("Bitte füllen Sie zuerst alle Felder aus.", "Fehler");
            }
        }

        // Registrier Button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Navigiere zur RegisterPage im Frame
            MainFrame.Navigate(new RegisterPage());
        }
    }
}