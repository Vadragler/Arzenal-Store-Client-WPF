using Arzenal.StoreManager.WPF.ViewModel;
using System.Windows;


namespace Arzenal.StoreManager.WPF.Views.Settings
{
    public partial class SettingWindow : Window
    {
        public SettingWindow(SettingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
