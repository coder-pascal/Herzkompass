using MySqlConnector;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;

namespace Herzkompass
{
    public partial class FavoritePage : Page
    {
        private DatabaseManager dbManager; // Verwalter der Datenbankverbindung
        private int loggedInUserId = UserSession.UserId; // ID des aktuell eingeloggten Benutzers
        public ObservableCollection<FavoriteProfile> FavoriteProfiles { get; set; } // Sammlung der Favoritenprofile

        public FavoritePage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection(); // Initialisiert die Datenbankverbindung

            FavoriteProfiles = new ObservableCollection<FavoriteProfile>();
            DataContext = this; // Setzt den Datenkontext auf die Seite selbst
            LoadFavoriteProfiles(); // Lädt die Favoritenprofile aus der Datenbank
        }

        private void LoadFavoriteProfiles()
        {
            try
            {
                // SQL-Abfrage zum Laden der Favoritenprofile
                string query = @"
                    SELECT a.id, a.benutzername, kp.profilbild, kp.geburtstag, kp.wohnort
                    FROM profile_favorites pf
                    JOIN accounts a ON pf.favorite_profile_id = a.id
                    LEFT JOIN kundenprofil kp ON a.id = kp.account_id
                    WHERE pf.favoriter_id = @loggedInUserId";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    // Parameter für die SQL-Abfrage
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        FavoriteProfiles.Clear(); // Löscht vorhandene Einträge
                        while (reader.Read())
                        {
                            int profileId = reader.GetInt32("id"); // ID des Profils
                            string username = reader["benutzername"].ToString(); // Benutzername des Profils
                            string profileImagePath = reader.IsDBNull(reader.GetOrdinal("profilbild"))
                                ? null : reader["profilbild"].ToString(); // Pfad zum Profilbild
                            string location = reader.IsDBNull(reader.GetOrdinal("wohnort"))
                                ? "Nicht angegeben" : reader["wohnort"].ToString(); // Wohnort
                            DateTime? birthday = reader.IsDBNull(reader.GetOrdinal("geburtstag"))
                                ? (DateTime?)null : reader.GetDateTime("geburtstag"); // Geburtstag

                            int? age = birthday.HasValue ? CalculateAge(birthday.Value) : (int?)null; // Berechnet das Alter

                            // Fügt das Profil zur Liste hinzu
                            FavoriteProfiles.Add(new FavoriteProfile
                            {
                                ProfileId = profileId,
                                Username = username,
                                ProfileImagePath = string.IsNullOrEmpty(profileImagePath)
                                    ? "/Images/benutzer.png" // Standardbild, wenn kein Bild vorhanden ist
                                    : profileImagePath,
                                Age = age.HasValue ? $"{age.Value} Jahre" : "Alter: Nicht angegeben",
                                Location = location
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Fehlermeldung bei Datenbankproblemen
                MessageBox.Show("Fehler beim Laden der Favoriten: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                // Fehlermeldung bei unerwarteten Problemen
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        // Berechnet das Alter anhand des Geburtsdatums
        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                age--;
            return age;
        }

        // Entfernt ein Favoritenprofil aus der Datenbank und Liste
        private void RemoveFavorite(int profileId)
        {
            try
            {
                string query = "DELETE FROM profile_favorites WHERE favoriter_id = @loggedInUserId AND favorite_profile_id = @profileId";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId); // Benutzer-ID
                    cmd.Parameters.AddWithValue("@profileId", profileId); // Profil-ID
                    cmd.ExecuteNonQuery(); // Führt die Löschanweisung aus

                    // Entfernt das Profil aus der ObservableCollection
                    var profileToRemove = FavoriteProfiles.FirstOrDefault(p => p.ProfileId == profileId);
                    if (profileToRemove != null)
                    {
                        FavoriteProfiles.Remove(profileToRemove);
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Fehlermeldung bei Datenbankproblemen
                MessageBox.Show("Fehler beim Entfernen des Favoriten: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                // Fehlermeldung bei unerwarteten Problemen
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        // Event-Handler für das Entfernen eines Favoriten durch Button-Klick
        private void OnRemoveFavoriteClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int profileId)
            {
                RemoveFavorite(profileId);
            }
        }

        // Navigation zu anderen Seiten durch Button-Klick
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SwipePage());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SwipePage());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LikePage());
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            // Leer, da dies die aktuelle Seite ist
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HelpPage());
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogoutPage());
        }
    }

    // Modellklasse für ein Favoritenprofil
    public class FavoriteProfile
    {
        public int ProfileId { get; set; } // ID des Profils
        public string Username { get; set; } // Benutzername
        public string ProfileImagePath { get; set; } // Pfad zum Profilbild
        public string Age { get; set; } // Alter als String
        public string Location { get; set; } // Wohnort
    }
}
