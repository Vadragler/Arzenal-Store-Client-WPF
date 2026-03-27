using Arzenal.Dto.DTOs.AppDto;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Models;
using Arzenal.StoreManager.ViewModels;
using Arzenal.StoreManager.WPF.ViewModel;
using Arzenal.StoreManager.WPF.Views;
using Arzenal.StoreManager.WPF.Views.AppInfo;
using Arzenal.StoreManager.WPF.Views.Settings;
using System.Windows;

namespace Arzenal.StoreManager.WPF.Services
{


    public class DialogService : IDialogService
    {
        private readonly IAppApiService _appApiService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IWindowService _windowService;

        public DialogService(IAppApiService appApiService, IFileDialogService fileDialogService, IWindowService windowService)
        {
            _appApiService = appApiService;
            _fileDialogService = fileDialogService;
            _windowService = windowService;
        }
        /// <summary>
        /// Affiche une fenêtre de confirmation avec un callback Action<bool>.
        /// </summary>
        public async Task ShowConfirmationDialog(string message, Action<bool> onConfirm)
        {
            bool result = await ShowConfirmationDialogAsync(message);
            onConfirm(result);
        }

        /// <summary>
        /// Affiche une fenêtre de confirmation asynchrone.
        /// </summary>
        public async Task<bool> ShowConfirmationDialogAsync(string message)
        {
            return await Task.Run(() =>
            {
                var confirmationWindow = new ConfirmationWindow
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };


                bool? dialogResult = confirmationWindow.ShowDialog();
                return dialogResult ?? false;
            });
        }

        /// <summary>
        /// Ouvre la fenêtre d’édition d’une application (AppInfoWindow) et retourne true si l’utilisateur a validé.
        /// </summary>
        public async Task<bool> ShowAppEditDialogAsync(ReadAppDto selectedApp)
        {
            var mainViewModel = App.Current.MainWindow.DataContext as MainViewModel;

            AppInfoViewModel appInfoViewModel;

            if (selectedApp != null)
                appInfoViewModel = new AppInfoViewModel(
                    mainViewModel,
                    _fileDialogService,
                    _windowService,
                    _appApiService, selectedApp);
            else
                appInfoViewModel = new AppInfoViewModel(
                    mainViewModel,
                    _fileDialogService,
                    _windowService,
                    _appApiService);

            var appInfoWindow = new AppInfoWindow(appInfoViewModel)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            return appInfoWindow.ShowDialog() ?? false;
        }

        public async Task<bool> ShowSettingsDialogAsync()
        {
            var settingsViewModel = new SettingViewModel(_appApiService);
            var settingsWindow = new SettingWindow(settingsViewModel)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            return settingsWindow.ShowDialog() ?? false;
        }

    }
}
