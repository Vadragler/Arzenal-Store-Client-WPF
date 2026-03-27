using Arzenal.StoreManager.WPF.ViewModel;
using System.Windows;

namespace Arzenal.StoreManager.WPF.Views
{
    public partial class ConfirmationWindow : Window
    {
        public ConfirmationWindow()
        {
            InitializeComponent();

            if (DataContext is ConfirmationViewModel vm)
            {
                vm.RequestClose += result =>
                {
                    DialogResult = result;
                    Close();
                };
            }
        }
    }
}