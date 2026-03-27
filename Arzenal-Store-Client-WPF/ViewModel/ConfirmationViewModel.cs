using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Input;

namespace Arzenal.StoreManager.WPF.ViewModel
{
    public class ConfirmationViewModel : INotifyPropertyChanged
    {


        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(nameof(Message)); }
        }
        public ICommand YesCommand { get; }
        public ICommand NoCommand { get; }

        public event Action<bool> RequestClose;

        public ConfirmationViewModel()
        {
            YesCommand = new RelayCommand(() => RequestClose?.Invoke(true));
            NoCommand = new RelayCommand(() => RequestClose?.Invoke(false));
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
