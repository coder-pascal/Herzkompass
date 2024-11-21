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
        private DatabaseManager dbManager; // Verwalter für die Datenbankverbindung
        private int loggedInUserId = UserSession.UserId; // ID des aktuell eingeloggten Benutzers
        private List<Profile> profileList = new List<Profile>(); // Liste von Profilen
        private int currentIndex = 0; // Index des aktuell angezeigten Profils

        public SwipePage()
        {
            InitializeComponent(); // Initialisiert die XAML-Komponenten
            dbManager = new DatabaseManager(); // Erstellt eine Instanz des DatabaseManagers
            dbManager.InitConnection(); // Initialisiert die Datenbankverbindung
            LoadProfiles(); // Lädt die Profile aus der Datenbank
            DisplayProfile(); // Zeigt das erste Profil an
        }

        // Lädt Profile aus der Datenbank
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
                    cmd.Parameters.AddWithValue("@loggedInUserId", loggedInUserId); // Fügt die Benutzer-ID als Parameter hinzu

                    using (MySqlDataReader reader = cmd.ExecuteReader()) // Führt die Abfrage aus
                    {
                        profileList.Clear(); // Leert die Liste der Profile
                        while (reader.Read()) // Liest jedes Profil
                        {
                            // Fügt jedes Profil zur Liste hinzu
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
                profileList = profileList.OrderBy(x => random.Next()).ToList(); // Mischt die Profile zufällig
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Profile: " + ex.Message); // Fehlermeldung, falls etwas schiefgeht
            }
        }

        // Zeigt das nächste Profil an
        private void DisplayProfile()
        {
            if (currentIndex < profileList.Count) // Überprüft, ob noch Profile vorhanden sind
            {
                var profile = profileList[currentIndex]; // Holt das aktuelle Profil

                txtName.Text = profile.Username; // Setzt den Benutzernamen
                txtAge.Text = CalculateAge(profile.Birthday).ToString(); // Berechnet und zeigt das Alter
                txtLocation.Text = profile.Location; // Setzt den Wohnort
                txtAboutMe.Text = profile.AboutMe; // Setzt den Text "Über mich"

                if (!string.IsNullOrEmpty(profile.ProfileImagePath)) // Überprüft, ob ein Profilbild vorhanden ist
                {
                    imgProfile.Source = new BitmapImage(new Uri(profile.ProfileImagePath, UriKind.RelativeOrAbsolute)); // Zeigt das Profilbild an
                }
                else
                {
                    imgProfile.Source = null; // Setzt das Bild auf null, wenn keines vorhanden ist
                }

                currentIndex++; // Erhöht den Index für das nächste Profil
            }
            else
            {
                MessageBox.Show("Keine weiteren Profile zum Anzeigen."); // Zeigt eine Nachricht an, wenn keine Profile mehr übrig sind
            }
        }

        // Berechnet das Alter basierend auf dem Geburtsdatum
        private int CalculateAge(DateTime birthdate)
        {
            int age = DateTime.Now.Year - birthdate.Year;
            if (DateTime.Now.DayOfYear < birthdate.DayOfYear) // Überprüft, ob der Geburtstag in diesem Jahr schon war
                age--; // Wenn nicht, wird das Alter um 1 verringert
            return age;
        }

        // Wird aufgerufen, wenn der "Dislike"-Button gedrückt wird
        private void BtnDislike_Click(object sender, RoutedEventArgs e)
        {
            DisplayProfile(); // Zeigt das nächste Profil an
        }

        // Wird aufgerufen, wenn der "Favorite"-Button gedrückt wird
        private void BtnFavorite_Click(object sender, RoutedEventArgs e)
        {
            SaveFavorite(profileList[currentIndex - 1].AccountId); // Speichert das Profil als Favorit
            DisplayProfile(); // Zeigt das nächste Profil an
        }

        // Wird aufgerufen, wenn der "Like"-Button gedrückt wird
        private void BtnLike_Click(object sender, RoutedEventArgs e)
        {
            SaveLike(profileList[currentIndex - 1].AccountId); // Speichert das Profil als "Like"
            DisplayProfile(); // Zeigt das nächste Profil an
        }

        // Speichert ein "Like" für das Profil
        private void SaveLike(int likedProfileId)
        {
            try
            {
                string query = "INSERT INTO profile_likes (liker_id, liked_profile_id) VALUES (@likerId, @likedProfileId)";
                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@likerId", loggedInUserId); // Fügt die ID des Benutzers hinzu
                    cmd.Parameters.AddWithValue("@likedProfileId", likedProfileId); // Fügt die ID des "gelikten" Profils hinzu
                    cmd.ExecuteNonQuery(); // Führt die Abfrage aus
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des Likes: " + ex.Message); // Fehlermeldung, falls etwas schiefgeht
            }
        }

        // Speichert ein "Favorit" für das Profil
        private void SaveFavorite(int favoriteProfileId)
        {
            try
            {
                string query = "INSERT INTO profile_favorites (favoriter_id, favorite_profile_id) VALUES (@favoriterId, @favoriteProfileId)";
                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@favoriterId", loggedInUserId); // Fügt die ID des Benutzers hinzu
                    cmd.Parameters.AddWithValue("@favoriteProfileId", favoriteProfileId); // Fügt die ID des Favoriten hinzu
                    cmd.ExecuteNonQuery(); // Führt die Abfrage aus
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des Favoriten: " + ex.Message); // Fehlermeldung, falls etwas schiefgeht
            }
        }

        // Navigiert zur Startseite
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }

        // Navigiert zur Einstellungsseite
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        // Navigiert zur Logout-Seite
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogoutPage());
        }

        // Button ohne Funktion
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //leer da auf eigene Seite
        }

        // Navigiert zur Like-Seite
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LikePage());
        }

        // Navigiert zur Favoriten-Seite
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new FavoritePage());
        }

        // Navigiert zur Hilfeseite
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HelpPage());
        }
    }

    // Klasse für Profile
    public class Profile
    {
        public int AccountId { get; set; } // ID des Profils
        public string Username { get; set; } // Benutzername
        public DateTime Birthday { get; set; } // Geburtsdatum
        public string Location { get; set; } // Wohnort
        public string AboutMe { get; set; } // Text "Über mich"
        public string ProfileImagePath { get; set; } // Pfad zum Profilbild
    }
}
