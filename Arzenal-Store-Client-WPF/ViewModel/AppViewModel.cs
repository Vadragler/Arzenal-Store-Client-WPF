using Arzenal.Dto.DTOs.AppDto;
using Arzenal.StoreManager.Core.Interfaces;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Arzenal.StoreManager.WPF.ViewModel
{
    public class AppViewModel : INotifyPropertyChanged
    {
        public ReadAppDto App { get; }

        private ImageSource? _icon;
        public ImageSource? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(); }
        }

        public AppViewModel(ReadAppDto app)
        {
            App = app;
        }

        public async Task LoadIconAsync(IAppApiService apiService)
        {
            var stream = await apiService.GetAsync<Stream>($"api/appfiles/icon/{App.Id}");
            if (stream == null) return;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            Icon = bitmap;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
