using System.Collections.ObjectModel;
using System.ComponentModel;
using MySql.Data.MySqlClient; // Ajoute cette référence pour les connexions MySQL
using System.Threading.Tasks;
using Magasin_PC.Settings.Database;
using Magasin_PC.Settings.SFTP;
using System.Data;
using System.Windows.Controls.Primitives;
using Google.Protobuf.WellKnownTypes;
using System.Data.Common;
using System.Reflection.PortableExecutable;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Magasin_PC.Settings
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AppModel> Apps { get; set; } = new ObservableCollection<AppModel>();

        private MainWindow _mainwindow;
        private SFTPManager _sftpmanager;

        public MainViewModel(MainWindow mainwindow, SFTPManager sftpmanager)
        {
            _mainwindow = mainwindow;
            _sftpmanager = sftpmanager;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Méthode pour charger les apps depuis la base de données
        public async Task LoadAppsAsync()
        {
            try
            {
                string query = "SELECT Id, Name, Description, Version, FilePath, IsVisible, Icone, Requirements, CategoryId, ReleaseDate, LastUpdated, AppSize FROM Apps";

                using (MySqlCommand cmd = new MySqlCommand(query, _mainwindow.GetConnectionInfos()))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Utilisation d'un while pour itérer à travers chaque enregistrement
                        while (await reader.ReadAsync())
                        {
                            // Crée une nouvelle instance d'AppModel avec les données de l'application
                            var app = new AppModel
                            {
                                Id = reader.GetGuid("Id"),
                                Name = reader.GetString("Name"),
                                Description = reader.GetString("Description"),
                                Version = reader.GetString("Version"),
                                FilePath = reader.GetString("FilePath"),
                                IsVisible = reader.GetBoolean("IsVisible"),
                                IconePath = reader.IsDBNull("Icone") ? null : reader.GetString("Icone"),
                                Requirements = reader.GetString("Requirements"),
                                CategoryId = reader.GetInt32("CategoryId"), // Conserve le CategoryId pour l'utilisation ultérieure
                                ReleaseDate = reader.GetDateTime("ReleaseDate"),
                                LastUpdated = reader.IsDBNull("LastUpdated") ? (DateTime?)null : reader.GetDateTime("LastUpdated"),
                                AppSize = reader.GetInt64("AppSize"),
                            };

                            // Ajoute l'application à la collection
                            Apps.Add(app);
                        }
                    }
                }

                // Une fois que toutes les applications ont été chargées, 
                // on peut récupérer les autres informations en utilisant les IDs des applications
                foreach (var app in Apps)
                {
                    app.Category = await GetCategorieModelById(app.CategoryId); // Récupération de la catégorie
                    app.OS = await GetOSModelById(app.Id); // Récupération des OS
                    app.Languages = await GetLanguagesModelById(app.Id); // Récupération des langues
                    app.Tag = await GetTagModelById(app.Id); // Récupération des tags
                    try
                    {
                        // Vérification si IconePath n'est pas null ou vide avant de charger l'icône
                        if (!string.IsNullOrEmpty(app.IconePath))
                        {
                            app.Icone = await LoadIconAsync(app.IconePath);
                        }
                        else
                        {
                            app.Icone = null; // Ou une valeur par défaut si nécessaire
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                

            }
            catch (Exception ex)
            {
              
                // Gérer les erreurs de connexion ou de requête
                Console.WriteLine($"Erreur lors du chargement des applications : {ex.Message}");
            }
        }

        public ImageSource IconImageSource { get; set; }

        public async Task<ImageSource> LoadIconAsync(string iconPath)
        {
            return IconImageSource = _sftpmanager.LoadIconFromSFTP(iconPath);
        }





        private async Task<CategorieModel> GetCategorieModelById(int id)
        {
            try
            {
                string query = "SELECT * FROM Categories WHERE Id = @CategoryId";

                using (MySqlCommand categoryCmd = new MySqlCommand(query, _mainwindow.GetConnectionInfos()))
                {
                    categoryCmd.Parameters.AddWithValue("@CategoryId", id);

                    using (var reader = await categoryCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var category = new CategorieModel
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name")
                            };
                            return category;
                        }
                    }
                }
                return null;

            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<ObservableCollection<OperatingSystemModel>> GetOSModelById(Guid appId)
        {
            try
            {
                int osId = 0;
                var osCollection = new ObservableCollection<OperatingSystemModel>();

                string query = "SELECT OSId FROM AppOperatingSystems WHERE AppId = @AppId";

                using (MySqlCommand osCmd = new MySqlCommand(query, _mainwindow.GetConnectionInfos()))
                {
                    osCmd.Parameters.AddWithValue("@AppId", appId);

                    using (var reader = await osCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            osId = reader.GetInt32("OSId"); // Récupère l'OSId à partir de AppOperatingSystems
                        }
                        reader.Close();
                    }
                }

                if (osId > 0)  // Si un OSId a été trouvé
                {
                    query = "SELECT * FROM OperatingSystems WHERE Id = @Id";

                    using (MySqlCommand osDetailsCmd = new MySqlCommand(query, _mainwindow.GetConnectionInfos()))
                    {
                        osDetailsCmd.Parameters.AddWithValue("@Id", osId);

                        using (var reader = await osDetailsCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var os = new OperatingSystemModel
                                {
                                    Id = reader.GetInt32("Id"),
                                    Name = reader.GetString("Name")
                                };

                                osCollection.Add(os);  // Ajoute l'OS à la collection
                            }
                            reader.Close();
                        }
                    }

                }
                return osCollection;
            }
            catch (Exception ex)
            {
                // Gestion de l'erreur (log l'exception si nécessaire)
                return null;
            }
        }

        private async Task<ObservableCollection<LanguageModel>> GetLanguagesModelById(Guid appId)
        {
            try
            {
                int languageId = 0;
                var languageCollection = new ObservableCollection<LanguageModel>();

                string query = "SELECT LanguageId FROM AppLanguages WHERE AppId = @AppId";

                using (MySqlCommand languageCmd = new MySqlCommand(query, _mainwindow.GetConnectionInfos()))
                {
                    languageCmd.Parameters.AddWithValue("@AppId", appId);

                    using (var reader = await languageCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            languageId = reader.GetInt32("LanguageId"); // Récupère l'OSId à partir de AppOperatingSystems
                        }
                        reader.Close();
                    }
                }

                if (languageId > 0)  // Si un OSId a été trouvé
                {
                    query = "SELECT * FROM Languages WHERE Id = @Id";

                    using (MySqlCommand LanguageDetailsCmd = new MySqlCommand(query, _mainwindow.GetConnectionInfos()))
                    {
                        LanguageDetailsCmd.Parameters.AddWithValue("@Id", languageId);

                        using (var reader = await LanguageDetailsCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var language = new LanguageModel
                                {
                                    Id = reader.GetInt32("Id"),
                                    Name = reader.GetString("Name")
                                };

                                languageCollection.Add(language);  // Ajoute l'OS à la collection
                            }
                            reader.Close();
                        }
                    }
                }
                return languageCollection;
            }
            catch (Exception ex)
            {
                // Gestion de l'erreur (log l'exception si nécessaire)
                return null;
            }
        }

        private async Task<ObservableCollection<TagModel>> GetTagModelById(Guid appId)
        {
            try
            {
                int tagId = 0;
                var tagCollection = new ObservableCollection<TagModel>();

                string query = "SELECT TagId FROM AppTags WHERE AppId = @AppId";

                using (MySqlCommand tagCmd = new MySqlCommand(query, _mainwindow.GetConnectionInfos()))
                {
                    tagCmd.Parameters.AddWithValue("@AppId", appId);

                    using (var reader = await tagCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            tagId = reader.GetInt32("TagId"); // Récupère l'OSId à partir de AppOperatingSystems
                        }
                        reader.Close();
                    }
                }

                if (tagId > 0)  // Si un OSId a été trouvé
                {
                    query = "SELECT * FROM Tags WHERE Id = @Id";

                    using (MySqlCommand tagDetailsCmd = new MySqlCommand(query, _mainwindow.GetConnectionInfos()))
                    {
                        tagDetailsCmd.Parameters.AddWithValue("@Id", tagId);

                        using (var reader = await tagDetailsCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var tag = new TagModel
                                {
                                    Id = reader.GetInt32("Id"),
                                    Name = reader.GetString("Name")
                                };

                                tagCollection.Add(tag);  // Ajoute l'OS à la collection
                            }
                            reader.Close();
                        }
                    }
                }

                return tagCollection;
            }
            catch (Exception ex)
            {

                // Gestion de l'erreur (log l'exception si nécessaire)
                return null;
            }
        }

        public void AddApp(AppModel app)
        {
            if (!Apps.Contains(app))
            {
                Apps.Add(app);
                // Ajouter également l'application dans la base de données via DatabaseManager si nécessaire
            }
        }

        public void RemoveApp(AppModel app)
        {
            if (Apps.Contains(app))
            {
                Apps.Remove(app);
                // Supprimer également l'application de la base de données via DatabaseManager si nécessaire
            }
        }
    }
}
