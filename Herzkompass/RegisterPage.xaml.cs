using Bcrypt;
using MySqlConnector;
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
    /// Interaktionslogik für RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private DatabaseManager dbManager;
        public RegisterPage()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitConnection();
        }

        // Register Button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Überprüfung ob alle Felder ausgefüllt sind
            if (!string.IsNullOrEmpty(txtEmail.Text) && !string.IsNullOrEmpty(txtBenutzername.Text) && !string.IsNullOrEmpty(txtPasswort.Password) && !string.IsNullOrEmpty(txtPasswort2.Password))
            {
                // Überprüfung ob beide Passwörter übereinstimmen und mehr als 0 Zeichen haben, damit es keine leeren Passwörter gibt
                if (txtPasswort.Password == txtPasswort2.Password && txtPasswort2.Password.Length > 0)
                {
                    try
                    {
                        // Überprüfen, ob der Benutzername oder die E-Mail bereits vorhanden sind
                        MySqlCommand checkCommand = dbManager.Connection.CreateCommand();
                        checkCommand.CommandText = "SELECT COUNT(*) FROM accounts WHERE benutzername = @benutzername OR email = @email";
                        checkCommand.Parameters.AddWithValue("@benutzername", txtBenutzername.Text);
                        checkCommand.Parameters.AddWithValue("@email", txtEmail.Text);

                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Benutzername oder E-Mail-Adresse ist bereits registriert.", "Fehler");
                            return;
                        }

                        // Passwort mit Bcrypt hashen
                        string hashedPassword = BCrypt.HashPassword(txtPasswort.Password, BCrypt.GenerateSalt());

                        MySqlCommand command = dbManager.Connection.CreateCommand();
                        command.CommandText = "INSERT INTO accounts (benutzername, email, passwort) VALUES (@benutzername, @email, @passwort)";
                        command.Parameters.AddWithValue("@benutzername", txtBenutzername.Text);
                        command.Parameters.AddWithValue("@email", txtEmail.Text);
                        command.Parameters.AddWithValue("@passwort", hashedPassword);

                        command.ExecuteNonQuery();
                        MessageBox.Show("Willkommen bei Herzkompass!", "Registrierung erfolgreich");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Fehler bei der Registrierung: " + ex.Message, "Fehler");
                    }
                    finally
                    {
                        // Ensure the connection is closed
                        if (dbManager.Connection.State == System.Data.ConnectionState.Open)
                        {
                            dbManager.Connection.Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Die Passwörter stimmen nicht überein oder sind leer.", "Fehler");
                }
            }
            else
            {
                MessageBox.Show("Bitte füllen Sie zuerst alle Felder aus.", "Fehler");
            }
        }

        // Zurück zur Anmeldung
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Navigiere zurück zum MainWindow, indem wir den Frame-Inhalt zurücksetzen.
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                // Leere den Frame, um zur ursprünglichen MainWindow-Ansicht zurückzukehren.
                mainWindow.MainFrame.Content = null;
            }
        }
    }
}