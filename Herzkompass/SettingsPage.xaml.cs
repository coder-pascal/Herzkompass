using Bcrypt;
using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Herzkompass
{
    public partial class SettingsPage : Page
    {
        private DatabaseManager dbManager;
        int loggedInUserId = UserSession.UserId;

        public SettingsPage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection();
            LoadUserData();
        }

        // Lädt die Benutzerdaten aus der Datenbank
        private void LoadUserData()
        {
            try
            {
                // SQL-Query, um Benutzername und Email zu holen
                string queryAccounts = "SELECT benutzername, email FROM accounts WHERE id = @loggedInUserId";
                using (MySqlCommand cmdAccounts = new MySqlCommand(queryAccounts, dbManager.Connection))
                {
                    cmdAccounts.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                    using (MySqlDataReader reader = cmdAccounts.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtUsername.Text = reader["benutzername"].ToString(); // Benutzername setzen
                            txtEmail.Text = reader["email"].ToString(); // Email setzen
                        }
                    }
                }

                // SQL-Query, um Profilinformationen zu holen
                string queryProfile = "SELECT geburtstag, wohnort, ueber_mich, profilbild FROM kundenprofil WHERE account_id = @loggedInUserId";
                using (MySqlCommand cmdProfile = new MySqlCommand(queryProfile, dbManager.Connection))
                {
                    cmdProfile.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                    using (MySqlDataReader reader = cmdProfile.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Geburtsdatum im deutschen Format anzeigen
                            DateTime geburtstag = Convert.ToDateTime(reader["geburtstag"]);
                            txtAge.Text = geburtstag.ToString("dd.MM.yyyy"); // Umwandlung in deutsches Format

                            txtLocation.Text = reader["wohnort"].ToString(); // Wohnort setzen
                            txtAboutMe.Text = reader["ueber_mich"].ToString(); // Über mich setzen
                            txtProfileImageLink.Text = reader["profilbild"].ToString(); // Profilbild-Link setzen
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Benutzerdaten: " + ex.Message); // Fehlerbehandlung
            }
        }

        // Speichert die Benutzereinstellungen
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;
            string location = txtLocation.Text;
            string aboutMe = txtAboutMe.Text;
            string profileImageLink = txtProfileImageLink.Text;
            string birthdate = txtAge.Text;

            // Überprüfung, ob die Passwörter übereinstimmen
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwörter stimmen nicht überein!"); // Fehlermeldung, wenn Passwörter nicht übereinstimmen
                return;
            }

            // Überprüfung des Geburtsdatums
            if (!ValidateBirthdate(birthdate))
            {
                MessageBox.Show("Bitte geben Sie das Geburtsdatum im Format TT.MM.JJJJ ein."); // Fehlermeldung bei ungültigem Geburtsdatum
                return;
            }

            string hashedPassword = string.IsNullOrEmpty(password) ? null : BCrypt.HashPassword(password, BCrypt.GenerateSalt());

            try
            {
                using (MySqlTransaction transaction = dbManager.Connection.BeginTransaction()) // Transaktion starten
                {
                    // SQL-Query für das Update von Benutzername und Email
                    string queryAccounts = "UPDATE accounts SET benutzername = @username, email = @email" +
                                           (hashedPassword != null ? ", passwort = @password" : "") +
                                           " WHERE id = @loggedInUserId";
                    using (MySqlCommand cmdAccounts = new MySqlCommand(queryAccounts, dbManager.Connection, transaction))
                    {
                        cmdAccounts.Parameters.AddWithValue("@username", username);
                        cmdAccounts.Parameters.AddWithValue("@email", email);
                        if (hashedPassword != null)
                        {
                            cmdAccounts.Parameters.AddWithValue("@password", hashedPassword); // Passwort hinzufügen, wenn gesetzt
                        }
                        cmdAccounts.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                        cmdAccounts.ExecuteNonQuery(); // Query ausführen
                    }

                    // Überprüfung, ob bereits ein Profil existiert
                    string checkProfileQuery = "SELECT COUNT(*) FROM kundenprofil WHERE account_id = @loggedInUserId";
                    using (MySqlCommand cmdCheckProfile = new MySqlCommand(checkProfileQuery, dbManager.Connection, transaction))
                    {
                        cmdCheckProfile.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                        int profileExists = Convert.ToInt32(cmdCheckProfile.ExecuteScalar());

                        string queryProfile;
                        if (profileExists == 0)
                        {
                            // Wenn das Profil noch nicht existiert, wird es eingefügt
                            queryProfile = @"
                            INSERT INTO kundenprofil (account_id, geburtstag, wohnort, ueber_mich, profilbild) 
                            VALUES (@accountId, @geburtstag, @wohnort, @ueber_mich, @profilbild)";
                        }
                        else
                        {
                            // Profil aktualisieren, wenn es schon existiert
                            queryProfile = @"
                            UPDATE kundenprofil 
                            SET geburtstag = @geburtstag, wohnort = @wohnort, ueber_mich = @ueber_mich, profilbild = @profilbild 
                            WHERE account_id = @accountId";
                        }

                        using (MySqlCommand cmdProfile = new MySqlCommand(queryProfile, dbManager.Connection, transaction))
                        {
                            cmdProfile.Parameters.AddWithValue("@accountId", loggedInUserId); // Ensure account_id is set
                            DateTime birthdateParsed = DateTime.ParseExact(birthdate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            string mysqlFormattedDate = birthdateParsed.ToString("yyyy-MM-dd");

                            cmdProfile.Parameters.AddWithValue("@geburtstag", mysqlFormattedDate); // Das umformatierte Datum
                            cmdProfile.Parameters.AddWithValue("@wohnort", location); // Wohnort
                            cmdProfile.Parameters.AddWithValue("@ueber_mich", aboutMe); // Über mich
                            cmdProfile.Parameters.AddWithValue("@profilbild", profileImageLink); // Profilbild-Link
                            cmdProfile.ExecuteNonQuery(); // Query ausführen
                        }
                    }

                    transaction.Commit(); // Transaktion abschließen
                    MessageBox.Show("Einstellungen erfolgreich gespeichert!"); // Erfolgsnachricht
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern der Einstellungen: " + ex.Message); // Fehlerbehandlung
            }
        }

        // Validiert das Geburtsdatum im Format TT.MM.JJJJ
        private bool ValidateBirthdate(string birthdate)
        {
            DateTime result;
            bool isValid = DateTime.TryParseExact(birthdate, "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out result);

            return isValid; // Gibt zurück, ob das Datum gültig ist
        }

        // Löscht den Account des Benutzers
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string deletePassword = txtDeletePassword.Password;

            // Sicherstellen, dass das Passwortfeld nicht leer ist
            if (string.IsNullOrEmpty(deletePassword))
            {
                MessageBox.Show("Bitte geben Sie Ihr Passwort ein, um den Account zu löschen."); // Fehlermeldung bei leerem Passwortfeld
                return;
            }

            try
            {
                // Ruft das gespeicherte Passwort aus der Datenbank ab
                string queryGetPassword = "SELECT passwort FROM accounts WHERE id = @loggedInUserId";
                string storedHashedPassword = string.Empty;

                using (MySqlCommand cmd = new MySqlCommand(queryGetPassword, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            storedHashedPassword = reader["passwort"].ToString(); // Passwort setzen
                        }
                    }
                }

                // Überprüft, ob das eingegebene Passwort mit dem gespeicherten Passwort übereinstimmt
                if (!BCrypt.CheckPassword(deletePassword, storedHashedPassword))
                {
                    MessageBox.Show("Das eingegebene Passwort ist falsch."); // Fehlermeldung bei falschem Passwort
                    return;
                }

                // Beginnt den Löschvorgang: Zuerst das Profil, dann das Konto löschen
                using (MySqlTransaction transaction = dbManager.Connection.BeginTransaction())
                {
                    try
                    {
                        // Löscht die Profildaten
                        string deleteProfileQuery = "DELETE FROM kundenprofil WHERE account_id = @loggedInUserId";
                        using (MySqlCommand cmdProfile = new MySqlCommand(deleteProfileQuery, dbManager.Connection, transaction))
                        {
                            cmdProfile.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                            cmdProfile.ExecuteNonQuery();
                        }

                        // Löscht das Benutzerkonto
                        string deleteAccountQuery = "DELETE FROM accounts WHERE id = @loggedInUserId";
                        using (MySqlCommand cmdAccount = new MySqlCommand(deleteAccountQuery, dbManager.Connection, transaction))
                        {
                            cmdAccount.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                            cmdAccount.ExecuteNonQuery();
                        }

                        // Bestätigt die Transaktion
                        transaction.Commit();
                        MessageBox.Show("Ihr Account wurde erfolgreich gelöscht."); // Erfolgsnachricht
                        this.NavigationService.Navigate(new LogoutPage()); // Weiterleitung zur Logout-Seite
                    }
                    catch (Exception ex)
                    {
                        // Rollback der Transaktion im Falle eines Fehlers
                        transaction.Rollback();
                        MessageBox.Show("Fehler beim Löschen des Accounts: " + ex.Message); // Fehlerbehandlung
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Überprüfen des Passworts: " + ex.Message); // Fehlerbehandlung
            }
        }

        // Navigationsmethoden
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage()); // Navigation zur Startseite
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogoutPage()); // Navigation zur Logout-Seite
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SwipePage()); // Navigation zur Swipe-Seite
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LikePage()); // Navigation zur Like-Seite
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new FavoritePage()); // Navigation zur Favoriten-Seite
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //leer da eigene Seite
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HelpPage()); // Navigation zur Hilfe-Seite
        }
    }
}
