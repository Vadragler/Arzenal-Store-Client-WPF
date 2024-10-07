using System.Collections.ObjectModel;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Windows.Controls;
using System.Data;
using Magasin_PC.Settings.Database;

namespace Magasin_PC
{
    public partial class SettingWindow : Window
    {
        private ObservableCollection<OperatingSystemModel> _operatingSystems { get; set; }
        private ObservableCollection<LanguageModel> _languages; // Nouvelle collection pour les langues
        private ObservableCollection<TagModel> _tags; // Nouvelle collection pour les tags
        private ObservableCollection<CategorieModel> _categories;
        private MySqlConnection _connection;
        private OperatingSystemModel? _selectedOperatingSystem;
        private LanguageModel? _selectedLanguage;
        private TagModel? _selectedTag;
        private CategorieModel? _selectedCategorie;
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MenuSettingsPanel.Visibility = Visibility.Visible;
            SettingPanel.Visibility = Visibility.Collapsed;
        }

        private async void SettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOperatingSystemsAsync(); // Charger les système d'explitation
            await LoadLanguagesAsync(); // Charger les langues
            await LoadTagsAsync(); // Charger les tags
            await LoadCategoriesAsync(); // Charger les catégories
            UpdateVisibility();
        }

        private void ShowOSSettings(object sender, RoutedEventArgs e)
        {
            MenuSettingsPanel.Visibility = Visibility.Collapsed;
            SettingPanel.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
            SettingsTitle.Text = "Systèmes d'exploitation";
            SettingsList.ItemsSource = _operatingSystems; // Charger la liste des OS
            SettingsTextBox.Text = string.Empty;
        }

        private void ShowLanguageSettings(object sender, RoutedEventArgs e)
        {
            MenuSettingsPanel.Visibility = Visibility.Collapsed;
            SettingPanel.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
            SettingsTitle.Text = "Langues";
            SettingsList.ItemsSource = _languages; // Charger la liste des Langues
            SettingsTextBox.Text = string.Empty;
        }

        private void ShowTagSettings(object sender, RoutedEventArgs e)
        {
            MenuSettingsPanel.Visibility = Visibility.Collapsed;
            SettingPanel.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
            SettingsTitle.Text = "Tags";
            SettingsList.ItemsSource = _tags; // Charger la liste des Tags
            SettingsTextBox.Text = string.Empty;
        }

        private void ShowCategorieSettings(object sender, RoutedEventArgs e)
        {
            MenuSettingsPanel.Visibility = Visibility.Collapsed;
            SettingPanel.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
            SettingsTitle.Text = "Categories";
            SettingsList.ItemsSource = _categories; // Charger la liste des Categories
            SettingsTextBox.Text = string.Empty;
        }


        private void UpdateVisibility()
        {
            bool isConnected = _mainWindow.GetConnectionStatus();
            if (isConnected) 
            {
                MenuSettingsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                MenuSettingsPanel.Visibility = Visibility.Collapsed;
            }
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
            }
        }

        private async Task LoadLanguagesAsync()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                _languages = new ObservableCollection<LanguageModel>();
                string query = "SELECT * FROM Languages"; // Suppose que tu as une table 'Languages'
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        _languages.Add(new LanguageModel
                        {   
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name") 
                        });
                    }
                }
            }
        }

        private async Task LoadTagsAsync()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                _tags = new ObservableCollection<TagModel>();
                string query = "SELECT * FROM Tags"; // Suppose que tu as une table 'Tags'
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        _tags.Add(new TagModel
                        {
                            Id = reader.GetInt32("Id"),
                            Name =  reader.GetString("Name")
                        });
                    }
                }
            }
        }

        private async Task LoadCategoriesAsync()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                _categories = new ObservableCollection<CategorieModel>();
                string query = "SELECT * FROM Categories"; // Suppose que tu as une table 'Tags'
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        _categories.Add(new CategorieModel
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name")
                        });
                    }
                }
            }
        }

        private void SettingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (SettingsList.SelectedItem is OperatingSystemModel selectedOS)
            {
                _selectedOperatingSystem = selectedOS;
                SettingsTextBox.Text = selectedOS.Name;
                CancelButton.Visibility = Visibility.Visible;
                AddOrUpdateButton.Content = "Valider";
                AddOrUpdateButton.Tag = FindResource("EditIcon");
            }
            else if (SettingsList.SelectedItem is LanguageModel selectedLanguage)
            {
                _selectedLanguage = selectedLanguage;
                SettingsTextBox.Text = selectedLanguage.Name;
                CancelButton.Visibility = Visibility.Visible;
                AddOrUpdateButton.Content = "Valider";
                AddOrUpdateButton.Tag = FindResource("EditIcon");
            }
            else if (SettingsList.SelectedItem is TagModel selectedTag)
            {
                _selectedTag = selectedTag;
                SettingsTextBox.Text = selectedTag.Name;
                CancelButton.Visibility = Visibility.Visible;
                AddOrUpdateButton.Content = "Valider";
                AddOrUpdateButton.Tag = FindResource("EditIcon");
            }
            else if (SettingsList.SelectedItem is CategorieModel selectedCategorie)
            {
                _selectedCategorie = selectedCategorie;
                SettingsTextBox.Text = selectedCategorie.Name;
                CancelButton.Visibility = Visibility.Visible;
                AddOrUpdateButton.Content = "Valider";
                AddOrUpdateButton.Tag = FindResource("EditIcon");
            }
            else
            {
                ClearSelection();
            } 
            
        }

        private void ClearSelection()
        {
            SettingsTextBox.Clear();
            _selectedOperatingSystem = null;
            _selectedTag = null;
            _selectedLanguage = null;
            _selectedCategorie = null;
            CancelButton.Visibility = Visibility.Collapsed;
            AddOrUpdateButton.Content = "Ajouter";
            AddOrUpdateButton.Tag = FindResource("AddIcon");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => ClearSelection();

        private async void AddOrUpdateItem_Click(object sender, RoutedEventArgs e)
        {
            _connection = _mainWindow.GetConnectionInfos();
            _databaseManager = new DatabaseManager(_connection, _mainWindow);
            switch (SettingsTitle.Text)
            {
                case "Systèmes d'exploitation":
                    if (_selectedOperatingSystem != null)
                    {
                        await _databaseManager.UpdateAsync("OperatingSystems", _selectedOperatingSystem.Id, SettingsTextBox.Text); // Utiliser la méthode générique pour mettre à jour
                        _selectedOperatingSystem.Name = SettingsTextBox.Text; // Mettre à jour l'élément dans la collection observable
                    }
                    else
                    {
                        await _databaseManager.AddAsync("OperatingSystems", SettingsTextBox.Text); // Utiliser la méthode générique pour ajouter
                        _operatingSystems.Add(new OperatingSystemModel { Name = SettingsTextBox.Text }); // Ajouter à la collection
                    }
                    SettingsTextBox.Clear();
                    await LoadOperatingSystemsAsync(); // Recharger les systèmes d'exploitation

                    break;
                case "Langues":
                    if (_selectedLanguage != null)
                    {
                        await _databaseManager.UpdateAsync("Languages", _selectedLanguage.Id, SettingsTextBox.Text); // Utiliser la méthode générique pour mettre à jour
                        _selectedLanguage.Name = SettingsTextBox.Text; // Mettre à jour l'élément dans la collection observable
                    }
                    else
                    {
                        await _databaseManager.AddAsync("Languages", SettingsTextBox.Text); // Utiliser la méthode générique pour ajouter
                        _languages.Add(new LanguageModel { Name = SettingsTextBox.Text }); // Ajouter à la collection
                    }
                    SettingsTextBox.Clear();
                    await LoadLanguagesAsync(); // Recharger les langues
                    break;
                case "Tags":
                    if (_selectedTag != null)
                    {
                        await _databaseManager.UpdateAsync("Tags", _selectedTag.Id, SettingsTextBox.Text); // Utiliser la méthode générique pour mettre à jour
                        _selectedTag.Name = SettingsTextBox.Text; // Mettre à jour l'élément dans la collection observable
                    }
                    else
                    {
                        await _databaseManager.AddAsync("Tags", SettingsTextBox.Text); // Utiliser la méthode générique pour ajouter
                        _tags.Add(new TagModel { Name = SettingsTextBox.Text }); // Ajouter à la collection
                    }
                    SettingsTextBox.Clear();
                    await LoadTagsAsync(); // Recharger les tags
                    break;
                case "Categories":
                    if (_selectedCategorie != null)
                    {
                        await _databaseManager.UpdateAsync("Categories", _selectedCategorie.Id, SettingsTextBox.Text); // Utiliser la méthode générique pour mettre à jour
                        _selectedCategorie.Name = SettingsTextBox.Text; // Mettre à jour l'élément dans la collection observable
                    }
                    else
                    {
                        await _databaseManager.AddAsync("Categories", SettingsTextBox.Text); // Utiliser la méthode générique pour ajouter
                        _categories.Add(new CategorieModel { Name = SettingsTextBox.Text }); // Ajouter à la collection
                    }
                    SettingsTextBox.Clear();
                    await LoadCategoriesAsync(); // Recharger les catégories
                    break;
            }
        }



        // Ajoute une langue


        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            _connection = _mainWindow.GetConnectionInfos();
            _databaseManager = new DatabaseManager(_connection, _mainWindow);
            switch (SettingsTitle.Text)
            {
                case "Systèmes d'exploitation":
                    if (sender is Button buttonOS && buttonOS.DataContext is OperatingSystemModel OSToDelete)
                    {
                        await _databaseManager.DeleteAsync("OperatingSystems", OSToDelete.Id); // Utiliser la méthode générique pour supprimer
                        _operatingSystems.Remove(OSToDelete); // Supprimer de la liste observable
                        await LoadOperatingSystemsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la tentative de suppression.");
                    }
                    break;
                case "Langues":
                    if (sender is Button buttonLanguage && buttonLanguage.DataContext is LanguageModel LanguageToDelete)
                    {
                        await _databaseManager.DeleteAsync("Languages", LanguageToDelete.Id); // Utiliser la méthode générique pour supprimer
                        _languages.Remove(LanguageToDelete); // Supprimer de la liste observable
                        await LoadLanguagesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la tentative de suppression.");
                    }
                    break;
                case "Tags":
                    if (sender is Button buttonTag && buttonTag.DataContext is TagModel TagToDelete)
                    {
                        await _databaseManager.DeleteAsync("Tags", TagToDelete.Id); // Utiliser la méthode générique pour supprimer
                        _tags.Remove(TagToDelete); // Supprimer de la liste observable
                        await LoadTagsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la tentative de suppression.");
                    }
                    break;
                case "Categories":
                    if (sender is Button buttonCategorie && buttonCategorie.DataContext is CategorieModel CategorieToDelete)
                    {
                        await _databaseManager.DeleteAsync("Categories", CategorieToDelete.Id); // Utiliser la méthode générique pour supprimer
                        _categories.Remove(CategorieToDelete); // Supprimer de la liste observable
                        await LoadTagsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la tentative de suppression.");
                    }
                    break;
            }
        }
    }
}
