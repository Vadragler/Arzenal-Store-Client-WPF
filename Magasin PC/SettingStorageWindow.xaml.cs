using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;


namespace Magasin_PC
{
    /// <summary>
    /// Logique d'interaction pour AppSetting.xaml
    /// </summary>
    public partial class SettingStorageWindow : Window
    {
        public SettingStorageWindow()
        {
            InitializeComponent();
            StorageConfig config = ConfigManager.LoadStorageConfig();
            if (config != null)
            {
                DockerHostTextBox.Text = config.DockerHost;
                DockerPortTextBox.Text = config.DockerPort;
                PasswordBox.Password = config.Password;
                Username.Text = config.User;
            }
        }

        private async void ValiderSftp(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.DisconnectSftp(); // Déconnecter l'ancienne connexion SFTP
            mainWindow.ChangeConnectionIcon(0);
            StorageConfig config = new StorageConfig();
            // Remplacez par vos valeurs de champ
            config.DockerHost = DockerHostTextBox.Text;
            config.DockerPort = DockerPortTextBox.Text;
            config.User = Username.Text; // ou un autre champ d'utilisateur
            config.Password = PasswordBox.Password; // Assurez-vous de gérer le mot de passe
            ConfigManager.SaveStorageConfig(config);
            this.Close();
            mainWindow.ConnectToSftp();
        }



        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Username" ||
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
                    if (textBox.Name == "Username")
                    {
                        textBox.Text = "Username";
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
