using Arzenal.Shared.Dtos.DTOs.AppDto;
using Arzenal_Store_Client_WPF.Services;
using Arzenal_Store_Client_WPF.Services.API; // Vos modèles (AppModel, CategorieModel, etc.)
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Arzenal_Store_Client_WPF.Settings
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand RemoveAppCommand { get; }
        public ICommand ModifyAppCommand { get; } // Commande pour modifier une application

        private readonly IDialogService _dialogService;
        private readonly ApiService _apiService;

        public MainViewModel(ApiService apiService, IDialogService dialogService)
        {
            _apiService = apiService;
            _dialogService = dialogService;
            // Initialisation de la commande avec une action
            RemoveAppCommand = new RelayCommand<AppModel>(async app => await RemoveAppAsync(app));
            ModifyAppCommand = new RelayCommand<AppModel>(async app => await ModifyAppAsync(app));

            // Correction : Utiliser une instance de MainWindow pour appeler UpdateDataContext()

        }

        private async Task<bool> ModifyAppAsync(AppModel app)
        {
            // Créer une instance de la fenêtre AppInfoWindow
            AppInfoWindow appInfoWindow = new AppInfoWindow(_apiService, app);
            appInfoWindow.Owner = Application.Current.MainWindow; // Définir le propriétaire de la fenêtre
            appInfoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Utiliser un cast explicite pour convertir le bool? en bool
            bool? dialogResult = appInfoWindow.ShowDialog();
            bool success = dialogResult ?? false; // Utiliser l'opérateur de coalescence pour gérer les valeurs null
            if (success)
            {
                await LoadAppsAsync(); // recharge la liste
                                       // Réaffecte la version modifiée à SelectedApp pour la refléter dans la vue
                var updatedApp = Apps.FirstOrDefault(a => a.Id == app.Id);
                if (updatedApp != null)
                {
                    SelectedApp = updatedApp;
                }
            }

            return success;

        }

        public async Task<bool> RemoveAppAsync(AppModel app)
        {
            var tcs = new TaskCompletionSource<bool>();

            _ = _dialogService.ShowConfirmationDialog(
                "Êtes-vous sûr de vouloir supprimer cette application ?",
                async confirmed =>
                {
                    if (confirmed)
                    {
                        var result = await _apiService.DeleteAsync($"api/apps/{app.Id}");

                        if (result)
                        {
                            await LoadAppsAsync(); // ⬅️ recharge la liste à partir de l'API
                            tcs.SetResult(true);
                        }
                        else
                        {
                            tcs.SetResult(false);
                        }
                    }
                    else
                    {
                        tcs.SetResult(false);
                    }
                });

            return await tcs.Task;
        }



        private ObservableCollection<AppModel> _apps = new ObservableCollection<AppModel>();
        public ObservableCollection<AppModel> Apps
        {
            get => _apps;
            set
            {
                _apps = value;
                OnPropertyChanged(nameof(Apps)); // Pass the property name explicitly
            }
        }



        private AppModel _selectedApp;
        public AppModel SelectedApp
        {
            get => _selectedApp;
            set
            {
                if (_selectedApp != value)
                {
                    _selectedApp = value;
                    OnPropertyChanged(nameof(SelectedApp));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Charge les applications depuis l'API et remplit la collection Apps.
        /// </summary>
        public async Task<bool> LoadAppsAsync()
        {
            // Appel à l'API pour récupérer les données déjà désérialisées
            var apps = await _apiService.GetAsync<List<ReadAppDto>>("api/apps");
            if (apps == null)
            {
                return false; // Gérer l'erreur de manière appropriée
            }
            else
            {
                // Préparer une liste temporaire pour les modifications
                var tempApps = new List<AppModel>();

                foreach (var appDto in apps)
                {
                    var app = new AppModel
                    {
                        Id = appDto.Id,
                        Name = appDto.Name,
                        Description = appDto.Description,
                        Version = appDto.Version,
                        FilePath = appDto.FilePath,
                        IsVisible = appDto.IsVisible,
                        IconePath = appDto.IconePath,
                        Requirements = appDto.Requirements,
                        ReleaseDate = appDto.ReleaseDate,
                        LastUpdated = appDto.LastUpdated,
                        AppSize = appDto.AppSize,
                        Category = appDto.Category,
                        OperatingSystems = appDto.OperatingSystems,
                        Languages = appDto.Languages,
                        Tags = appDto.Tags
                    };

                    // Charger les données supplémentaires
                    if (!string.IsNullOrEmpty(app.IconePath))
                    {
                        app.Icone = await LoadIconAsync(app.IconePath);
                    }

                    tempApps.Add(app);
                }
                // Effectuer toutes les modifications sur le thread UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Apps = new ObservableCollection<AppModel>(tempApps);
                });


                return true; // Retourner true si le chargement a réussi
            }
        }


        public async Task<ImageSource> LoadIconAsync(string iconPath)
        {/*
            try
            {
                var response = await _apiService.GetAsync<HttpResponseMessage>($"http://localhost:5181/api/icons/{iconPath}"); // Remplacez par votre URL d'API
                response.EnsureSuccessStatusCode();

                var iconStream = await response.Content.ReadAsStreamAsync();
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = iconStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); // Assurez-vous que l'image est utilisable dans un contexte multi-thread

                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'icône : {ex.Message}");
                return null;
            }*/
            return null; // Remplacez par votre logique de chargement d'icône
        }



        public void AddApp(AppModel app)
        {
            if (!Apps.Contains(app))
            {
                Apps.Add(app);
            }
        }


    }
}
