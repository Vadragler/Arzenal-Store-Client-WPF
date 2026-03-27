namespace Arzenal.StoreManager.Core.Interfaces
{
    public interface IUiDispatcher
    {
        Task RunOnUiThreadAsync(Action action);
    }


}

