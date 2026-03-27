using Arzenal.StoreManager.WPF.ViewModel;
using Arzenal.StoreManager.WPF.Views.MainWindow.Controls;
using System.ComponentModel;
using System.Windows;

namespace Arzenal.StoreManager.WPF.Views.MainWindow
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, __) => await viewModel.LoadAppsAsync();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }
}
