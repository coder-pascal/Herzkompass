using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MySqlConnector;

namespace Herzkompass
{
    public partial class SwipePage : Page
    {
        private DatabaseManager dbManager;
        private int loggedInUserId = UserSession.UserId;
        private List<Profile> profileList = new List<Profile>();
        private int currentIndex = 0;

        public SwipePage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection();
            LoadProfiles();
            DisplayProfile();
        }

        private void LoadProfiles()
        {
            try
            {
                string query = @"
                    SELECT kp.account_id, a.benutzername, kp.geburtstag, kp.wohnort, kp.ueber_mich, kp.profilbild 
                    FROM kundenprofil kp
                    JOIN accounts a ON kp.account_id = a.id
                    WHERE kp.account_id != @loggedInUserId
                      AND kp.account_id NOT IN (SELECT liked_profile_id FROM profile_likes WHERE liker_id = @loggedInUserId)
                      AND kp.account_id NOT IN (SELECT favorite_profile_id FROM profile_favorites WHERE favoriter_id = @loggedInUserId)";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        profileList.Clear();
                        while (reader.Read())
                        {
                            profileList.Add(new Profile
                            {
                                AccountId = Convert.ToInt32(reader["account_id"]),
                                Username = reader["benutzername"].ToString(),
                                Birthday = Convert.ToDateTime(reader["geburtstag"]),
                                Location = reader["wohnort"].ToString(),
                                AboutMe = reader["ueber_mich"].ToString(),
                                ProfileImagePath = reader["profilbild"].ToString()
                            });
                        }
                    }
                }

                var random = new Random();
                profileList = profileList.OrderBy(x => random.Next()).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Profile: " + ex.Message);
            }
        }

        private void DisplayProfile()
        {
            if (currentIndex < profileList.Count)
            {
                var profile = profileList[currentIndex];

                txtName.Text = profile.Username;
                txtAge.Text = CalculateAge(profile.Birthday).ToString();
                txtLocation.Text = profile.Location;
                txtAboutMe.Text = profile.AboutMe;

                if (!string.IsNullOrEmpty(profile.ProfileImagePath))
                {
                    imgProfile.Source = new BitmapImage(new Uri(profile.ProfileImagePath, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgProfile.Source = null;
                }

                currentIndex++;
            }
            else
            {
                MessageBox.Show("Keine weiteren Profile zum Anzeigen.");
            }
        }

        private int CalculateAge(DateTime birthdate)
        {
            int age = DateTime.Now.Year - birthdate.Year;
            if (DateTime.Now.DayOfYear < birthdate.DayOfYear)
                age--;
            return age;
        }

        private void BtnDislike_Click(object sender, RoutedEventArgs e)
        {
            DisplayProfile();
        }

        private void BtnFavorite_Click(object sender, RoutedEventArgs e)
        {
            SaveFavorite(profileList[currentIndex - 1].AccountId);
            DisplayProfile();
        }

        private void BtnLike_Click(object sender, RoutedEventArgs e)
        {
            SaveLike(profileList[currentIndex - 1].AccountId);
            DisplayProfile();
        }

        private void SaveLike(int likedProfileId)
        {
            try
            {
                string query = "INSERT INTO profile_likes (liker_id, liked_profile_id) VALUES (@likerId, @likedProfileId)";
                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@likerId", loggedInUserId);
                    cmd.Parameters.AddWithValue("@likedProfileId", likedProfileId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des Likes: " + ex.Message);
            }
        }

        private void SaveFavorite(int favoriteProfileId)
        {
            try
            {
                string query = "INSERT INTO profile_favorites (favoriter_id, favorite_profile_id) VALUES (@favoriterId, @favoriteProfileId)";
                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@favoriterId", loggedInUserId);
                    cmd.Parameters.AddWithValue("@favoriteProfileId", favoriteProfileId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des Favoriten: " + ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogoutPage());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //leer da auf eigene Seite
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LikePage());
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new FavoritePage());
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HelpPage());
        }
    }

    public class Profile
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public DateTime Birthday { get; set; }
        public string Location { get; set; }
        public string AboutMe { get; set; }
        public string ProfileImagePath { get; set; }
    }
}
