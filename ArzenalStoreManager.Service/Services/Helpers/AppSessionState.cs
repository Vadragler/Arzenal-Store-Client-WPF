using System.ComponentModel;

namespace Arzenal.StoreManager.Core.Services.Helpers
{
    public class AppSessionState : INotifyPropertyChanged
    {
        private static readonly AppSessionState _instance = new();
        public static AppSessionState Instance => _instance;

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
