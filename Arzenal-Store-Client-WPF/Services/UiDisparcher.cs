using Arzenal.StoreManager.Core.Interfaces;
using System.Windows;

namespace Arzenal.StoreManager.WPF.Services
{
    public class UiDispatcher : IUiDispatcher
    {
        public Task RunOnUiThreadAsync(Action action)
        {
            return Application.Current.Dispatcher.InvokeAsync(action).Task;
        }
    }
}
