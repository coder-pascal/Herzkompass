using MySqlConnector;
using System.Windows.Controls;
using System.Windows;

namespace Herzkompass
{
    public partial class HelpPage : Page
    {
        private DatabaseManager dbManager; // Verwalter der Datenbankverbindung
        private int loggedInUserId = UserSession.UserId; // ID des aktuell eingeloggten Benutzers

        public HelpPage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection(); // Initialisiert die Datenbankverbindung
        }

        // Event-Handler für das Einreichen eines Tickets
        private void OnSubmitTicketClick(object sender, RoutedEventArgs e)
        {
            string ticketContent = TicketInput.Text.Trim(); // Inhalt des Tickets vom Eingabefeld

            if (string.IsNullOrEmpty(ticketContent))
            {
                // Zeigt eine Fehlermeldung, wenn kein Text eingegeben wurde
                MessageBox.Show("Bitte geben Sie ein Anliegen ein.", "Hinweis");
                return;
            }

            try
            {
                // SQL-Abfrage zum Einfügen eines neuen Tickets in die Datenbank
                string query = "INSERT INTO tickets (account_id, ticket_text, created_at) VALUES (@accountId, @ticketText, NOW())";

                using (MySqlCommand cmd = new MySqlCommand(query, dbManager.Connection))
                {
                    cmd.Parameters.AddWithValue("@accountId", loggedInUserId); // Benutzer-ID als Parameter
                    cmd.Parameters.AddWithValue("@ticketText", ticketContent); // Ticket-Text als Parameter
                    cmd.ExecuteNonQuery(); // Führt die Abfrage aus
                }

                // Zeigt eine Erfolgsmeldung und leert das Eingabefeld
                MessageBox.Show("Ihr Anliegen wurde erfolgreich eingereicht. Wir melden uns bei Ihnen!", "Erfolg");
                TicketInput.Clear();
            }
            catch (MySqlException ex)
            {
                // Zeigt eine Fehlermeldung bei Datenbankproblemen
                MessageBox.Show("Fehler beim Einreichen des Tickets: " + ex.Message, "Fehler");
            }
            catch (Exception ex)
            {
                // Zeigt eine Fehlermeldung bei unerwarteten Problemen
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler");
            }
        }

        // Navigation zu anderen Seiten durch Button-Klick
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
            // Leer, da dies die aktuelle Seite ist
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogoutPage());
        }
    }
}
