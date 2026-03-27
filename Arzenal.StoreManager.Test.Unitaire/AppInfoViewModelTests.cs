using Arzenal.StoreManager.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class AppInfoViewModelTests
    {
        private AppInfoViewModel CreateVm()
        {
            _fileDialog = new Mock<IFileDialogService>();
            _windowService = new Mock<IWindowService>();
            _apiService = new Mock<IAppApiService>();

            _mainVm = new MainViewModel
            {
                Apps = new ObservableCollection<AppItemViewModel>()
            };

            _apiService
                .Setup(x => x.GetCollectionAsync<ReadCategorieDto>("api/categories"))
                .ReturnsAsync(new[]
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
