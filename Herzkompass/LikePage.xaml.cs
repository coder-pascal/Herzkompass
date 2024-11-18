using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace Herzkompass
{
    public partial class LikePage : Page
    {
        private DatabaseManager dbManager;
        private int loggedInUserId = UserSession.UserId;
        public ObservableCollection<LikedProfile> LikedProfiles { get; set; }

        public LikePage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection();

            LikedProfiles = new ObservableCollection<LikedProfile>();
            DataContext = this; // Bind the page's data context to itself for binding
            LoadLikedProfiles();
        }

        private void LoadLikedProfiles()
        {
            try
            {
                string query = @"
                    SELECT a.id, a.benutzername, kp.profilbild, kp.geburtstag, kp.wohnort
                    FROM profile_likes pl
                    JOIN accounts a ON pl.liked_profile_id = a.id
                    LEFT JOIN kundenprofil kp ON a.id = kp.account_id
                    WHERE pl.liker_id = @loggedInUserId";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        LikedProfiles.Clear();
                        while (reader.Read())
                        {
                            int profileId = reader.GetInt32("id");
                            string username = reader["benutzername"].ToString();
                            string profileImagePath = reader.IsDBNull(reader.GetOrdinal("profilbild")) ? null : reader["profilbild"].ToString();
                            string location = reader.IsDBNull(reader.GetOrdinal("wohnort")) ? "Nicht angegeben" : reader["wohnort"].ToString();
                            DateTime? birthday = reader.IsDBNull(reader.GetOrdinal("geburtstag")) ? (DateTime?)null : reader.GetDateTime("geburtstag");

                            int? age = birthday.HasValue ? CalculateAge(birthday.Value) : (int?)null;

                            LikedProfiles.Add(new LikedProfile
                            {
                                ProfileId = profileId,
                                Username = username,
                                ProfileImagePath = string.IsNullOrEmpty(profileImagePath)
                                    ? "/Images/benutzer.png" // Use a default image if none is provided
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
                MessageBox.Show("Fehler beim Laden der Likes: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                age--;
            return age;
        }

        private void OnRemoveLikeClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int profileId)
            {
                RemoveLike(profileId);
            }
        }

        private void OnAddToFavoritesClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int profileId)
            {
                AddToFavorites(profileId);
            }
        }

        private void RemoveLike(int profileId)
        {
            try
            {
                string query = "DELETE FROM profile_likes WHERE liker_id = @loggedInUserId AND liked_profile_id = @profileId";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                    cmd.Parameters.AddWithValue("@profileId", profileId);
                    cmd.ExecuteNonQuery();

                    // Entferne das Profil aus der Liste
                    var profileToRemove = LikedProfiles.FirstOrDefault(p => p.ProfileId == profileId);
                    if (profileToRemove != null)
                    {
                        LikedProfiles.Remove(profileToRemove);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Fehler beim Entfernen des Likes: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        private void AddToFavorites(int profileId)
        {
            try
            {
                // Start a transaction to ensure data consistency
                using (var transaction = dbManager.Connection.BeginTransaction())
                {
                    // Füge das Profil zu den Favoriten hinzu
                    string insertQuery = "INSERT INTO profile_favorites (favoriter_id, favorite_profile_id) VALUES (@loggedInUserId, @profileId)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, dbManager.Connection, transaction))
                    {
                        insertCmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                        insertCmd.Parameters.AddWithValue("@profileId", profileId);
                        insertCmd.ExecuteNonQuery();
                    }

                    // Entferne das Profil aus der Likes-Tabelle
                    string deleteQuery = "DELETE FROM profile_likes WHERE liker_id = @loggedInUserId AND liked_profile_id = @profileId";
                    using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, dbManager.Connection, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                        deleteCmd.Parameters.AddWithValue("@profileId", profileId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Commit the transaction
                    transaction.Commit();

                    // Entferne das Profil aus der LikedProfiles-Liste (UI)
                    var profileToRemove = LikedProfiles.FirstOrDefault(p => p.ProfileId == profileId);
                    if (profileToRemove != null)
                    {
                        LikedProfiles.Remove(profileToRemove);
                    }

                    MessageBox.Show("Profil wurde zu den Favoriten hinzugefügt!", "Erfolg");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Fehler beim Hinzufügen zu den Favoriten: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        // Navigation
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
            //leer da eigene Seite
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

    public class LikedProfile
    {
        public int ProfileId { get; set; }
        public string Username { get; set; }
        public string ProfileImagePath { get; set; }
        public string Age { get; set; }
        public string Location { get; set; }
    }
}
