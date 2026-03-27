using Arzenal.Dto.DTOs.AppDto;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Mapping;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;


namespace Arzenal.StoreManager.WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IAppApiService _apiService;
        private readonly IDialogService _dialogService;
        private readonly IUiDispatcher _uiDispatcher;
        private readonly IAuthService _authService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IWindowService _windowService;

        private readonly AppMapper _mapper = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand RemoveAppCommand { get; }
        public ICommand ModifyAppCommand { get; }
        public ICommand BackToListCommand { get; }
        public ICommand AddAppCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ReconnectCommand { get; }

        public MainViewModel(IAppApiService apiService, IDialogService dialogService,
                             IUiDispatcher uiDispatcher, IAuthService authService, IFileDialogService fileDialogService, IWindowService windowService)
        {
            _apiService = apiService;
            _dialogService = dialogService;
            _uiDispatcher = uiDispatcher;
            _authService = authService;
            _fileDialogService = fileDialogService;
            _windowService = windowService;

            _authService.ConnectionStateChanged += async connected =>
            {
                if (connected)
                    await LoadAppsAsync();
            };


            RemoveAppCommand = new RelayCommand<AppViewModel>(async appVm =>
            {
                if (appVm == null) return;
                await RemoveAppAsync(appVm.App);
            });

            ModifyAppCommand = new RelayCommand<AppViewModel>(async appVm =>
            {
                if (appVm == null) return;
                await ModifyAppAsync(appVm.App);
            });

            BackToListCommand = new RelayCommand(BackToList);
            AddAppCommand = new RelayCommand(async () => await AddAppAsync());
            OpenSettingsCommand = new RelayCommand(async () => await OpenSettingsAsync());
            ReconnectCommand = new RelayCommand(async () => await ReconnectAsync());
        }
        
        private async Task AddAppAsync(ReadAppDto app = null)
        {
            // Ne crée plus AppInfoWindow ici, juste le ViewModel
            var appInfoVm = new AppInfoViewModel(
                    this,
                    _fileDialogService,
                    _windowService,
                    _apiService);
            bool? result = await _dialogService.ShowAppEditDialogAsync(app);

            if (result == true)
                await LoadAppsAsync();
        }

        private async Task<bool> ModifyAppAsync(ReadAppDto app)
        {
            bool success = await _dialogService.ShowAppEditDialogAsync(app);
            if (success)
            {
                await LoadAppsAsync();
                SelectedApp = Apps.FirstOrDefault(a => a.App.Id == app.Id);
            }
            return success;
        }

        private async Task<bool> RemoveAppAsync(ReadAppDto app)
        {
            bool confirmed = await _dialogService.ShowConfirmationDialogAsync("Confirmer suppression ?");
            if (!confirmed) return false;

            var result = await _apiService.DeleteAsync($"api/apps/{app.Id}");
            if (result) await LoadAppsAsync();
            return result;
        }

        private async Task OpenSettingsAsync()
        {
            await _dialogService.ShowSettingsDialogAsync();
        }

        private async Task ReconnectAsync()
        {
            await _authService.AuthenticateAsync(); 

        }

        protected void SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }

        public async Task<bool> LoadAppsAsync()
        {
            var appsDto = await _apiService.GetAsync<List<ReadAppDto>>("api/apps");
            if (appsDto == null)
                return false;

            var appViewModels = appsDto.Select(appDto => new AppViewModel(appDto)).ToList();

            foreach (var appVm in appViewModels)
            {
                _ = appVm.LoadIconAsync(_apiService); // charge l'icône async sans bloquer
            }

            await _uiDispatcher.RunOnUiThreadAsync(() =>
            {
                Apps = new ObservableCollection<AppViewModel>(appViewModels);
            });

            return true;
        }

        private void BackToList() { SelectedApp = null; IsDetailsVisible = false; IsAddAppButtonVisible = true; }




        private ObservableCollection<AppViewModel> _apps = new();
        public ObservableCollection<AppViewModel> Apps
        {
            get => _apps;
            set => SetProperty(ref _apps, value);
        }

        private AppViewModel _selectedApp;
        public AppViewModel SelectedApp
        {
            get => _selectedApp;
            set => SetProperty(ref _selectedApp, value);
        }

        private bool _isDetailsVisible;
        public bool IsDetailsVisible
        {
            get => _isDetailsVisible;
            set => SetProperty(ref _isDetailsVisible, value);
        }

        private bool _isAddAppButtonVisible = true;
        public bool IsAddAppButtonVisible
        {
            get => _isAddAppButtonVisible;
            set => SetProperty(ref _isAddAppButtonVisible, value);
        }

    }

}
