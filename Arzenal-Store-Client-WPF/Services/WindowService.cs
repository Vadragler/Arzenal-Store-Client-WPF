using Arzenal.StoreManager.Core.Interfaces;
using System.Windows;

namespace Arzenal.StoreManager.WPF.Services
{
    public class WindowService : IWindowService
    {
        public void CloseWindow(object viewModel)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == viewModel)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
