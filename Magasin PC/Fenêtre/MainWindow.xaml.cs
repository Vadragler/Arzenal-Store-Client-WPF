using Magasin_PC.Settings;
using Magasin_PC.Settings.Database;
using Magasin_PC.Settings.SFTP;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net;

namespace Magasin_PC
{
    public partial class MainWindow : Window
    {
        private ImageSource currentIcon;

        private MainViewModel viewModel;

        private System.Timers.Timer connectionCheckTimer;

        protected MySqlConnection connection { get; private set; }
        public bool IsConnected {  get;  set; } = false;

        private CancellationTokenSource? _cts;

        private SftpClient _sftpclient = new SftpClient();
        private SFTPManager _sftpmanager;

        private CancellationTokenSource _sftpConnectionCancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ConnectToSftp();
            await ConnectToDatabaseAsync();
            await StartCheckingConnection();
        }


        public bool GetConnectionStatus() => IsConnected;


        public MySqlConnection GetConnectionInfos() => connection;
        

        public async Task StartCheckingConnection()
        {
            // Si la tâche précédente est en cours, l'annuler
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }

            // Créer un nouveau CancellationTokenSource
            _cts = new CancellationTokenSource();

            try
            {
                // Lancer la méthode CheckConnectionStatus avec le nouveau token
                await CheckConnectionStatus(_cts.Token);
            }
            catch (TaskCanceledException)
            {
                // La tâche a été annulée
            }
        }

        public void StopCheckingConnection()
        {
            // Annuler la vérification de la connexion en cours
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }


        private async Task CheckConnectionStatus(CancellationToken cancellationToken)
        {
            bool isConnected = true;

            while (!cancellationToken.IsCancellationRequested) // Vérifier régulièrement si l'annulation est demandée
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); // Vérifier la connexion toutes les 2 secondes, gérer l'annulation

                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    try
                    {
                        connection.Ping();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UpdateConnectionStatus(1); // Connexion OK
                        });
                        isConnected = true;
                        IsConnected = true;
                    }
                    catch
                    {
                        IsConnected = false;
                        isConnected = false; // La connexion est perdue
                    }
                }
                else
                {
                    IsConnected = false;
                    isConnected = false;
                }

                if (!isConnected)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateConnectionStatus(-1); // Indiquer que la connexion est perdue
                    });

                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken); // Attendre 1 minute avant la prochaine tentative

                    try
                    {
                        await ConnectToDatabaseAsync(); // Tenter de se reconnecter
                    }
                    catch (Exception)
                    {
                        // Gérer l'échec de la reconnexion ici
                    }
                }
            }
        }


        public async Task ConnectToDatabaseAsync()
        {
           UpdateConnectionStatus(0); // Mise à jour de l'icône en vert (connexion réussie).
           
            try
            {
                connection = new MySqlConnection(LoadConnectionString());

                await connection.OpenAsync();
                IsConnected = true;
                viewModel = new MainViewModel(this, _sftpmanager);
                await viewModel.LoadAppsAsync();
                DataContext = viewModel;
                DatabaseManager _databasemanager = new DatabaseManager(connection,this);
                await _databasemanager.OnDatabaseReconnect();
                UpdateConnectionStatus(1); // Mise à jour de l'icône en vert (connexion réussie).
            }
            catch (Exception)
            {
                IsConnected = false;
                UpdateConnectionStatus(-1); // Mise à jour de l'icône en vert (connexion réussie).
            }
        }

        public void UpdateConnectionStatus(int isConnected)
        {
            DrawingImage databaseIcon = (DrawingImage)FindResource("DatabaseIcon");

            // Accéder au GeometryDrawing
            DrawingGroup drawingGroup = (DrawingGroup)databaseIcon.Drawing;
            GeometryDrawing geometryDrawing = (GeometryDrawing)drawingGroup.Children[0];
            switch (isConnected)
            {
                case 1:
                    // Changer la couleur
                    geometryDrawing.Brush = new SolidColorBrush(Colors.Green);
                break;
                case -1:
                    // Changer la couleur
                    geometryDrawing.Brush = new SolidColorBrush(Colors.Red);
                 break;
                case 0:
                    // Changer la couleur
                    geometryDrawing.Brush = new SolidColorBrush(Colors.Blue);
                break;
            }
        }

        public void ChangeConnectionIcon(int state)
        {
            switch (state)
            {
                case 0:
                    currentIcon = (ImageSource)FindResource("StorageConnexionIcon");
                    break;
                case 1:
                    currentIcon = (ImageSource)FindResource("StorageValidIcon");
                    break;
                case -1:
                    currentIcon = (ImageSource)FindResource("StorageLostIcon");
                    break;
            }

            // Met à jour l'icône du bouton
            ConnectionSftpStatusButton.Tag = currentIcon; // Ou ConnectionSftpStatusButton.Content si tu préfères
        }


        private string LoadConnectionString()
        {
            // Charger la chaîne de connexion à partir de la configuration
            DatabaseConfig config = ConfigManager.LoadConfig()!;
            return $"Server={config.Server};Port={config.Port};Database={config.Database};User Id={config.User};Pwd={config.Password};";
        }


        private void ConnectionStatusButton_Click(object sender, RoutedEventArgs e)
        {
            SettingBDDWindow dbWindow = new SettingBDDWindow(); // Fenêtre de gestion BDD
            dbWindow.ShowDialog();
        }

        private void ConnectionStorageButton_Click(object sender, RoutedEventArgs e)
        {
            SettingStorageWindow dbWindow = new SettingStorageWindow(); // Fenêtre de gestion BDD
            dbWindow.ShowDialog();
        }

        public async Task ConnectToSftp()
        {
            ChangeConnectionIcon(0);
            // Remplacez les valeurs par les données de configuration appropriées
            StorageConfig config = ConfigManager.LoadStorageConfig()!;
            if (config != null)
            {
                await _sftpclient.ConnectAsync(config.DockerHost!, config.User!, config.Password!, int.Parse(config.DockerPort!));
                _sftpmanager = new SFTPManager(_sftpclient.GetClient());
               _sftpclient.StartCheckingSftpConnection(config.DockerHost!, config.User!, config.Password!, int.Parse(config.DockerPort!));
            }
        }

        public void DisconnectSftp() => _sftpclient.Disconnect();
        
        public void StopCheckingSftpConnection() => _sftpclient.StopCheckingSftpConnection();
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopCheckingSftpConnection();
            //StopCheckingConnection();
            DisconnectSftp();
            //DisconnectBdd();
            base.OnClosing(e);
        }

        private void DisconnectBdd()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
                StopCheckingConnection();
            }
        }

        private void AppList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppList.SelectedItem != null)
            {
                viewModel.SelectedApp = AppList.SelectedItem as AppModel;
                AppList.Visibility = Visibility.Collapsed;
                DetailsView.Visibility = Visibility.Visible;
               AddAppButton.Visibility = Visibility.Collapsed;
            }
        }

        private void BackToList_Click(object sender, RoutedEventArgs e)
        {
            AppList.SelectedItem = null;
            AppList.Visibility = Visibility.Visible;
            DetailsView.Visibility = Visibility.Collapsed;
            AddAppButton.Visibility = Visibility.Visible;
        }

        private void AppList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Si l'utilisateur clique à l'extérieur d'un élément sélectionné
            if (AppList.SelectedItem != null)
            {
                // Vérifie si le clic s'est produit à l'extérieur de l'élément sélectionné
                var point = e.GetPosition(AppList);
                var hitTestResult = VisualTreeHelper.HitTest(AppList, point);

                if (hitTestResult == null || !(hitTestResult.VisualHit is FrameworkElement element) || element.DataContext != AppList.SelectedItem)
                {
                    // Désélectionner l'élément
                    AppList.SelectedItem = null;
                }
            }
        }

        private async void ModifyApp_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var AppSelected = button?.DataContext as AppModel;
            if (AppSelected != null)
            {
                AppInfoWindow appInfoWindow = new AppInfoWindow(AppSelected.Id, 
                                                                AppSelected.Name, 
                                                                AppSelected.Description, 
                                                                AppSelected.Version, 
                                                                AppSelected.IsVisible, 
                                                                AppSelected.FilePath, 
                                                                AppSelected.Icone,
                                                                AppSelected.IconePath,
                                                                AppSelected.Tag,
                                                                AppSelected.OS,
                                                                AppSelected.Languages,
                                                                AppSelected.Requirements,
                                                                AppSelected.Category,
                                                                AppSelected.ReleaseDate,
                                                                AppSelected.LastUpdated,
                                                                AppSelected.AppSize,
                                                                this,
                                                                _sftpclient
                                                                );
                appInfoWindow.Owner = this;
                appInfoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                bool? result = appInfoWindow.ShowDialog();
                if (result == true)
                {
                    AppSelected.Icone = appInfoWindow.icone;
                    AppSelected.IconePath = appInfoWindow.iconepath;
                    AppSelected.Name = appInfoWindow.name;
                    AppSelected.Version = appInfoWindow.version;
                    AppSelected.Description = appInfoWindow.description;
                    AppSelected.IsVisible = appInfoWindow.isvisible;
                    AppSelected.FilePath = appInfoWindow.filepath;
                    AppSelected.Tag = appInfoWindow.selectedTags;
                    AppSelected.OS = appInfoWindow.selectedOS;
                    AppSelected.Category = appInfoWindow.category;
                    AppSelected.LastUpdated = appInfoWindow.lastupdated;
                    AppSelected.AppSize = appInfoWindow.appsize;
                    AppSelected.Languages = appInfoWindow.selectedLanguages;
                    AppSelected.Requirements = appInfoWindow.requirements;
                    viewModel = new MainViewModel(this, _sftpmanager);
                    await viewModel.LoadAppsAsync();
                    DataContext = viewModel;

                }
            }
        }

        private async void AddApp_Click(object sender, RoutedEventArgs e)
        {
            FileService fileService = new FileService();
            string? filePath = fileService.SelectFile(); 
            if (filePath != null)
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                AppInfoWindow appInfoWindow = new AppInfoWindow(Guid.NewGuid(), name, null, "Alpha 0.0.1 build 0", false, filePath, null, null, null ,null,null,null,null,DateTime.Now,null,0,this,_sftpclient);
                appInfoWindow.Owner = this;
                appInfoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                bool? result = appInfoWindow.ShowDialog();
                if (result == true)
                {
                    AppModel newApp = new AppModel
                    {
                        Id = appInfoWindow.id,
                        Icone = appInfoWindow.icone,
                        Name = appInfoWindow.name,
                        FilePath = filePath,
                        Version = appInfoWindow.version,
                        Description = appInfoWindow.description,
                        IsVisible = appInfoWindow.isvisible,
                        Tag = appInfoWindow.selectedTags,
                        OS = appInfoWindow.selectedOS,
                        Category = appInfoWindow.category,
                        ReleaseDate = appInfoWindow.releasedate,
                        LastUpdated = appInfoWindow.lastupdated,
                        AppSize = appInfoWindow.appsize,
                        Languages = appInfoWindow.selectedLanguages,
                        Requirements = appInfoWindow.requirements
                    };
                    ((MainViewModel)this.DataContext).AddApp(newApp);
                }
                viewModel = new MainViewModel(this, _sftpmanager);
                await viewModel.LoadAppsAsync();
                DataContext = viewModel;
            }
        }

        private void RemoveApp_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedApp = button?.DataContext as AppModel;  // Récupère l'application sélectionnée dans la liste
            var confirmationWindow = new ConfirmationWindow(this);

            // Positionner la fenêtre de confirmation au centre de la fenêtre principale
            confirmationWindow.Owner = this; // Spécifie la fenêtre principale comme propriétaire
            confirmationWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Affichez la fenêtre de confirmation de manière modale
            confirmationWindow.ShowDialog();

            if (confirmationWindow.DialogResult == true)
            {
                if (selectedApp != null)
                {
                    ((MainViewModel)this.DataContext).RemoveApp(selectedApp);
                    DetailsView.Visibility = Visibility.Collapsed;
                    AppList.Visibility = Visibility.Visible;
                    AddAppButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow StWindow = new SettingWindow(this); // Fenêtre de paramètre
            StWindow.ShowDialog();
        }
    }
}
