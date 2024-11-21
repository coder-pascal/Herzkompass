// Namespace für das Projekt Herzkompass, der die LikePage enthält
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;

namespace Herzkompass
{
    // LikePage Klasse, die eine WPF-Seite darstellt, auf der die vom Benutzer "gemocht" Profile angezeigt werden
    public partial class LikePage : Page
    {
        // Instanz des DatabaseManagers für den Zugriff auf die Datenbank
        private DatabaseManager dbManager;
        // Die Benutzer-ID des angemeldeten Benutzers wird aus der Benutzersitzung abgerufen
        private int loggedInUserId = UserSession.UserId;
        // ObservableCollection für die Anzeige der gemocht Profile
        public ObservableCollection<LikedProfile> LikedProfiles { get; set; }

        // Konstruktor der LikePage
        public LikePage()
        {
            InitializeComponent();
            // Initialisiert den DatabaseManager und stellt eine Verbindung zur Datenbank her
            dbManager = new DatabaseManager();
            dbManager.InitConnection();

            // Initialisiert die ObservableCollection, die die gemocht Profile enthält
            LikedProfiles = new ObservableCollection<LikedProfile>();
            // Setzt den DataContext der Seite auf die Seite selbst, um Datenbindung zu ermöglichen
            DataContext = this;
            // Lädt die gemocht Profile aus der Datenbank
            LoadLikedProfiles();
        }

        // Methode zum Laden der gemocht Profile aus der Datenbank
        private void LoadLikedProfiles()
        {
            try
            {
                // SQL-Abfrage zum Abrufen der gemocht Profile des angemeldeten Benutzers
                string query = @"
                    SELECT a.id, a.benutzername, kp.profilbild, kp.geburtstag, kp.wohnort
                    FROM profile_likes pl
                    JOIN accounts a ON pl.liked_profile_id = a.id
                    LEFT JOIN kundenprofil kp ON a.id = kp.account_id
                    WHERE pl.liker_id = @loggedInUserId";

                // SQL-Befehl ausführen, um die Daten abzurufen
                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    // Parameter für die Abfrage hinzufügen
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);

                    // Ausführen der Abfrage und Abrufen der Ergebnisse
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Leeren der Liste der gemocht Profile
                        LikedProfiles.Clear();
                        // Durchlaufen der Abfrageergebnisse
                        while (reader.Read())
                        {
                            // Abrufen der Profileigenschaften aus der Datenbank
                            int profileId = reader.GetInt32("id");
                            string username = reader["benutzername"].ToString();
                            string profileImagePath = reader.IsDBNull(reader.GetOrdinal("profilbild")) ? null : reader["profilbild"].ToString();
                            string location = reader.IsDBNull(reader.GetOrdinal("wohnort")) ? "Nicht angegeben" : reader["wohnort"].ToString();
                            DateTime? birthday = reader.IsDBNull(reader.GetOrdinal("geburtstag")) ? (DateTime?)null : reader.GetDateTime("geburtstag");

                            // Berechnung des Alters, falls das Geburtsdatum vorhanden ist
                            int? age = birthday.HasValue ? CalculateAge(birthday.Value) : (int?)null;

                            // Hinzufügen des geladenen Profils zur Liste der gemocht Profile
                            LikedProfiles.Add(new LikedProfile
                            {
                                ProfileId = profileId,
                                Username = username,
                                ProfileImagePath = string.IsNullOrEmpty(profileImagePath)
                                    ? "/Images/benutzer.png" // Standardbild verwenden, wenn keines vorhanden ist
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
                // Fehlerbehandlung bei MySQL-Ausnahmen
                MessageBox.Show("Fehler beim Laden der Likes: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei allgemeinen Ausnahmen
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        // Methode zur Berechnung des Alters basierend auf dem Geburtsdatum
        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Now.Year - birthDate.Year;
            // Wenn der Geburtstag in diesem Jahr noch nicht stattgefunden hat, wird das Alter um 1 Jahr verringert
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                age--;
            return age;
        }

        // Ereignisbehandlungsmethode für das Entfernen eines Likes
        private void OnRemoveLikeClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int profileId)
            {
                // Entfernen des Likes für das angeklickte Profil
                RemoveLike(profileId);
            }
        }

        // Ereignisbehandlungsmethode für das Hinzufügen eines Profils zu den Favoriten
        private void OnAddToFavoritesClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int profileId)
            {
                // Hinzufügen des Profils zu den Favoriten
                AddToFavorites(profileId);
            }
        }

        // Methode zum Entfernen eines Likes aus der Datenbank und der UI-Liste
        private void RemoveLike(int profileId)
        {
            try
            {
                // SQL-Abfrage zum Entfernen des Likes aus der Datenbank
                string query = "DELETE FROM profile_likes WHERE liker_id = @loggedInUserId AND liked_profile_id = @profileId";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    // Parameter für die Abfrage hinzufügen
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                    cmd.Parameters.AddWithValue("@profileId", profileId);
                    // Ausführen der Abfrage, um das Like zu entfernen
                    cmd.ExecuteNonQuery();

                    // Entfernen des Profils aus der UI-Liste der gemocht Profile
                    var profileToRemove = LikedProfiles.FirstOrDefault(p => p.ProfileId == profileId);
                    if (profileToRemove != null)
                    {
                        LikedProfiles.Remove(profileToRemove);
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Fehlerbehandlung bei MySQL-Ausnahmen
                MessageBox.Show("Fehler beim Entfernen des Likes: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei allgemeinen Ausnahmen
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        // Methode zum Hinzufügen eines Profils zu den Favoriten und Entfernen aus den Likes
        private void AddToFavorites(int profileId)
        {
            try
            {
                // Transaktionsblock für konsistente Datenoperationen
                using (var transaction = dbManager.Connection.BeginTransaction())
                {
                    // SQL-Abfrage zum Hinzufügen des Profils zu den Favoriten
                    string insertQuery = "INSERT INTO profile_favorites (favoriter_id, favorite_profile_id) VALUES (@loggedInUserId, @profileId)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, dbManager.Connection, transaction))
                    {
                        // Parameter für die Abfrage hinzufügen
                        insertCmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                        insertCmd.Parameters.AddWithValue("@profileId", profileId);
                        // Ausführen der Abfrage, um das Profil zu den Favoriten hinzuzufügen
                        insertCmd.ExecuteNonQuery();
                    }

                    // SQL-Abfrage zum Entfernen des Profils aus der Likes-Tabelle
                    string deleteQuery = "DELETE FROM profile_likes WHERE liker_id = @loggedInUserId AND liked_profile_id = @profileId";
                    using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, dbManager.Connection, transaction))
                    {
                        // Parameter für die Abfrage hinzufügen
                        deleteCmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                        deleteCmd.Parameters.AddWithValue("@profileId", profileId);
                        // Ausführen der Abfrage, um das Profil aus den Likes zu entfernen
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Bestätigung der Transaktion
                    transaction.Commit();

                    // Entfernen des Profils aus der UI-Liste der gemocht Profile
                    var profileToRemove = LikedProfiles.FirstOrDefault(p => p.ProfileId == profileId);
                    if (profileToRemove != null)
                    {
                        LikedProfiles.Remove(profileToRemove);
                    }

                    // Benutzerbenachrichtigung über den Erfolg
                    MessageBox.Show("Profil wurde zu den Favoriten hinzugefügt!", "Erfolg");
                }
            }
            catch (MySqlException ex)
            {
                // Fehlerbehandlung bei MySQL-Ausnahmen
                MessageBox.Show("Fehler beim Hinzufügen zu den Favoriten: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei allgemeinen Ausnahmen
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        // Navigation zu verschiedenen Seiten innerhalb der Anwendung
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SwipePage());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Keine Navigation, bleibt leer für benutzerdefinierte Seiten
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new FavoritePage());
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HelpPage());
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }
    }

    // Klasse, die ein gemocht Profil darstellt
    public class LikedProfile
    {
        public int ProfileId { get; set; } // Profil-ID
        public string Username { get; set; } // Benutzername des Profils
        public string ProfileImagePath { get; set; } // Pfad zum Profilbild
        public string Age { get; set; } // Alter des Profilinhabers
        public string Location { get; set; } // Standort des Profilinhabers
    }
}
