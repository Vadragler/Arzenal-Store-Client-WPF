using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Dto.DTOs.AppFileDto;
using Arzenal.Dto.DTOs.CategorieDto;
using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.Dto.DTOs.TagDto;
using Arzenal.StoreManager.Core.Dtos;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Mapping;
using Arzenal.StoreManager.ViewModels.AppFormViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace Arzenal.StoreManager.WPF.ViewModel
{
    public partial class AppInfoViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IFileDialogService _fileDialogService;
        private readonly IWindowService _windowService;
        private readonly IAppApiService _appApiService;
        private readonly AppMapper _mapper = new();

        public ObservableCollection<ReadCategorieDto> AvailableCategories { get; } = new();

        public GenericCollectionViewModel<ReadOperatingSystemDto> OsSection { get; }
        public GenericCollectionViewModel<ReadLanguageDto> LanguageSection { get; }
        public GenericCollectionViewModel<ReadTagDto> TagSection { get; }

        

        public ICommand BrowseIconCommand { get; }
        public ICommand ModifyFilePathCommand { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand SaveButtonCommand { get; }

        public AppInfoViewModel(
            MainViewModel mainViewModel,
            IFileDialogService fileDialogService,
            IWindowService windowService,
            IAppApiService appApiService,
            ReadAppDto app = null)
        {
            _mainViewModel = mainViewModel;
            _fileDialogService = fileDialogService;
            _windowService = windowService;
            _appApiService = appApiService;

            OsSection = new GenericCollectionViewModel<ReadOperatingSystemDto>(
                _fileDialogService,
                () => _appApiService.GetCollectionAsync<ReadOperatingSystemDto>("api/operatingsystems"))
            { SectionTitle = "Systèmes d'exploitation:" };

            LanguageSection = new GenericCollectionViewModel<ReadLanguageDto>(
                _fileDialogService,
                () => _appApiService.GetCollectionAsync<ReadLanguageDto>("api/languages"))
            { SectionTitle = "Languages:" };

            TagSection = new GenericCollectionViewModel<ReadTagDto>(
                _fileDialogService,
                () => _appApiService.GetCollectionAsync<ReadTagDto>("api/tags"))
            { SectionTitle = "Tags:" };

            if (app != null)
            {
                OsSection.ItemsLoaded += () => RestoreSelectedOs(app);
                LanguageSection.ItemsLoaded += () => RestoreSelectedLanguages(app);
                TagSection.ItemsLoaded += () => RestoreSelectedTags(app);
            }


            BrowseIconCommand = new RelayCommand(BrowseIcon);
            ModifyFilePathCommand = new RelayCommand(ModifyFilePath);
            SaveButtonCommand = new RelayCommand(async () => await SaveAsync());
            CancelButtonCommand = new RelayCommand(Cancel);

            if (app != null)
                Load(app);

            _ = LoadDataAsync();
        }

        private void Load(ReadAppDto appDto)
        {
            var app = _mapper.MapDtoToModel(appDto);
            _appId = app.Id;
            AppName = app.Name;
            AppFilePath = app.FilePath;
            AppDescription = app.Description;
            AppVersion = app.Version;
            SelectedCategory = AvailableCategories.FirstOrDefault(c => c.Name == app.Category);

            // Tags
            foreach (var tagName in app.Tags ?? Enumerable.Empty<string>())
            {
                var tag = TagSection.Items.FirstOrDefault(x => x.Name == tagName);
                if (tag != null)
                    TagSection.SelectedItems.Add(tag);
            }

            // Languages
            foreach (var langName in app.Languages ?? Enumerable.Empty<string>())
            {
                var lang = LanguageSection.Items.FirstOrDefault(x => x.Name == langName);
                if (lang != null)
                    LanguageSection.SelectedItems.Add(lang);
            }

            // Operating systems
            foreach (var osName in app.OperatingSystems ?? Enumerable.Empty<string>())
            {
                var os = OsSection.Items.FirstOrDefault(x => x.Name == osName);
                if (os != null)
                    OsSection.SelectedItems.Add(os);
            }


            IsVisible = app.IsVisible;
            AppSize = app.AppSize;
        }

        #region Properties
        private Guid _appId;
        public Guid AppId { get => _appId; set => SetProperty(ref _appId, value); }

        private string _appName;
        public string AppName { get => _appName; set => SetProperty(ref _appName, value); }

        private string _appDescription;
        public string AppDescription { get => _appDescription; set => SetProperty(ref _appDescription, value); }

        private string _appVersion;
        public string AppVersion { get => _appVersion; set => SetProperty(ref _appVersion, value); }

        private string _appIconPath;
        public string AppIconPath { get => _appIconPath; set => SetProperty(ref _appIconPath, value); }

        private bool _isNameDuplicate;
        public bool IsNameDuplicate { get => _isNameDuplicate; set => SetProperty(ref _isNameDuplicate, value); }

        private string _appFilePath;
        public string AppFilePath { get => _appFilePath; set => SetProperty(ref _appFilePath, value); }

        private bool _isFilePathDuplicate;
        public bool IsFilePathDuplicate { get => _isFilePathDuplicate; set => SetProperty(ref _isFilePathDuplicate, value); }

        private string _appRequirements;
        public string AppRequirements { get => _appRequirements; set => SetProperty(ref _appRequirements, value); }

        private double _appSize;
        public double AppSize { get => _appSize; set => SetProperty(ref _appSize, value); }

        private bool _isVisible;
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }

        private ReadCategorieDto _selectedCategory;
        public ReadCategorieDto SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
        #endregion

        private void RestoreSelectedTags(ReadAppDto app)
        {
            foreach (var tagName in app.Tags ?? Enumerable.Empty<string>())
            {
                var tag = TagSection.Items.FirstOrDefault(x => x.Name == tagName);
                if (tag != null)
                    TagSection.SelectedItems.Add(tag);
            }
        }

        private void RestoreSelectedOs(ReadAppDto app)
        {
            foreach (var osName in app.OperatingSystems?? Enumerable.Empty<string>())
            {
                var os = OsSection.Items.FirstOrDefault(x => x.Name == osName);
                if (os != null)
                    OsSection.SelectedItems.Add(os);
            }
        }


        private void RestoreSelectedLanguages(ReadAppDto app)
        {
            foreach (var langName in app.Languages ?? Enumerable.Empty<string>())
            {
                var lang = LanguageSection.Items.FirstOrDefault(x => x.Name == langName);
                if (lang != null)
                    LanguageSection.SelectedItems.Add(lang);
            }
        }


        private async Task LoadDataAsync()
        {
            var categories = await _appApiService.GetCollectionAsync<ReadCategorieDto>("api/categories");
            AvailableCategories.Clear();
            foreach (var cat in categories)
                AvailableCategories.Add(cat);

            if (SelectedCategory == null && AvailableCategories.Count > 0)
                SelectedCategory = AvailableCategories.First();
        }

        #region UI Actions via services
        private void BrowseIcon()
        {
            var path = _fileDialogService.SelectImage();
            if (!string.IsNullOrEmpty(path))
                AppIconPath = path;
        }

        private void ModifyFilePath()
        {
            var path = _fileDialogService.SelectFile();
            if (!string.IsNullOrEmpty(path))
            {
                AppFilePath = path;
                IsFilePathDuplicate = _mainViewModel.Apps.Any(a => a.App.FilePath == path);
            }
        }

        private void Cancel()
        {
            _windowService.CloseWindow(this);
        }
        #endregion

        private async Task SaveAsync()
        {
            ReadAppDto? app;

            if (_appId == Guid.Empty)
            {
                var createApp = new CreateAppDto
                {
                    Name = AppName,
                    Description = AppDescription,
                    Version = AppVersion,
                    Requirements = AppRequirements,
                    AppSize = (long)AppSize,
                    IsVisible = IsVisible,
                    CategoryId = SelectedCategory?.Id,
                    TagIds = TagSection.SelectedItems.Select(t => t.Id).ToList(),
                    LanguageIds = LanguageSection.SelectedItems.Select(l => l.Id).ToList(),
                    OsIds = OsSection.SelectedItems.Select(os => os.Id).ToList()
                };
                app = await _appApiService.PostAsync<ReadAppDto>("api/apps", createApp);
            }
            else
            {
                var updateApp = new UpdateAppDto
                {
                    Name = AppName,
                    Description = AppDescription,
                    Version = AppVersion,
                    Requirements = AppRequirements,
                    AppSize = (long)AppSize,
                    IsVisible = IsVisible,
                    CategoryId = SelectedCategory?.Id,
                    TagIds = TagSection.SelectedItems.Select(t => t.Id).ToList(),
                    LanguageIds = LanguageSection.SelectedItems.Select(l => l.Id).ToList(),
                    OsIds = OsSection.SelectedItems.Select(os => os.Id).ToList()
                };
                app = await _appApiService.PatchAsync<ReadAppDto>($"api/apps/{_appId}", updateApp);
            }

            if (app != null)
                await UploadFilesAsync(app);
            _windowService.CloseWindow(this);
        }

        private async Task UploadFilesAsync(ReadAppDto app)
        {
            // Upload fichier app
            foreach (var os in OsSection.SelectedItems)
            {
                if (OsSection.LocalFilePaths.TryGetValue(os, out var path))
                {
                    // Créer ton UploadFileDto avec ce path
                    var upload = new UploadFileClientDto
                    {
                        ApiDto = new UploadFileDto
                    {
                        AppId = app.Id,
                        Version = app.Version,
                        Platform = os.Name,
                        Type = "app",
                        FileName = Path.GetFileName(path)
                    },
                        LocalPath = path
                    };
                    await _appApiService.PostAsync<ReadAppFilePathDto>("api/appfiles/upload", upload);
                }
            }


            // Upload icône
            if (!string.IsNullOrEmpty(AppIconPath) && app.IconePath != AppIconPath)
            {
                var iconUploadClient = new UploadFileClientDto
                {
                    ApiDto = new UploadFileDto
                    {
                        AppId = app.Id,
                        Version = app.Version,
                        Type = "icon",
                        FileName = Path.GetFileName(AppIconPath) // juste le nom
                    },
                    LocalPath = AppIconPath
                };
                await _appApiService.PostAsync<ReadAppFilePathDto>("api/appfiles/upload", iconUploadClient);
            }
        }


        public void UpdateDuplicateChecks()
        {
            var apps = _mainViewModel.Apps;
            IsNameDuplicate = apps.Any(a => a.App.Name == AppName && a.App.Id != _appId);
            IsFilePathDuplicate = apps.Any(a => a.App.FilePath == AppFilePath && a.App.Id != _appId);
        }
    }
}
