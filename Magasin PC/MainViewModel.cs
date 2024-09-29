using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace Magasin_PC
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AppModel> Apps { get; set; } = new ObservableCollection<AppModel>();

        private string _filePath = "C:\\MyAppStore\\apps.json";  // Chemin vers le fichier JSON

        public MainViewModel()
        {
            LoadApps();
        }

        private AppModel _selectedApp;
        public AppModel SelectedApp
        {
            get { return _selectedApp; }
            set
            {
                _selectedApp = value;
                OnPropertyChanged(nameof(SelectedApp));
            }
        }

        // Autres propriétés et méthodes

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public void AddApp(AppModel app)
        {
            if (!Apps.Contains(app))
            {
                Apps.Add(app);
                SaveApps();  // Sauvegarder les applications après l'ajout
            }
        }

        public void RemoveApp(AppModel app)
        {
            if (Apps.Contains(app))
            {
                Apps.Remove(app);
                SaveApps();  // Sauvegarder les applications après la suppression
            }
        }

        public void SaveApps()
        {
            string directoryPath = Path.GetDirectoryName(_filePath)!;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string json = JsonConvert.SerializeObject(Apps, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public void LoadApps()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                var apps = JsonConvert.DeserializeObject<ObservableCollection<AppModel>>(json);
                if (apps != null)
                {
                    Apps = apps;
                }
            }
        }
    }
}
