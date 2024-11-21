using MySqlConnector;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Herzkompass
{
    public partial class HomePage : Page
    {
        private DatabaseManager dbManager; // Verwalter der Datenbankverbindung
        int loggedInUserId = UserSession.UserId; // ID des aktuell eingeloggten Benutzers

        public HomePage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection(); // Initialisiert die Datenbankverbindung
            LoadUserDetails(loggedInUserId); // Lädt die Benutzerdetails
        }

        // Lädt die Details des Benutzers aus der Datenbank
        public void LoadUserDetails(int userId)
        {
            try
            {
                // Variablen für die Benutzerdaten initialisieren
                string benutzername = "";
                string profilbild = "";
                string wohnort = "";
                string ueberMich = "";
                DateTime? geburtsdatum = null;

                // SQL-Abfrage zur Abholung der Benutzerdetails
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
                        // Werte aus der Datenbank lesen
                        benutzername = reader.GetString("benutzername");
                        profilbild = reader.IsDBNull(reader.GetOrdinal("profilbild")) ? "" : reader.GetString("profilbild");
                        wohnort = reader.IsDBNull(reader.GetOrdinal("wohnort")) ? "Nicht angegeben" : reader.GetString("wohnort");
                        ueberMich = reader.IsDBNull(reader.GetOrdinal("ueber_mich")) ? "Nicht angegeben" : reader.GetString("ueber_mich");
                        geburtsdatum = reader.IsDBNull(reader.GetOrdinal("geburtstag")) ? (DateTime?)null : reader.GetDateTime("geburtstag");
                    }
                }

                // Benutzerdaten im UI setzen
                txtUserProfileInfo.Text = $"Benutzername: {benutzername}\n";

                // Alter berechnen und anzeigen
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
                // Fehlermeldung bei Datenbankproblemen
                MessageBox.Show("Datenbankfehler: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                // Fehlermeldung bei unerwarteten Problemen
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        // Berechnet das Alter basierend auf dem Geburtsdatum
        private int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }

        // Navigation zu anderen Seiten durch Button-Klick
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

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LikePage());
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            // Leer, da dies die aktuelle Seite ist
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HelpPage());
        }
    }
}
