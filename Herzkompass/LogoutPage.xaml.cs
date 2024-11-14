using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Herzkompass
{
    /// <summary>
    /// Interaktionslogik für LogoutPage.xaml
    /// </summary>
    public partial class LogoutPage : Page
    {
        public LogoutPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Erfolgreich abgemeldet. \nSie werden automatisch auf die Startseite umgeleitet." + UserSession.UserId.ToString(), "Erfolg");
            UserSession.ClearSession();
            MessageBox.Show(UserSession.UserId.ToString());

            // Navigiere zurück zum MainWindow, indem wir den Frame-Inhalt zurücksetzen.
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                // Leere den Frame, um zur ursprünglichen MainWindow-Ansicht zurückzukehren.
                mainWindow.MainFrame.Content = null;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SwipePage());
        }
    }
}
