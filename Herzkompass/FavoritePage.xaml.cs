using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MySqlConnector;

namespace Herzkompass
{
    public partial class FavoritePage : Page
    {
        private DatabaseManager dbManager;
        private int loggedInUserId = UserSession.UserId;
        public ObservableCollection<FavoriteProfile> FavoriteProfiles { get; set; }

        public FavoritePage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection();

            FavoriteProfiles = new ObservableCollection<FavoriteProfile>();
            DataContext = this; // Bind the page's data context to itself for binding
            LoadFavoriteProfiles();
        }

        private void LoadFavoriteProfiles()
        {
            try
            {
                string query = @"
                    SELECT a.id, a.benutzername, kp.profilbild, kp.geburtstag, kp.wohnort
                    FROM profile_favorites pf
                    JOIN accounts a ON pf.favorite_profile_id = a.id
                    LEFT JOIN kundenprofil kp ON a.id = kp.account_id
                    WHERE pf.favoriter_id = @loggedInUserId";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        FavoriteProfiles.Clear();
                        while (reader.Read())
                        {
                            int profileId = reader.GetInt32("id");
                            string username = reader["benutzername"].ToString();
                            string profileImagePath = reader.IsDBNull(reader.GetOrdinal("profilbild")) ? null : reader["profilbild"].ToString();
                            string location = reader.IsDBNull(reader.GetOrdinal("wohnort")) ? "Nicht angegeben" : reader["wohnort"].ToString();
                            DateTime? birthday = reader.IsDBNull(reader.GetOrdinal("geburtstag")) ? (DateTime?)null : reader.GetDateTime("geburtstag");

                            int? age = birthday.HasValue ? CalculateAge(birthday.Value) : (int?)null;

                            FavoriteProfiles.Add(new FavoriteProfile
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
                MessageBox.Show("Fehler beim Laden der Favoriten: " + ex.Message, "Fehler");
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

        private void RemoveFavorite(int profileId)
        {
            try
            {
                string query = "DELETE FROM profile_favorites WHERE favoriter_id = @loggedInUserId AND favorite_profile_id = @profileId";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);
                    cmd.Parameters.AddWithValue("@profileId", profileId);
                    cmd.ExecuteNonQuery();

                    // Entferne das Profil aus der Liste
                    var profileToRemove = FavoriteProfiles.FirstOrDefault(p => p.ProfileId == profileId);
                    if (profileToRemove != null)
                    {
                        FavoriteProfiles.Remove(profileToRemove);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Fehler beim Entfernen des Favoriten: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        private void OnRemoveFavoriteClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int profileId)
            {
                RemoveFavorite(profileId);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SwipePage());
        }
    }

    public class FavoriteProfile
    {
        public int ProfileId { get; set; }
        public string Username { get; set; }
        public string ProfileImagePath { get; set; }
        public string Age { get; set; }
        public string Location { get; set; }
    }
}
