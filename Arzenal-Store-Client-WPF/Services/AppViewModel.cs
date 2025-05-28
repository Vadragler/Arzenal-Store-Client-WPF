using Arzenal_Store_Client_WPF.Settings;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Arzenal_Store_Client_WPF.Services
{
    public class AppViewModel : INotifyPropertyChanged
    {
        private MySqlConnection _dbconnection; // Connexion à la base de données
        private ObservableCollection<AppModel> _apps; // Liste des applications
        private AppModel _selectedApp; // Application sélectionnée


        // Commandes
        public ICommand ModifyAppCommand { get; set; }
        public ICommand RemoveAppCommand { get; set; }
        public ICommand BackToListCommand { get; set; }
        public ICommand AppListMouseUpCommand { get; set; }
        public ICommand DeselectAppCommand { get; set; }

        // Propriétés liées à l'interface utilisateur
        private bool _isDetailsVisible;
        private bool _isAddAppButtonVisible;

        public bool IsDetailsVisible
        {
            get => _isDetailsVisible;
            set
            {
                if (_isDetailsVisible != value)
                {
                    _isDetailsVisible = value;
                    OnPropertyChanged(nameof(IsDetailsVisible));
                }
            }
        }

        public bool IsAddAppButtonVisible
        {
            get => _isAddAppButtonVisible;
            set
            {
                if (_isAddAppButtonVisible != value)
                {
                    _isAddAppButtonVisible = value;
                    OnPropertyChanged(nameof(IsAddAppButtonVisible));
                }
            }
        }




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

        // Constructeur
        public AppViewModel(MainViewModel viewModel, MySqlConnection dbConnection)
        {
            _dbconnection = dbConnection;
            // Initialiser les commandes

            //ModifyAppCommand = new RelayCommand(ModifyApp);
            //RemoveAppCommand = new RelayCommand(RemoveApp);
            //BackToListCommand = new RelayCommand(BackToList);
            //AppListMouseUpCommand = new RelayCommand(BackToList); // Commande pour fermer les détails
            //DeselectAppCommand = new RelayCommand(DeselectApp); // Commande pour désélectionner une application
        }

        // Méthode pour charger les applications


        // Méthode pour modifier une application
        private async void ModifyApp(object parameter)
        {
            if (parameter is AppModel appToModify)
            {
                /*var appInfoWindow = new AppInfoWindow(_apiService, appToModify.Id);
                appInfoWindow.Owner = GetOwnerWindow();
                appInfoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = appInfoWindow.ShowDialog();
                if (result == true)
                {
                    // Mise à jour de l'application après modification
                    UpdateAppSelected(appToModify, appInfoWindow);

                    
                }*/
            }
        }

        // Méthode pour obtenir la fenêtre parente
        private Window GetOwnerWindow()
        {
            return Application.Current.MainWindow;
        }

        // Méthode pour mettre à jour l'application sélectionnée après modification
        private void UpdateAppSelected(AppModel appSelected, AppInfoWindow appInfoWindow)
        {
            /*  appSelected.Name = appInfoWindow.name;
              appSelected.Description = appInfoWindow.description;
              // Mettre à jour les autres propriétés de l'application...*/
        }

        // Méthode pour supprimer une application
        private void RemoveApp(object parameter)
        {
            if (parameter is AppModel appToRemove)
            {


            }
        }

        // Méthode pour revenir à la liste des applications
        private void BackToList(object parameter)
        {
            SelectedApp = null;
            IsDetailsVisible = false;
            IsAddAppButtonVisible = true;
        }

        // Méthode pour désélectionner l'application
        private void DeselectApp(object parameter)
        {
            SelectedApp = null; // Désélectionner l'élément
        }

        // Gestion de l'événement PropertyChanged pour la notification des changements
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
