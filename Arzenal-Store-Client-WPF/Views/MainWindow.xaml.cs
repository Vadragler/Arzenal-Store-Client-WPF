using Arzenal_Store_Client_WPF.Services;
using Arzenal_Store_Client_WPF.Services.API;
using Arzenal_Store_Client_WPF.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arzenal_Store_Client_WPF
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private ImageSource currentIcon;
        private MainViewModel viewModel;
        private ApiService _apiService;
        private System.Net.Http.HttpClient _httpClient;

        public MainWindow(ApiService apiService)
        {
            InitializeComponent();

            // Replace the following line:  
            // var serviceProvider = ServiceLocator.ConfigureServices();  

            // With the corrected implementation:  
            var serviceProvider = ServiceLocator.ConfigureServices();
            // Initialiser le ViewModel avec l'ApiService
            _viewModel = serviceProvider.GetRequiredService<MainViewModel>();

            // Assigner le DataContext pour lier le ViewModel à la vue
            DataContext = _viewModel;
            _apiService = apiService;

            // Charger les applications
            LoadAppsAsyncWrapper(); // Utilisation d'une méthode wrapper pour appeler l'opération asynchrone
        }

        // Ajout d'une méthode wrapper pour gérer l'appel asynchrone
        private async void LoadAppsAsyncWrapper()
        {
            bool load = await _viewModel.LoadAppsAsync();
            if (load)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataContext = _viewModel;
                    ListNonConnecter.Visibility = Visibility.Collapsed;
                    AppList.Visibility = Visibility.Visible;
                });
            }
        }

        public void UpdateDataContext()
        {
            // Mettre à jour le DataContext de la fenêtre principale
            Application.Current.Dispatcher.Invoke(() =>
            {
                DataContext = _viewModel;
            });
        }






        private async void AddApp_Click(object sender, RoutedEventArgs e)
        {
            //_dbconnection = _dbConnectionManager.Connection;
            FileService fileService = new FileService();
            string? filePath = fileService.SelectFile();
            if (filePath != null)
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                var appInfo = new AppModel
                {
                    Name = name, // Le nom de l'application
                    Description = null, // Description, ici `null`
                    Version = "Alpha 0.0.1 build 0", // Version de l'application
                    IsVisible = false, // Si l'application est visible ou non
                    FilePath = filePath, // Le chemin du fichier
                    IconePath = null, // Le chemin de l'icône, ici `null`
                    Tags = null, // Le tag, ici `null`
                    OperatingSystems = null, // Le système d'exploitation, ici `null`
                    Languages = null, // Langues, ici `null`
                    Requirements = null, // Exigences, ici `null`
                    Category = null, // Catégorie, ici `null`
                    ReleaseDate = null, // Date de sortie
                    LastUpdated = null, // Dernière mise à jour, ici `null`
                    AppSize = 0 // Taille de l'application
                };
                AppInfoWindow appInfoWindow = new AppInfoWindow(_apiService, appInfo);
                appInfoWindow.Owner = this;
                appInfoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                bool? result = appInfoWindow.ShowDialog();
                if (result == true)
                {


                    await _viewModel.LoadAppsAsync();
                }
                DataContext = _viewModel;
            }
        }



        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            //_dbconnection = _dbConnectionManager.Connection;
            SettingWindow StWindow = new SettingWindow(_apiService); // Fenêtre de paramètre
            StWindow.ShowDialog();
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

        private void AppList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppList.SelectedItem != null)
            {
                _viewModel.SelectedApp = AppList.SelectedItem as AppModel;
                AppList.Visibility = Visibility.Collapsed;
                DetailsView.Visibility = Visibility.Visible;
                AddAppButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await _viewModel.LoadAppsAsync())
            {
                int freePort = GetFreePort(); // Trouve un port libre
                string redirectUri = $"http://localhost:{freePort}/callback";
                string authUrl = $"http://localhost:56525/login?redirect_uri={HttpUtility.UrlEncode(redirectUri)}";

                // Démarre le mini serveur HTTP en arrière-plan
                _ = Task.Run(() => StartHttpListener(freePort));

                // Ouvre le navigateur
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });

                LoadAppsAsyncWrapper();
            }
        }

        private async Task StartHttpListener(int port)
        {
            string redirectUri = $"http://localhost:{port}/callback";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(redirectUri + "/");

            try
            {
                listener.Start();
                HttpListenerContext context = await listener.GetContextAsync(); // Attente de la requête asynchrone

                string token = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("token");
                App.Current.Properties["Token"] = token;
                _apiService.SetToken(token);
                var serviceProvider = ServiceLocator.ConfigureServices();
                _viewModel = serviceProvider.GetRequiredService<MainViewModel>();

                LoadAppsAsyncWrapper();

                // Répond à la requête
                string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ressources", "success.html");
                string responseString = File.ReadAllText(htmlPath);

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur dans le listener : {ex.Message}", "Erreur");
            }
            finally
            {
                listener.Stop();
            }



        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }
        private int GetFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }


    }

}
