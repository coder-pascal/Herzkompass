using System;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace Herzkompass
{
    public partial class HelpPage : Page
    {
        private DatabaseManager dbManager;
        private int loggedInUserId = UserSession.UserId; // Angemeldeter Benutzer

        public HelpPage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection();
        }

        private void OnSubmitTicketClick(object sender, RoutedEventArgs e)
        {
            string ticketContent = TicketInput.Text.Trim();

            if (string.IsNullOrEmpty(ticketContent))
            {
                MessageBox.Show("Bitte geben Sie ein Anliegen ein.", "Hinweis");
                return;
            }

            try
            {
                string query = "INSERT INTO tickets (account_id, ticket_text, created_at) VALUES (@accountId, @ticketText, NOW())";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@accountId", loggedInUserId);
                    cmd.Parameters.AddWithValue("@ticketText", ticketContent);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Ihr Anliegen wurde erfolgreich eingereicht. Wir melden uns bei Ihnen!", "Erfolg");
                TicketInput.Clear();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Fehler beim Einreichen des Tickets: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

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
            this.NavigationService.Navigate(new LikePage());
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
            // leer da eigene Seite
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogoutPage());
        }
    }
}
