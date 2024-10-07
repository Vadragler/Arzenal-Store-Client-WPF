using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.IO;
using Magasin_PC.Settings;
using MySql.Data.MySqlClient;
using Magasin_PC.Settings.Database;
using Magasin_PC.Settings.SFTP;
using System.Data;
using System;
using System.Windows.Media;

namespace Magasin_PC
{
    /// <summary>
    /// Logique d'interaction pour AppInfoWindow.xaml
    /// </summary>
    public partial class AppInfoWindow : Window
    {
        public ObservableCollection<TagModel> AvailableTags { get; set; }
        public ObservableCollection<CategorieModel> AvailableCategories { get; set; }
        public ObservableCollection<LanguageModel> AvailableLanguages { get; set; } // Ajouté
        public ObservableCollection<OperatingSystemModel> AvailableOS { get; set; } // Ajouté
        public ObservableCollection<TagModel> selectedTags { get; set; } = new ObservableCollection<TagModel>();
        public ObservableCollection<LanguageModel> selectedLanguages { get; set; } = new ObservableCollection<LanguageModel>();// Ajouté
        public ObservableCollection<OperatingSystemModel> selectedOS { get; set; } = new ObservableCollection<OperatingSystemModel>(); // Ajouté

        private MainWindow _mainWindow;

        private MySqlConnection _connection;

        private SftpClient _sftpclient;

        public Guid id { get; private set; }
        public string name { get; private set; }
        public string description { get; private set; }
        public string version { get; private set; }
        public bool isvisible { get; private set; }
        public ImageSource icone { get; private set; }
        public string iconepath { get; private set; }
        public string filepath { get; private set; }
        public string requirements { get; private set; }
        public CategorieModel category { get; private set; }
        public DateTime releasedate { get; private set; }
        public DateTime? lastupdated { get; private set; }
        public long appsize { get; private set; }

        public AppInfoWindow(Guid appId,
                             string initname = "",
                             string? initdesc = "",
                             string initvers = "",
                             bool initisvisible = false,
                             string initfilepath = "",
                             ImageSource initicone =  null,
                             string? initiconepath = "",
                             ObservableCollection<TagModel>? inittags = null,
                             ObservableCollection<OperatingSystemModel>? initos = null,
                             ObservableCollection<LanguageModel>? initlanguages = null,
                             string? initrequirements = null,
                             CategorieModel? initcategory = null,
                             DateTime initreleaseDate = default,
                             DateTime? initlastUpdated = null,
                             long initappSize = 0,
                             MainWindow mainwindow = null,
                             SftpClient sftpclient = null)
        {
            InitializeComponent();
            _mainWindow = mainwindow;
            _connection = _mainWindow.GetConnectionInfos();
            _sftpclient = sftpclient;
            this.DataContext = this;

            this.id = appId;
            Update(appId, initname, initfilepath);
            AppNameTextBox.Text = initname;
            AppDescriptionTextBox.Text = initdesc;
            AppVersionTextBox.Text = initvers;
            AppIsVisibleCheckBox.IsChecked = initisvisible;
            AppFilePathBox.Text = initfilepath;
            AppIconImage.Source = initicone;
            releasedate = initreleaseDate;
            lastupdated = initlastUpdated;
            SelectedLanguagesListBox.ItemsSource = initlanguages;
            SelectedOSListBox.ItemsSource = initos;
            SelectedTagsListBox.ItemsSource = inittags;
            CategorieComboBox.SelectedValue = initcategory.Id;
            AppSizeTextBox.Text = initappSize.ToString();
            AppRequirementsTextBox.Text = initrequirements;

            AppNameTextBox.TextChanged += AppNameTextBox_TextChanged;
            Loaded += AppInfoWindow_Loaded;
            
        }

        private async void AppInfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadListOS();
            await LoadListLanguage();
            await LoadListTags();
            await LoadListCategories();

        }

        private async Task LoadListOS()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                AvailableOS = new ObservableCollection<OperatingSystemModel>();
                string query = "SELECT * FROM OperatingSystems";
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AvailableOS.Add(new OperatingSystemModel
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name")
                        });
                    }
                }
                OSComboBox.ItemsSource = AvailableOS;
            }
        }

        private async Task LoadListLanguage()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                AvailableLanguages = new ObservableCollection<LanguageModel>();
                string query = "SELECT * FROM Languages";
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AvailableLanguages.Add(new LanguageModel
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name")
                        });
                    }
                }
                LanguageComboBox.ItemsSource = AvailableLanguages;          
            }
        }

        private async Task LoadListTags()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                AvailableTags = new ObservableCollection<TagModel>();
                string query = "SELECT * FROM Tags";
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AvailableTags.Add(new TagModel
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name")
                        });
                    }
                }
                TagComboBox.ItemsSource = AvailableTags;
            }
        }

        private async Task LoadListCategories()
        {
            if (_mainWindow.GetConnectionStatus())
            {
                _connection = _mainWindow.GetConnectionInfos();
                AvailableCategories = new ObservableCollection<CategorieModel>();
                string query = "SELECT * FROM Categories";
                using (MySqlCommand cmd = new MySqlCommand(query, _connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AvailableCategories.Add(new CategorieModel
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name")
                        });
                    }
                }
                CategorieComboBox.ItemsSource = AvailableCategories;
            }
        }

        // Méthode pour charger les données à partir d'un fichier JSON
        private ObservableCollection<string> LoadDataFromJson(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    var data = JsonConvert.DeserializeObject<ObservableCollection<string>>(jsonData);
                    return data ?? new ObservableCollection<string>();
                }
                else
                {
                    MessageBox.Show($"Le fichier {filePath} n'a pas été trouvé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new ObservableCollection<string>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du fichier {filePath}: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ObservableCollection<string>();
            }
        }

        private void BrowseIcon_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.ico)|*.png;*.jpg;*.jpeg;*.ico";

            if (openFileDialog.ShowDialog() == true)
            {
                string iconPath = openFileDialog.FileName;
                AppIconPathTextBox.Text = iconPath;

                try
                {
                    // Charger l'image sélectionnée sans verrouillage
                    var bitmapSource = new BitmapImage();
                    bitmapSource.BeginInit();
                    bitmapSource.UriSource = new Uri(iconPath, UriKind.Absolute);
                    bitmapSource.CacheOption = BitmapCacheOption.OnLoad; // Charger l'image en mémoire
                    bitmapSource.EndInit();
                    bitmapSource.Freeze(); // Geler pour libérer la référence
                    AppIconImage.Source = bitmapSource;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Erreur lors du chargement de l'icône: {ex.Message}");
                }
            }
        }



        private void Update(Guid appId, string initname, string initfilepath)
        {
            var apps = (Application.Current.MainWindow.DataContext as MainViewModel)?.Apps;
            var existingFilepathApp = apps?.FirstOrDefault(app => app.FilePath == initfilepath && app.Id != appId);
            var existingNameApp = apps?.FirstOrDefault(app => app.Name == initname && app.Id != appId);

            WarningTextName.Visibility = Visibility.Collapsed;
            if (existingFilepathApp != null)
            {
                WarningTextFilePath.Visibility = Visibility.Visible;
            }
            else
            {
                WarningTextFilePath.Visibility = Visibility.Collapsed;
            }
            if (existingNameApp != null)
            {
                WarningTextName.Visibility = Visibility.Visible;
            }
            else
            {
                WarningTextName.Visibility = Visibility.Collapsed;
            }
        }

        private void AppNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Update(this.id, AppNameTextBox.Text, AppFilePathBox.Text);
        }

        private void ModifyFilePath_Click(object sender, RoutedEventArgs e)
        {
            FileService fileService = new FileService();
            string? filePath = fileService.SelectFile();  // Ouvre une boîte de dialogue pour sélectionner un fichier ZIP
            if (filePath != null)
            {
                this.AppFilePathBox.Text = filePath;
                var test = (this.DataContext as MainViewModel)?.Apps.FirstOrDefault(app => app.FilePath == filePath);

                if (test != null)
                {
                    WarningTextFilePath.Visibility = Visibility.Visible;
                }
                else
                {
                    WarningTextFilePath.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            id = this.id;
            name = this.AppNameTextBox.Text;
            description = this.AppDescriptionTextBox.Text;
            version = this.AppVersionTextBox.Text;
            isvisible = (bool)AppIsVisibleCheckBox.IsChecked!;
            iconepath = this.AppIconPathTextBox.Text;
            filepath = this.AppFilePathBox.Text;
            requirements = this.AppRequirementsTextBox.Text;
            category = (CategorieModel)CategorieComboBox.SelectedItem;
            if (releasedate == default(DateTime))
            {
                releasedate = DateTime.Now;
            }
            lastupdated = DateTime.Now;

            if (long.TryParse(AppSizeTextBox.Text, out long appSize))
            {
                appsize = appSize;
            }
            else
            {
                MessageBox.Show("Veuillez entrer une taille valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DatabaseManager dbManager = new DatabaseManager(_mainWindow.GetConnectionInfos(), _mainWindow);  // Utiliser la fenêtre existante
            await dbManager.AddOrUpdateAppAsync(id, name, description, version, filepath, isvisible, iconepath, requirements, category.Id, releasedate, lastupdated, appsize, selectedOS, selectedLanguages, selectedTags,_sftpclient);
            //Task.Run(() => dbManager.AddOrUpdateAppAsync(id, name, description, version, filepath, isvisible, icone, requirements, category.Id, releasedate, lastupdated, appsize, selectedOS, selectedLanguages, selectedTags)).Wait();
            this.DialogResult = true;
            this.Close();
        }


        // Bouton Ajouter Tag
        private void TagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagComboBox.SelectedItem != null)
            {
                TagModel selectedTag = (TagModel)TagComboBox.SelectedItem;
                if (!selectedTags.Contains(selectedTag))
                {
                    selectedTags.Add(selectedTag);
                    SelectedTagsListBox.ItemsSource = selectedTags;
                }
            }
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            TagModel? tagToRemove = (sender as Button)?.Tag as TagModel;
            if (tagToRemove != null && selectedTags.Contains(tagToRemove))
            {
                selectedTags.Remove(tagToRemove);
            }
        }

        // Méthodes pour gérer les langues
        private void LanguagesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem != null)
            {
                LanguageModel selectedLanguage = (LanguageModel)LanguageComboBox.SelectedItem;
                if (!selectedLanguages.Contains(selectedLanguage))
                {
                    selectedLanguages.Add(selectedLanguage);
                    SelectedLanguagesListBox.ItemsSource = selectedLanguages;
                }
            }
        }

        private void RemoveLanguage_Click(object sender, RoutedEventArgs e)
        {
            LanguageModel? languageToRemove = (sender as Button)?.Tag as LanguageModel;
            if (languageToRemove != null && selectedLanguages.Contains(languageToRemove))
            {
                selectedLanguages.Remove(languageToRemove);
            }
        }

        // Méthodes pour gérer les systèmes d'exploitation
        private void OSComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OSComboBox.SelectedItem != null)
            {
                OperatingSystemModel? SelectedOS = (OperatingSystemModel)OSComboBox.SelectedItem;
                if (!selectedOS.Contains(SelectedOS))
                {
                    selectedOS.Add(SelectedOS);
                    SelectedOSListBox.ItemsSource = selectedOS;
                }
            }
        }

        private void RemoveOS_Click(object sender, RoutedEventArgs e)
        {
            OperatingSystemModel? osToRemove = (sender as Button)?.Tag as OperatingSystemModel;
            if (osToRemove != null && selectedOS.Contains(osToRemove))
            {
                selectedOS.Remove(osToRemove);
            }
        }
    }
}
