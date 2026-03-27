using Arzenal.Dto.DTOs.CategorieDto;
using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.Dto.DTOs.TagDto;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Models;
using Arzenal.StoreManager.Data.Database;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Arzenal.StoreManager.WPF.ViewModel
{
    public partial class SettingViewModel : ObservableObject
    {
        private readonly IAppApiService _apiService;

        [ObservableProperty]
        private bool isMenuSettingsVisible = true;

        [ObservableProperty]
        private string currentType = "OS";

        [ObservableProperty]
        private BaseModel? selectedItem;

        [ObservableProperty]
        private string inputText = "";

        [ObservableProperty]
        private ObservableCollection<BaseModel>? currentCollection = new();

        [ObservableProperty]
        private ObservableCollection<OperatingSystemModel> operatingSystems = new();
        [ObservableProperty]
        private ObservableCollection<LanguageModel> languages = new();
        [ObservableProperty]
        private ObservableCollection<TagModel> tags = new();
        [ObservableProperty]
        private ObservableCollection<CategorieModel> categories = new();

        public IRelayCommand BackCommand { get; }
        public IRelayCommand ShowOSSettingCommand { get; }
        public IRelayCommand ShowLanguageSettingsCommand { get; }
        public IRelayCommand ShowTagSettingCommand { get; }
        public IRelayCommand ShowCategorieSettingCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IRelayCommand AddOrUpdateCommand { get; }
        public IRelayCommand<BaseModel> DeleteItemCommand { get; }

        public SettingViewModel(IAppApiService apiService)
        {
            _apiService = apiService;

            BackCommand = new RelayCommand(() => IsMenuSettingsVisible = true);
            ShowOSSettingCommand = new RelayCommand(() => ShowSettings("Systèmes d'exploitation"));
            ShowLanguageSettingsCommand = new RelayCommand(() => ShowSettings("Langues"));
            ShowTagSettingCommand = new RelayCommand(() => ShowSettings("Tags"));
            ShowCategorieSettingCommand = new RelayCommand(() => ShowSettings("Catégories"));

            AddOrUpdateCommand = new RelayCommand(async () => await AddOrUpdateItem() /*() => !string.IsNullOrWhiteSpace(InputText)*/);
            DeleteItemCommand = new RelayCommand<BaseModel>(async item => await DeleteItem(item));
            CancelCommand = new RelayCommand(ClearSelection);

            _ = LoadAsyncLists();
        }

        private void ShowSettings(string title)
        {
            CurrentType = title;
            IsMenuSettingsVisible = false;

            CurrentCollection = CurrentType switch
            {
                "Systèmes d'exploitation" => new ObservableCollection<BaseModel>(OperatingSystems.Cast<BaseModel>()),
                "Langues" => new ObservableCollection<BaseModel>(Languages.Cast<BaseModel>()),
                "Tags" => new ObservableCollection<BaseModel>(Tags.Cast<BaseModel>()),
                "Catégories" => new ObservableCollection<BaseModel>(Categories.Cast<BaseModel>()),
                _ => new ObservableCollection<BaseModel>()
            };

            SelectedItem = null; // Ou CurrentCollection.FirstOrDefault() si tu veux sélectionner le premier
        }

        partial void OnSelectedItemChanged(BaseModel? value)
        {
            if (value != null)
            {
                InputText = value.Name; // Mettre le nom dans le TextBox
            }
            else
            {
                InputText = "";
            }
        }


        private async Task LoadAsyncLists()
        {
            var osDtos = await _apiService.GetAsync<List<ReadOperatingSystemDto>>("api/operatingsystems");
            if (osDtos != null)
            {
                OperatingSystems = new ObservableCollection<OperatingSystemModel>(osDtos.Select(dto => new OperatingSystemModel { Id = dto.Id, Name = dto.Name }));
            }

            var langDtos = await _apiService.GetAsync<List<ReadLanguageDto>>("api/languages");
            if (langDtos != null)
            {
                Languages = new ObservableCollection<LanguageModel>(langDtos.Select(dto => new LanguageModel { Id = dto.Id, Name = dto.Name }));
            }

            var tagDtos = await _apiService.GetAsync<List<ReadTagDto>>("api/tags");
            if (tagDtos != null)
            {
                Tags = new ObservableCollection<TagModel>(tagDtos.Select(dto => new TagModel { Id = dto.Id, Name = dto.Name }));
            }

            var catDtos = await _apiService.GetAsync<List<ReadCategorieDto>>("api/categories");
            if (catDtos != null)
            {
                Categories = new ObservableCollection<CategorieModel>(catDtos.Select(dto => new CategorieModel { Id = dto.Id, Name = dto.Name }));
            }

            // Mettre à jour CurrentCollection pour que le ListBox reflète la bonne liste
            CurrentCollection = CurrentType switch
            {
                "Systèmes d'exploitation" => new ObservableCollection<BaseModel>(OperatingSystems.Cast<BaseModel>()),
                "Langues" => new ObservableCollection<BaseModel>(Languages.Cast<BaseModel>()),
                "Tags" => new ObservableCollection<BaseModel>(Tags.Cast<BaseModel>()),
                "Catégories" => new ObservableCollection<BaseModel>(Categories.Cast<BaseModel>()),
                _ => new ObservableCollection<BaseModel>()
            };

            SelectedItem = null; // ou CurrentCollection.FirstOrDefault() si tu veux sélectionner le premier
        }


        private async Task AddOrUpdateItem()
        {
            if (SelectedItem != null)
            {
                // Update
                switch (SelectedItem)
                {
                    case OperatingSystemModel os:
                        await _apiService.PutAsync<UpdateOperatingSystemDto>($"api/operatingsystems/{os.Id}", new { Name = InputText });
                        os.Name = InputText; // met à jour le nom local

                        break;
                    case LanguageModel lang:
                        await _apiService.PutAsync<UpdateLanguageDto>($"api/languages/{lang.Id}", new { Name = InputText });
                        lang.Name = InputText;
                        break;
                    case TagModel tag:
                        await _apiService.PutAsync<UpdateTagDto>($"api/tags/{tag.Id}", new { Name = InputText });
                        tag.Name = InputText;
                        break;
                    case CategorieModel cat:
                        await _apiService.PutAsync<UpdateCategorieDto>($"api/categories/{cat.Id}", new { Name = InputText });
                        cat.Name = InputText;
                        break;
                }
            }
            else
            {
                // Create
                switch (CurrentType)
                {
                    case "Systèmes d'exploitation":
                        var osResult = await _apiService.PostAsync<ReadOperatingSystemDto>("api/operatingsystems", new { Name = InputText });
                        if (osResult != null && osResult.Name != null)
                        {
                            var osModel = new OperatingSystemModel { Id = osResult.Id, Name = osResult.Name };
                            OperatingSystems.Add(osModel);
                            if (CurrentCollection != null)
                                CurrentCollection.Add(osModel);
                        }
                        break;
                    case "Langues":
                        var langResult = await _apiService.PostAsync<ReadLanguageDto>("api/languages", new { Name = InputText });
                        if (langResult != null && langResult.Name != null)
                        {
                            var langModel = new LanguageModel { Id = langResult.Id, Name = langResult.Name };
                            Languages.Add(langModel);
                            if (CurrentCollection != null)
                                CurrentCollection.Add(langModel);
                        }
                        break;
                    case "Tags":
                        var tagResult = await _apiService.PostAsync<ReadTagDto>("api/tags", new { Name = InputText });
                        if (tagResult != null && tagResult.Name != null)
                        {
                            var tagModel = new TagModel { Id = tagResult.Id, Name = tagResult.Name };
                            Tags.Add(tagModel);
                            if (CurrentCollection != null)
                                CurrentCollection.Add(tagModel);
                        }
                        break;
                    case "Catégories":
                        var catResult = await _apiService.PostAsync<ReadCategorieDto>("api/categories", new { Name = InputText });
                        if (catResult != null && catResult.Name != null)
                        {
                            var catModel = new CategorieModel { Id = catResult.Id, Name = catResult.Name };
                            Categories.Add(catModel);
                            if (CurrentCollection != null)
                                CurrentCollection.Add(catModel);
                            
                        }
                        break;
                }
            }

            ClearSelection();
        }


        private async Task DeleteItem(BaseModel item)
        {
            string? route = item switch
            {
                OperatingSystemModel => $"api/operatingsystems/{item.Id}",
                LanguageModel => $"api/languages/{item.Id}",
                TagModel => $"api/tags/{item.Id}",
                CategorieModel => $"api/categories/{item.Id}",
                _ => null
            };

            if (!string.IsNullOrEmpty(route))
            {
                bool success = await _apiService.DeleteAsync(route);
                if (success && CurrentCollection != null)
                {
                    CurrentCollection.Remove(item); // Met à jour instantanément l'UI
                    switch (CurrentType)
                    {
                        case "Systèmes d'exploitation":
                            OperatingSystems.Remove((OperatingSystemModel)item);
                            break;
                        case "Langues":
                            Languages.Remove((LanguageModel)item);
                            break;
                        case "Tags":
                            Tags.Remove((TagModel)item);
                            break;
                        case "Catégories":
                            Categories.Remove((CategorieModel)item);
                            break;
                    }
                }
            }
        }

        private void ClearSelection()
        {
            SelectedItem = null;
            InputText = "";
        }
    }
}
