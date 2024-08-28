using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace MagasinPC
{
    public class MainViewModel
    {
        public ObservableCollection<AppModel> Apps { get; set; } = new ObservableCollection<AppModel>();

        private string _filePath = "C:\\MyAppStore\\apps.json";  // Chemin vers le fichier JSON

        public MainViewModel()
        {
            Apps = new ObservableCollection<AppModel>();
            LoadApps();
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
