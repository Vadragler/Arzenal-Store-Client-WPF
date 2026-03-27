using Arzenal.Dto.DTOs.CategorieDto;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.WPF.ViewModel;
using Moq;
using System.Collections.ObjectModel;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class AppInfoViewModelTests
    {
        private Mock<IFileDialogService> _fileDialog;
        private Mock<IWindowService> _windowService;
        private Mock<IAppApiService> _apiService;
        private MainViewModel _mainVm;

        private AppInfoViewModel CreateVm()
        {
            _fileDialog = new Mock<IFileDialogService>();
            _windowService = new Mock<IWindowService>();
            _apiService = new Mock<IAppApiService>();

            var dialogService = new Mock<IDialogService>();
            var uiDispatcher = new Mock<IUiDispatcher>();
            var authService = new Mock<IAuthService>();

            _mainVm = new MainViewModel(
                _apiService.Object,
                dialogService.Object,
                uiDispatcher.Object,
                authService.Object,
                _fileDialog.Object,
                _windowService.Object
            )
            {
                Apps = new ObservableCollection<AppViewModel>()
            };

            _apiService
                .Setup(x => x.GetCollectionAsync<ReadCategorieDto>("api/categories"))
                .ReturnsAsync(new ObservableCollection<ReadCategorieDto>
                {
                    new ReadCategorieDto { Id = Guid.NewGuid(), Name = "Games" }
                });

            return new AppInfoViewModel(
                _mainVm,
                _fileDialog.Object,
                _windowService.Object,
                _apiService.Object
            );
        }


    }
}
