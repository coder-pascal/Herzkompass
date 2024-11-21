using System.Windows.Controls;
using System.Windows;

namespace Herzkompass
{
    /// <summary>
    /// Interaktionslogik für LogoutPage.xaml
    /// </summary>
    public partial class LogoutPage : Page
    {
        // Konstruktor für die Logout-Seite, initialisiert die Seite
        public LogoutPage()
        {
            InitializeComponent();
        }

        // Event-Handler für den Klick auf den Logout-Button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Zeigt eine MessageBox an, die bestätigt, dass der Benutzer abgemeldet wurde.
            MessageBox.Show("Erfolgreich abgemeldet. \nSie werden automatisch auf die Startseite umgeleitet." + UserSession.UserId.ToString(), "Erfolg");

            // Löscht die Benutzersession, um den Benutzer abzumelden
            UserSession.ClearSession();

            // Zeigt die UserId nach dem Abmelden an (dies ist möglicherweise für Debugging-Zwecke)
            MessageBox.Show(UserSession.UserId.ToString());

            // Versucht, das Hauptfenster zu finden und den Frame-Inhalt zurückzusetzen
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                // Leert den Frame-Inhalt, um zur ursprünglichen Ansicht des MainWindow zurückzukehren
                mainWindow.MainFrame.Content = null;
            }
        }

        // Event-Handler für den Klick auf den Button, um zur HomePage zu navigieren
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }

        // Event-Handler für den Klick auf den Button, um zur SettingsPage zu navigieren
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        // Event-Handler für den Klick auf den Button, um zur SwipePage zu navigieren
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SwipePage());
        }

        // Event-Handler für den Klick auf den Button, um zur LikePage zu navigieren
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LikePage());
        }

        // Event-Handler für den Klick auf den Button, um zur FavoritePage zu navigieren
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new FavoritePage());
        }

        // Event-Handler für den Klick auf den Button, um zur HelpPage zu navigieren
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HelpPage());
        }

        // Event-Handler für den Klick auf den Button, der keine Navigation ausführt (leere Seite)
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            // leer da eigene Seite
        }
    }
}
