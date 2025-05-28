using Arzenal.Shared.Dtos.DTOs.CategorieDto;
using Arzenal.Shared.Dtos.DTOs.LanguageDto;
using Arzenal.Shared.Dtos.DTOs.OperatingSystemDto;
using Arzenal.Shared.Dtos.DTOs.TagDto;
using Arzenal_Store_Client_WPF.Models;
using Arzenal_Store_Client_WPF.Services.API;
using Arzenal_Store_Client_WPF.Services.Database;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Arzenal_Store_Client_WPF
{
    public partial class SettingWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ObservableCollection<OperatingSystemModel> _operatingSystems { get; set; }
        private ObservableCollection<LanguageModel> _languages; // Nouvelle collection pour les langues
        private ObservableCollection<TagModel> _tags; // Nouvelle collection pour les tags
        private ObservableCollection<CategorieModel> _categories;
        private ApiService _apiService;

        public SettingWindow(ApiService apiservice)
        {
            _apiService = apiservice;

            InitializeComponent();
            //_databaseManager = new DatabaseManager(_connection);
            Loaded += SettingWindow_Loaded; // Événement pour charger les données
        }

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            //await LoadAsyncLists();
            TogglePanels(true);
        }

        private async void SettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAsyncLists();
        }

        private async Task LoadAsyncLists()
        {
            var operatingSystemDtos = await _apiService.GetAsync<List<ReadOperatingSystemDto>>("api/operatingsystems");
            _operatingSystems = [.. operatingSystemDtos.Select(dto => new OperatingSystemModel
                {
                    Id = dto.Id,
                    Name = dto.Name
                })];
            var languageDtos = await _apiService.GetAsync<List<ReadLanguageDto>>("api/languages");
            _languages = [.. languageDtos.Select(dto => new LanguageModel
            {
                Id = dto.Id,
                Name = dto.Name
            })];

            var tagDtos = await _apiService.GetAsync<List<ReadTagDto>>("api/tags");
            _tags = [.. tagDtos.Select(dto => new TagModel
            {
                Id = dto.Id,
                Name = dto.Name
            })];

            var categorieDtos = await _apiService.GetAsync<List<ReadCategorieDto>>("api/categories");
            _categories = [.. categorieDtos.Select(dto => new CategorieModel
            {
                Id = dto.Id,
                Name = dto.Name
            })];
        }

        private void ShowOSSettings(object sender, RoutedEventArgs e) => ShowSettings("Systèmes d'exploitation", _operatingSystems);
        private void ShowLanguageSettings(object sender, RoutedEventArgs e) => ShowSettings("Langues", _languages);
        private void ShowTagSettings(object sender, RoutedEventArgs e) => ShowSettings("Tags", _tags);
        private void ShowCategorieSettings(object sender, RoutedEventArgs e) => ShowSettings("Categories", _categories);


        private void ShowSettings<T>(string title, ObservableCollection<T> collection) where T : BaseModel
        {
            TogglePanels(false);
            SettingsTitle.Text = title;
            SettingsList.ItemsSource = collection;
            SettingsTextBox.Text = string.Empty;

        }

        private void TogglePanels(bool showMenu)
        {
            MenuSettingsPanel.Visibility = showMenu ? Visibility.Visible : Visibility.Collapsed;
            SettingPanel.Visibility = showMenu ? Visibility.Collapsed : Visibility.Visible;
            BackButton.Visibility = showMenu ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SettingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingsList.SelectedItem is BaseModel selectedItem)
            {
                SettingsTextBox.Text = selectedItem.Name;
                CancelButton.Visibility = Visibility.Visible;
                AddOrUpdateButton.Content = "Valider";
                AddOrUpdateButton.Tag = FindResource("EditIcon");
            }
            else
            {
                ClearSelection();
            }
        }

        private void SettingsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Corrected the logic to fix the errors
            CancelButton.Visibility = !string.IsNullOrWhiteSpace(SettingsTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            AddOrUpdateButton.IsEnabled = !string.IsNullOrWhiteSpace(SettingsTextBox.Text);
        }

        private void ClearSelection()
        {
            SettingsTextBox.Clear();
            CancelButton.Visibility = Visibility.Collapsed;
            AddOrUpdateButton.Content = "Ajouter";
            AddOrUpdateButton.Tag = FindResource("AddIcon");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => ClearSelection();






        private async void AddOrUpdateItem_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer le bouton qui a été cliqué  
            var button = sender as Button;
            // Vérifier si le bouton et son DataContext ne sont pas null  
            if (button?.DataContext is BaseModel selectedItem)
            {
                string? route = null;
                var title = SettingsTitle.Text;
                switch (title)
                {
                    case "Systèmes d'exploitation":
                        route = $"api/operatingsystems/{selectedItem.Id}";
                        var selectedOS = new UpdateOperatingSystemDto
                        {
                            Name = SettingsTextBox.Text
                        };
                        await _apiService.PutAsync<UpdateOperatingSystemDto>(route, selectedOS);
                        break;

                    case "Langues":
                        route = $"api/languages/{selectedItem.Id}";
                        var selectedLanguage = new UpdateLanguageDto
                        {
                            Name = SettingsTextBox.Text
                        };
                        await _apiService.PutAsync<UpdateLanguageDto>(route, selectedLanguage);
                        break;

                    case "Tags":
                        route = $"api/tags/{selectedItem.Id}";
                        var selectedTag = new UpdateTagDto
                        {
                            Name = SettingsTextBox.Text
                        };
                        await _apiService.PutAsync<UpdateTagDto>(route, selectedTag);
                        break;

                    case "Categories":
                        route = $"api/categories/{selectedItem.Id}";
                        var selectedCategorie = new UpdateCategorieDto
                        {
                            Name = SettingsTextBox.Text
                        };
                        await _apiService.PutAsync<UpdateCategorieDto>(route, selectedCategorie);
                        break;
                }
            }
            else
            {                 // Si le bouton n'est pas cliqué, on ajoute un nouvel élément
                string? route = null;
                var title = SettingsTitle.Text;
                switch (title)
                {
                    case "Systèmes d'exploitation":
                        route = "api/operatingsystems";
                        var operatingsystem = new CreateOperatingSystemDto
                        {
                            Name = SettingsTextBox.Text
                        };
                        await _apiService.PostAsync<CreateOperatingSystemDto>(route, operatingsystem);
                        break;
                    case "Langues":
                        route = "api/languages";
                        var language = new CreateLanguageDto
                        {
                            Name = SettingsTextBox.Text
                        };
                        await _apiService.PostAsync<CreateLanguageDto>(route, language);
                        break;
                    case "Tags":
                        route = "api/tags";
                        var tag = new CreateTagDto
                        {
                            Name = SettingsTextBox.Text
                        };
                        await _apiService.PostAsync<CreateTagDto>(route, tag);
                        break;
                    case "Categories":
                        route = "api/categories";
                        var categorie = new CreateCategorieDto
                        {
                            Name = SettingsTextBox.Text
                        };
                        await _apiService.PostAsync<CreateCategorieDto>(route, categorie);
                        break;
                }

            }
            await LoadAsyncLists();
        }
        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer le bouton qui a été cliqué  
            var button = sender as Button;

            // Vérifier si le bouton et son DataContext ne sont pas null  
            if (button?.DataContext is BaseModel selectedItem)
            {
                string? route = null; // Remplacement de "var" par "string?" pour corriger l'erreur CS0815  

                var title = SettingsTitle.Text;
                switch (title)
                {
                    case "Systèmes d'exploitation":
                        route = "api/operatingsystems";
                        break;
                    case "Langues":
                        route = "api/languages";
                        break;
                    case "Tags":
                        route = "api/tags";
                        break;
                    case "Categories":
                        route = "api/categories";
                        break;
                }

                // Appeler la méthode pour gérer la suppression  
                if (route != null)
                {
                    await _apiService.DeleteAsync($"{route}/{selectedItem.Id}");
                }
            }
        }


    }
}
