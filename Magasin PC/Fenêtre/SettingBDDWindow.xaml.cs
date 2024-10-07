using System.Windows;
using System.Windows.Controls;
using Magasin_PC.Settings.Database;

namespace Magasin_PC
{
    /// <summary>
    /// Logique d'interaction pour AppSetting.xaml
    /// </summary>
    public partial class SettingBDDWindow : Window
    {
        public SettingBDDWindow()
        {
            InitializeComponent();
            DatabaseConfig config = ConfigManager.LoadConfig()!;
            if (config != null)
            {
                DockerHostTextBox.Text = config.Server;
                DockerPortTextBox.Text = config.Port;
                PasswordBox.Password = config.Password;
                ConnectionStringTextBox.Text = config.Database;
            }
        }

        private async void Valider(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.StopCheckingConnection();
            mainWindow.UpdateConnectionStatus(0);
            DatabaseConfig config = new DatabaseConfig();
            config.Server = DockerHostTextBox.Text;
            config.Database = ConnectionStringTextBox.Text;
            config.Port = DockerPortTextBox.Text;
            config.Password = PasswordBox.Password;
            config.User = "root";
            ConfigManager.SaveConfig(config);
            this.Close();
            await mainWindow.ConnectToDatabaseAsync();
            await mainWindow.StartCheckingConnection();

        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Connexion BDD" ||
                    textBox.Text == "Adresse IP du Docker" ||
                    textBox.Text == "Port Docker")
                {
                    textBox.Text = string.Empty;
                    textBox.Foreground = System.Windows.Media.Brushes.Black; // Change couleur du texte
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox.Name == "ConnectionStringTextBox")
                    {
                        textBox.Text = "Connexion BDD";
                    }
                    else if (textBox.Name == "DockerHostTextBox")
                    {
                        textBox.Text = "Adresse IP du Docker";
                    }
                    else if (textBox.Name == "DockerPortTextBox")
                    {
                        textBox.Text = "Port Docker";
                    }
                    textBox.Foreground = System.Windows.Media.Brushes.Gray; // Remettre la couleur par défaut
                }
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Pour le PasswordBox, tu peux gérer un texte d'espace réservé de manière similaire
            if (PasswordBox.Password == "Mot de passe")
            {
                PasswordBox.Clear();
                PasswordBox.Foreground = System.Windows.Media.Brushes.Black; // Change couleur du texte
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordBox.Password = "Mot de passe";
                PasswordBox.Foreground = System.Windows.Media.Brushes.Gray; // Remettre la couleur par défaut
            }
            else
            {

            }
        }
    }
}
