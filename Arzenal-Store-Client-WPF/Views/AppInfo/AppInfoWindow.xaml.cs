using Arzenal.StoreManager.WPF.ViewModel;
using System.Windows;


namespace Arzenal.StoreManager.WPF.Views.AppInfo
{
    public partial class AppInfoWindow : Window
    {

        public AppInfoWindow(AppInfoViewModel _viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
