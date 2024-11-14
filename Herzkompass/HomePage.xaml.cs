using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Herzkompass
{
    public partial class HomePage : Page
    {
        private DatabaseManager dbManager;
        int loggedInUserId = UserSession.UserId;

        public HomePage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection();
            LoadUserDetails(loggedInUserId);
        }

        public void LoadUserDetails(int userId)
        {
            try
            {
                // Variablen für Benutzerdaten
                string benutzername = "";
                string profilbild = "";
                string wohnort = "";
                string ueberMich = "";
                DateTime? geburtsdatum = null;

                MySqlCommand command = dbManager.Connection.CreateCommand();
                command.CommandText = @"
                    SELECT a.benutzername, kp.profilbild, kp.geburtstag, kp.wohnort, kp.ueber_mich
                    FROM accounts a
                    LEFT JOIN kundenprofil kp ON a.id = kp.account_id
                    WHERE a.id = @userId";
                command.Parameters.AddWithValue("@userId", userId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        benutzername = reader.GetString("benutzername");
                        profilbild = reader.IsDBNull(reader.GetOrdinal("profilbild")) ? "" : reader.GetString("profilbild");
                        wohnort = reader.IsDBNull(reader.GetOrdinal("wohnort")) ? "Nicht angegeben" : reader.GetString("wohnort");
                        ueberMich = reader.IsDBNull(reader.GetOrdinal("ueber_mich")) ? "Nicht angegeben" : reader.GetString("ueber_mich");
                        geburtsdatum = reader.IsDBNull(reader.GetOrdinal("geburtstag")) ? (DateTime?)null : reader.GetDateTime("geburtstag");
                    }
                }

                // Setze den Benutzernamen und die Profilinformationen
                txtUserProfileInfo.Text = $"Benutzername: {benutzername}\n";

                // Berechnung des Alters, falls Geburtsdatum vorhanden
                if (geburtsdatum.HasValue)
                {
                    int alter = CalculateAge(geburtsdatum.Value);
                    txtUserProfileInfo.Text += $"Alter: {alter} Jahre\n";
                }
                else
                {
                    txtUserProfileInfo.Text += "Alter: Nicht angegeben\n";
                }

                txtUserProfileInfo.Text += $"Wohnort: {wohnort}\n";
                txtUserProfileInfo.Text += $"Über mich: {ueberMich}";

                // Profilbild setzen oder Standardbild verwenden
                if (!string.IsNullOrEmpty(profilbild) && profilbild.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    imgUserProfile.Source = new BitmapImage(new Uri(profilbild, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgUserProfile.Source = new BitmapImage(new Uri("/benutzer.png", UriKind.Relative)); // Standardbild
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
        }

        private int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogoutPage());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SwipePage());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new FavoritePage());
        }
    }
}
