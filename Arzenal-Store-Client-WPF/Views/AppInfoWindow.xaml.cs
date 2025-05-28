using Arzenal.Shared.Dtos.DTOs.AppDto;
using Arzenal.Shared.Dtos.DTOs.CategorieDto;
using Arzenal.Shared.Dtos.DTOs.LanguageDto;
using Arzenal.Shared.Dtos.DTOs.OperatingSystemDto;
using Arzenal.Shared.Dtos.DTOs.TagDto;
using Arzenal_Store_Client_WPF.Services.API;
using Arzenal_Store_Client_WPF.Services.Database;
using Arzenal_Store_Client_WPF.Settings;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;



namespace Arzenal_Store_Client_WPF
{
    /// <summary>
    /// Logique d'interaction pour AppInfoWindow.xaml
    /// </summary>
    public partial class AppInfoWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly Guid? _appId;
        private readonly AppModel _app;

        // Add the missing field for selectedTags  
        private ObservableCollection<TagModel> selectedTags = new ObservableCollection<TagModel>();

        // Add the missing field for selectedLanguages  
        private ObservableCollection<LanguageModel> selectedLanguages = new ObservableCollection<LanguageModel>();

        // Add the missing field for selectedOS  
        private ObservableCollection<OperatingSystemModel> selectedOS = new ObservableCollection<OperatingSystemModel>();

        public AppInfoWindow(ApiService apiService, AppModel? app)
        {
            InitializeComponent();
            _apiService = apiService;
            _app = app;
            _appId = app.Id;

            AppNameTextBox.TextChanged += AppNameTextBox_TextChanged;
            Loaded += AppInfoWindow_Loaded;

        }


        async void AppInfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadListOS();
            await LoadListLanguage();
            await LoadListTags();
            await LoadListCategories();

            if (_appId != Guid.Empty)
            {
                // Fetch the app details and handle the result
                var app = await _apiService.GetAsync<ReadAppDto>($"api/apps/{_appId.Value}");
                if (app != null)
                {
                    // Use the fetched app data as needed
                    AppNameTextBox.Text = app.Name;
                    AppDescriptionTextBox.Text = app.Description;
                    AppVersionTextBox.Text = app.Version;
                    AppIconPathTextBox.Text = app.IconePath;
                    AppFilePathBox.Text = app.FilePath;
                    AppRequirementsTextBox.Text = app.Requirements;
                    AppSizeTextBox.Text = app.AppSize.ToString();
                    AppIsVisibleCheckBox.IsChecked = app.IsVisible;
                    CategorieComboBox.SelectedItem = app.Category;
                    SelectedTagsListBox.ItemsSource = app.Tags;
                    SelectedLanguagesListBox.ItemsSource = app.Languages;
                    SelectedOSListBox.ItemsSource = app.OperatingSystems;
                }
            }
            else
            {
                AppNameTextBox.Text = _app.Name;
                AppVersionTextBox.Text = _app.Version;
                AppFilePathBox.Text = _app.FilePath;
                AppSizeTextBox.Text = _app.AppSize.ToString();
            }
        }

        private async Task LoadListOS()
        {
            // Fetch the list of operating systems from the API
            var operatingsystems = await _apiService.GetAsync<List<ReadOperatingSystemDto>>("api/operatingsystems");

            // Set the ItemsSource of the OSComboBox to the fetched list
            OSComboBox.ItemsSource = operatingsystems;
        }

        private async Task LoadListLanguage()
        {
            var languages = await _apiService.GetAsync<List<ReadLanguageDto>>("api/languages");
            LanguageComboBox.ItemsSource = languages;

        }

        private async Task LoadListTags()
        {
            var tags = await _apiService.GetAsync<List<ReadTagDto>>("api/tags");
            TagComboBox.ItemsSource = tags;

        }

        private async Task LoadListCategories()
        {
            var categories = await _apiService.GetAsync<List<ReadCategorieDto>>("api/categories");
            CategorieComboBox.ItemsSource = categories;

        }

        // Méthode pour charger les données à partir d'un fichier JSON


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



        private void Update(Guid? appId, string initname, string initfilepath)
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
            Update(_appId, AppNameTextBox.Text, AppFilePathBox.Text);
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
            var apps = new UpdateAppDto
            {
                Name = this.AppNameTextBox.Text,
                Description = this.AppDescriptionTextBox.Text,
                Version = this.AppVersionTextBox.Text,
                IconePath = this.AppIconPathTextBox.Text,
                FilePath = this.AppFilePathBox.Text,
                Requirements = this.AppRequirementsTextBox.Text,
                AppSize = long.Parse(this.AppSizeTextBox.Text),
                IsVisible = (bool)AppIsVisibleCheckBox.IsChecked!,
                CategoryId = (int)CategorieComboBox.SelectedValue, // Explicit cast added here
                TagIds = this.selectedTags.Select(tag => tag.Id).ToList(),
                LanguageIds = this.selectedLanguages.Select(lang => lang.Id).ToList(),
                OsIds = this.selectedOS.Select(os => os.Id).ToList()
            };
            var app = await _apiService.PatchAsync<UpdateAppDto>($"api/apps/{_appId}", apps);
            bool success = false;
            if (app != null)
            {
                success = true;
            }
            this.DialogResult = success;
            this.Close();
        }


        // Bouton Ajouter Tag
        private void TagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagComboBox.SelectedItem != null)
            {
                // Cast the selected item to ReadTagDto  
                var selectedTagDto = (ReadTagDto)TagComboBox.SelectedItem;

                // Create a TagModel from the selected ReadTagDto  
                var selectedTag = new TagModel
                {
                    Id = selectedTagDto.Id,
                    Name = selectedTagDto.Name
                };

                // Add the selected tag to the ObservableCollection if it doesn't already exist  
                if (!selectedTags.Any(tag => tag.Id == selectedTag.Id))
                {
                    selectedTags.Add(selectedTag);
                }

                // Update the ItemsSource of the SelectedTagsListBox  
                SelectedTagsListBox.ItemsSource = selectedTags;
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
                var selectedLanguageDto = (ReadLanguageDto)LanguageComboBox.SelectedItem;
                LanguageModel selectedLanguage = new LanguageModel
                {
                    Id = selectedLanguageDto.Id,
                    Name = selectedLanguageDto.Name
                };
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
                var selectedOSDto = (ReadOperatingSystemDto)OSComboBox.SelectedItem;
                OperatingSystemModel? SelectedOS = new OperatingSystemModel
                {
                    Id = selectedOSDto.Id,
                    Name = selectedOSDto.Name
                };
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

        private void SelectedTagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
