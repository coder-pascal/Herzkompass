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

        private void LoadUserData()
        {
            try
            {
                // Lade die Grunddaten aus der 'accounts' Tabelle
                string queryAccounts = "SELECT benutzername, email FROM accounts WHERE id = @loggedInUserId";
                using (MySqlCommand cmdAccounts = new MySqlCommand(queryAccounts, dbManager.Connection))
                {
                    cmdAccounts.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                    using (MySqlDataReader reader = cmdAccounts.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtUsername.Text = reader["benutzername"].ToString();
                            txtEmail.Text = reader["email"].ToString();
                        }
                    }
                }

                // Lade die Profildaten aus der 'kundenprofil' Tabelle
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

                            txtLocation.Text = reader["wohnort"].ToString();
                            txtAboutMe.Text = reader["ueber_mich"].ToString();
                            txtProfileImageLink.Text = reader["profilbild"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Benutzerdaten: " + ex.Message);
            }
        }

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

            // Password validation
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwörter stimmen nicht überein!");
                return;
            }

            // Validation for birthdate
            if (!ValidateBirthdate(birthdate))
            {
                MessageBox.Show("Bitte geben Sie das Geburtsdatum im Format TT.MM.JJJJ ein.");
                return;
            }

            string hashedPassword = string.IsNullOrEmpty(password) ? null : BCrypt.HashPassword(password, BCrypt.GenerateSalt());

            try
            {
                using (MySqlTransaction transaction = dbManager.Connection.BeginTransaction())
                {
                    // Update 'accounts' table
                    string queryAccounts = "UPDATE accounts SET benutzername = @username, email = @email" +
                                           (hashedPassword != null ? ", passwort = @password" : "") +
                                           " WHERE id = @loggedInUserId";
                    using (MySqlCommand cmdAccounts = new MySqlCommand(queryAccounts, dbManager.Connection, transaction))
                    {
                        cmdAccounts.Parameters.AddWithValue("@username", username);
                        cmdAccounts.Parameters.AddWithValue("@email", email);
                        if (hashedPassword != null)
                        {
                            cmdAccounts.Parameters.AddWithValue("@password", hashedPassword);
                        }
                        cmdAccounts.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                        cmdAccounts.ExecuteNonQuery();
                    }

                    // Check if profile exists in 'kundenprofil'
                    string checkProfileQuery = "SELECT COUNT(*) FROM kundenprofil WHERE account_id = @loggedInUserId";
                    using (MySqlCommand cmdCheckProfile = new MySqlCommand(checkProfileQuery, dbManager.Connection, transaction))
                    {
                        cmdCheckProfile.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                        int profileExists = Convert.ToInt32(cmdCheckProfile.ExecuteScalar());

                        string queryProfile;
                        if (profileExists == 0)
                        {
                            // Insert new profile if not exists
                            queryProfile = @"
                            INSERT INTO kundenprofil (account_id, geburtstag, wohnort, ueber_mich, profilbild) 
                            VALUES (@accountId, @geburtstag, @wohnort, @ueber_mich, @profilbild)";
                        }
                        else
                        {
                            // Update existing profile
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
                            cmdProfile.Parameters.AddWithValue("@wohnort", location);
                            cmdProfile.Parameters.AddWithValue("@ueber_mich", aboutMe);
                            cmdProfile.Parameters.AddWithValue("@profilbild", profileImageLink);
                            cmdProfile.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    MessageBox.Show("Einstellungen erfolgreich gespeichert!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern der Einstellungen: " + ex.Message);
            }
        }

        // Validate the birthdate format (TT.MM.JJJJ)
        private bool ValidateBirthdate(string birthdate)
        {
            DateTime result;
            bool isValid = DateTime.TryParseExact(birthdate, "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out result);

            return isValid;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string deletePassword = txtDeletePassword.Password;

            // Ensure the password field is not empty
            if (string.IsNullOrEmpty(deletePassword))
            {
                MessageBox.Show("Bitte geben Sie Ihr Passwort ein, um den Account zu löschen.");
                return;
            }

            try
            {
                // Retrieve the stored hashed password from the database
                string queryGetPassword = "SELECT passwort FROM accounts WHERE id = @loggedInUserId";
                string storedHashedPassword = string.Empty;

                using (MySqlCommand cmd = new MySqlCommand(queryGetPassword, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            storedHashedPassword = reader["passwort"].ToString();
                        }
                    }
                }

                // Verify if the entered password matches the stored hashed password using BCrypt.CheckPassword
                if (!BCrypt.CheckPassword(deletePassword, storedHashedPassword))
                {
                    MessageBox.Show("Das eingegebene Passwort ist falsch.");
                    return;
                }

                // Begin the deletion process: Delete the profile first, then the account
                using (MySqlTransaction transaction = dbManager.Connection.BeginTransaction())
                {
                    try
                    {
                        // Delete profile data
                        string deleteProfileQuery = "DELETE FROM kundenprofil WHERE account_id = @loggedInUserId";
                        using (MySqlCommand cmdProfile = new MySqlCommand(deleteProfileQuery, dbManager.Connection, transaction))
                        {
                            cmdProfile.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                            cmdProfile.ExecuteNonQuery();
                        }

                        // Delete account
                        string deleteAccountQuery = "DELETE FROM accounts WHERE id = @loggedInUserId";
                        using (MySqlCommand cmdAccount = new MySqlCommand(deleteAccountQuery, dbManager.Connection, transaction))
                        {
                            cmdAccount.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                            cmdAccount.ExecuteNonQuery();
                        }

                        // Commit the transaction
                        transaction.Commit();
                        MessageBox.Show("Ihr Account wurde erfolgreich gelöscht.");
                        this.NavigationService.Navigate(new LogoutPage()); // Redirect to logout page after deletion
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of error
                        transaction.Rollback();
                        MessageBox.Show("Fehler beim Löschen des Accounts: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Überprüfen des Passworts: " + ex.Message);
            }
        }

        // Navigation methods
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogoutPage());
        }
    }
}
