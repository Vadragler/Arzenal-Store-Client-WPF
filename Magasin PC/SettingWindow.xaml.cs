using System.Collections.ObjectModel;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Windows.Controls;
using System.Data;

namespace Magasin_PC
{
    public partial class SettingWindow : Window
    {
        private ObservableCollection<OperatingSystemModel> _operatingSystems { get; set; }
        private ObservableCollection<OSVersionModel> _osVersions;
        private MySqlConnection _connection;
        private OperatingSystemModel? _selectedOperatingSystem;
        private OSVersionModel? _selectedOSVersion;
        private MainWindow _mainWindow;
        private DatabaseManager _databaseManager;

        public SettingWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _connection = _mainWindow.GetConnectionInfos();
            _databaseManager = new DatabaseManager(_connection,_mainWindow);
            Loaded += SettingWindow_Loaded; // Événement pour charger les données
        }

        private async void SettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOperatingSystemsAsync();
            await LoadOSVersionsAsync();
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            bool isConnected = _mainWindow.GetConnectionStatus();
            TextOS.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            TextOSVersion.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            OSList.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            OSVersionsList.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            OSTextBox.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            OSVersionTextBox.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            AddOrUpdateOSButton.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            AddOrUpdateOSVersionButton.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadOperatingSystemsAsync()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                _operatingSystems = new ObservableCollection<OperatingSystemModel>();
                string query = "SELECT * FROM OperatingSystems";
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        _operatingSystems.Add(new OperatingSystemModel
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name")
                        });
                    }
                }
                OSList.ItemsSource = _operatingSystems;
            }
        }

        private async Task LoadOSVersionsAsync()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                _osVersions = new ObservableCollection<OSVersionModel>();
                string query = "SELECT * FROM OSVersions";
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        _osVersions.Add(new OSVersionModel
                        {
                            Id = reader.GetInt32("Id"),
                            Version = reader.GetString("Version"),
                            OperatingSystemId = reader.GetInt32("OSId")
                        });
                    }
                }
                OSVersionsList.ItemsSource = _osVersions;
            }
        }

        private void OSList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OSList.SelectedItem is OperatingSystemModel selectedOS)
            {
                _selectedOperatingSystem = selectedOS;
                OSTextBox.Text = selectedOS.Name;
                CancelButtonOS.Visibility = Visibility.Visible;
                AddOrUpdateOSButton.Content = "Valider";
                AddOrUpdateOSButton.Tag = FindResource("EditIcon");
            }
            else
            {
                ClearOSSelection();
            }
        }

        private void ClearOSSelection()
        {
            OSTextBox.Clear();
            _selectedOperatingSystem = null;
            CancelButtonOS.Visibility = Visibility.Collapsed;
            AddOrUpdateOSButton.Content = "Ajouter";
            AddOrUpdateOSButton.Tag = FindResource("AddIcon");
        }

        private void CancelButtonOS_Click(object sender, RoutedEventArgs e) => ClearOSSelection();

        private void OSVersionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OSVersionsList.SelectedItem is OSVersionModel selectedVersion)
            {
                _selectedOSVersion = selectedVersion;
                OSVersionTextBox.Text = selectedVersion.Version;
                AddOrUpdateOSVersionButton.Content = "Valider";
                AddOrUpdateOSVersionButton.Tag = FindResource("EditIcon");
                CancelButtonVersion.Visibility = Visibility.Visible;
            }
            else
            {
                ClearOSVersionSelection();
            }
        }

        private void ClearOSVersionSelection()
        {
            OSVersionTextBox.Clear();
            _selectedOSVersion = null;
            AddOrUpdateOSVersionButton.Content = "Ajouter";
            AddOrUpdateOSVersionButton.Tag = FindResource("AddIcon");
            CancelButtonVersion.Visibility = Visibility.Collapsed;
        }

        private void CancelButtonVersion_Click(object sender, RoutedEventArgs e) => ClearOSVersionSelection();

        private async void AddOperatingSystem_Click(object sender, RoutedEventArgs e)
        {
            _connection = _mainWindow.GetConnectionInfos();
            _databaseManager = new DatabaseManager(_connection, _mainWindow);
            if (_selectedOperatingSystem != null)
            {
                await _databaseManager.UpdateAsync("OperatingSystems", _selectedOperatingSystem.Id, OSTextBox.Text); // Utiliser la méthode générique pour mettre à jour
                _selectedOperatingSystem.Name = OSTextBox.Text; // Mettre à jour l'élément dans la collection observable
            }
            else
            {
                await _databaseManager.AddAsync("OperatingSystems", OSTextBox.Text); // Utiliser la méthode générique pour ajouter
                _operatingSystems.Add(new OperatingSystemModel { Name = OSTextBox.Text }); // Ajouter à la collection
            }
            OSTextBox.Clear();
            await LoadOperatingSystemsAsync(); // Recharger les systèmes d'exploitation
        }

        private async void AddOSVersion_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOperatingSystem != null)
            {
                _connection = _mainWindow.GetConnectionInfos();
                _databaseManager = new DatabaseManager(_connection, _mainWindow);
                if (_selectedOSVersion != null)
                {
                    await _databaseManager.UpdateAsync("OSVersions", _selectedOSVersion.Id, OSVersionTextBox.Text); // Utiliser la méthode générique pour mettre à jour
                    _selectedOSVersion.Version = OSVersionTextBox.Text; // Mettre à jour l'élément dans la collection observable
                }
                else
                {
                    await _databaseManager.AddAsync("OSVersions", OSVersionTextBox.Text, _selectedOperatingSystem.Id); // Utiliser la méthode générique pour ajouter
                    _osVersions.Add(new OSVersionModel { Version = OSVersionTextBox.Text, OperatingSystemId = _selectedOperatingSystem.Id }); // Ajouter à la collection
                }
                OSVersionTextBox.Clear();
                await LoadOSVersionsAsync(); // Recharger les versions d'OS
            }
        }

        private async void DeleteOperatingSystem_Click(object sender, RoutedEventArgs e)
        {
            _connection = _mainWindow.GetConnectionInfos();
            _databaseManager = new DatabaseManager(_connection, _mainWindow);
            if (sender is Button button && button.DataContext is OperatingSystemModel osToDelete)
            {
                await _databaseManager.DeleteAsync("OperatingSystems", osToDelete.Id); // Utiliser la méthode générique pour supprimer
                _operatingSystems.Remove(osToDelete); // Supprimer de la liste observable
            }
            else
            {
                MessageBox.Show("Erreur lors de la tentative de suppression.");
            }
        }

        private async void DeleteOSVersion_Click(object sender, RoutedEventArgs e)
        {
            _connection = _mainWindow.GetConnectionInfos();
            _databaseManager = new DatabaseManager(_connection, _mainWindow);
            if (sender is Button button && button.DataContext is OSVersionModel versionToDelete)
            {
                _databaseManager = new DatabaseManager(_connection, _mainWindow);
                await _databaseManager.DeleteAsync("OSVersions", versionToDelete.Id); // Utiliser la méthode générique pour supprimer
                _osVersions.Remove(versionToDelete); // Supprimer de la liste observable
            }
            else
            {
                MessageBox.Show("Erreur lors de la tentative de suppression.");
            }
        }
    }
}
