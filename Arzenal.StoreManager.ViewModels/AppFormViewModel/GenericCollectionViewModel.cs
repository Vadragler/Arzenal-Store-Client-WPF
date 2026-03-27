using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.ViewModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Arzenal.StoreManager.ViewModels.AppFormViewModel
{
    public class GenericCollectionViewModel<T> : ObservableObject
    {
        public Dictionary<T, string> LocalFilePaths { get; } = new();

        private readonly IFileDialogService _fileDialogService;
       
        private readonly Func<Task<ObservableCollection<T>>> _loadItemsFunc;

        public event Action? ItemsLoaded;

        public string? SectionTitle { get; set; }
        public ObservableCollection<T> Items { get; } = new();
        public ObservableCollection<T> SelectedItems { get; } = new();

        public bool IsOsSection { get; set; }

        private T? _chosenItem;
        public T? ChosenItem
        {
            get => _chosenItem;
            set
            {
                if (SetProperty(ref _chosenItem, value) && value != null)
                {
                    AddItem(value);
                    _chosenItem = default; // reset pour pouvoir resélectionner
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public RelayCommand<T> BrowseFileCommand { get; }
        public Task Initialization { get; }
        public GenericCollectionViewModel(IFileDialogService fileDialogService, Func<Task<ObservableCollection<T>>> loadItemsFunc)
        {
            _fileDialogService = fileDialogService;
            _loadItemsFunc = loadItemsFunc;

            AddCommand = new RelayCommand<T>(AddItem, CanAddItem);
            RemoveCommand = new RelayCommand<T>(RemoveItem);
            IsOsSection = typeof(T) == typeof(ReadOperatingSystemDto);
            BrowseFileCommand = new RelayCommand<T>(BrowseFile, CanBrowseFile);
            Initialization = LoadItemsAsync();
        }

        private bool CanBrowseFile(T item) => IsOsSection;

        private void BrowseFile(T item)
        {
            if (!IsOsSection || item == null) return;

            
            var path = _fileDialogService.SelectFile();
            if (!string.IsNullOrEmpty(path))
            {
                LocalFilePaths[item] = path;
                OnPropertyChanged(nameof(LocalFilePaths));
            }
        }

        private async Task LoadItemsAsync()
        {
            var items = await _loadItemsFunc();
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
            ItemsLoaded?.Invoke();
        }

        private void AddItem(T item)
        {
            CollectionHelper.AddItem(item, SelectedItems);
        }

        private bool CanAddItem(T item)
        {
            return CollectionHelper.CanAddItem(item, SelectedItems);
        }

        private void RemoveItem(T item)
        {
            CollectionHelper.RemoveItem(item, SelectedItems);
        }
    }
}
