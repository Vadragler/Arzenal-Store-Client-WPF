using System.Windows;

namespace Arzenal.StoreManager.WPF
{
    public static class UiMessageService
    {
        public static void ShowError(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    message,
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });
        }

        public static void ShowWarning(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    message,
                    "Attention",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            });
        }
    }

}
