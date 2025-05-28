using Arzenal_Store_Client_WPF.Models;
using Arzenal_Store_Client_WPF.Services.API;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;


namespace Arzenal_Store_Client_WPF.Services.Database
{
    public class DatabaseManager
    {
        private readonly ApiService _apiService;

        private List<PendingDatabaseAction> _pendingActions = new List<PendingDatabaseAction>();
        public ObservableCollection<OperatingSystemModel> OperatingSystems { get; } = new ObservableCollection<OperatingSystemModel>();

        public ObservableCollection<TagModel> Tag { get; } = new ObservableCollection<TagModel>();

        public ObservableCollection<CategorieModel> Categorie { get; } = new ObservableCollection<CategorieModel>();

        public ObservableCollection<LanguageModel> Language { get; } = new ObservableCollection<LanguageModel>();


        public DatabaseManager(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<AppModel>> GetAppsAsync()
        {
            try
            {
                // Appel à l'API pour récupérer les applications
                var apps = await _apiService.GetAsync<List<AppModel>>("apps");
                return apps;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AddAsync(string tableName, string name, int? operatingSystemId = null)
        {
            try
            {
                object data = null;

                if (tableName == "OperatingSystems")
                {
                    data = new { Name = name };
                    await _apiService.PostAsync<object>("operatingsystems", data);
                }
                else if (tableName == "Languages")
                {
                    data = new { Name = name };
                    await _apiService.PostAsync<object>("languages", data);
                }
                else if (tableName == "Tags")
                {
                    data = new { Name = name };
                    await _apiService.PostAsync<object>("tags", data);
                }
                else if (tableName == "Categories")
                {
                    data = new { Name = name };
                    await _apiService.PostAsync<object>("categories", data);
                }
                else
                {
                    throw new ArgumentException("Invalid table name.");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(string tableName, int id, string name)
        {
            try
            {
                object data = new { Name = name };

                if (tableName == "OperatingSystems")
                {
                    await _apiService.PutAsync<object>($"operatingsystems/{id}", data);
                }
                else if (tableName == "Languages")
                {
                    await _apiService.PutAsync<object>($"languages/{id}", data);
                }
                else if (tableName == "Tags")
                {
                    await _apiService.PutAsync<object>($"tags/{id}", data);
                }
                else if (tableName == "Categories")
                {
                    await _apiService.PutAsync<object>($"categories/{id}", data);
                }
                else
                {
                    throw new ArgumentException("Invalid table name.");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task UpdateAppRelationsAsync(Guid appId, string tableName, string columnId, List<int> selectedIds)
        {
            try
            {
                // Supprimer les relations actuelles
                await _apiService.DeleteAsync($"apps/{appId}/{tableName}");

                // Ajouter les nouvelles relations
                foreach (var id in selectedIds)
                {
                    var data = new { AppId = appId, Id = id };
                    await _apiService.PostAsync<object>($"apps/{appId}/{tableName}", data);
                }
            }
            catch (Exception)
            {
            }
        }


        public async Task<bool> DeleteAsync(string tableName, int id)
        {
            try
            {
                // Appeler l'API pour supprimer l'enregistrement
                if (tableName == "OperatingSystems")
                {
                    await _apiService.DeleteAsync($"operatingsystems/{id}");
                }
                else if (tableName == "Languages")
                {
                    await _apiService.DeleteAsync($"languages/{id}");
                }
                else if (tableName == "Tags")
                {
                    await _apiService.DeleteAsync($"tags/{id}");
                }
                else if (tableName == "Categories")
                {
                    await _apiService.DeleteAsync($"categories/{id}");
                }
                else if (tableName == "Apps")
                {
                    await _apiService.DeleteAsync($"apps/{id}");
                }
                else
                {
                    throw new ArgumentException("Invalid table name.");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public void SavePendingAction(string actionType, string tableName, int? id, string? name, int? operatingSystemId = null)
        {
            var pendingAction = new PendingDatabaseAction
            {
                ActionType = actionType,
                TableName = tableName,
                UpdateId = id,
                UpdateName = name,
                OperatingSystemId = operatingSystemId
            };

            _pendingActions.Add(pendingAction);
            SavePendingActionsToFile();
        }

        public void SavePendingActionsToFile()
        {
            var filePath = "pendingActions.json";
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_pendingActions);
            File.WriteAllText(filePath, json);
        }

        public void LoadPendingActionsFromFile()
        {
            var filePath = "pendingActions.json";
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _pendingActions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PendingDatabaseAction>>(json)!;
            }
        }



        private async Task ApplyPendingChanges(string tableName)
        {
            LoadPendingActionsFromFile();
            // Liste temporaire pour contenir les actions à supprimer après l'itération
            var actionsToRemove = new List<PendingDatabaseAction>();
            try
            {
                foreach (var action in _pendingActions.Where(a => a.TableName == tableName).ToList()) // Copier pour éviter les conflits lors de la modification de la liste
                {
                    try
                    {
                        if (action.ActionType == "Update")
                        {
                            bool success = await UpdateAsync(tableName, (int)action.UpdateId!, action.UpdateName! ?? action.OSVersionName!);
                        }
                        else if (action.ActionType == "Insert")
                        {
                            bool success = await AddAsync(tableName, action.UpdateName! ?? action.OSVersionName!, action.OperatingSystemId);
                        }
                        else if (action.ActionType == "Delete")
                        {
                            bool success = await DeleteAsync(tableName, (int)action.UpdateId!);
                        }

                        // Ajouter l'action à la liste à supprimer si elle s'est exécutée correctement
                        actionsToRemove.Add(action);
                    }
                    catch (Exception)
                    {
                        // En cas d'erreur, on arrête la boucle
                        break;
                    }
                }

                // Supprimer toutes les actions exécutées avec succès de la liste principale
                foreach (var action in actionsToRemove)
                {
                    _pendingActions.Remove(action);
                }
                SavePendingActionsToFile();
            }
            catch (Exception)
            {

            }
        }

        public async Task OnDatabaseReconnect()
        {
            //_connection = _mainWindow.GetConnectionInfos();
            await ApplyPendingChanges("OperatingSystems");
            await ApplyPendingChanges("Languages");
            await ApplyPendingChanges("Tags");
            await ApplyPendingChanges("Categories");
        }
    }

    public class PendingDatabaseAction
    {
        public required string ActionType { get; set; } // "Insert" ou "Update"
        public required string TableName { get; set; }
        public int? UpdateId { get; set; } // Utilisé pour les modifications
        public string? UpdateName { get; set; } // Nom à insérer ou mettre à jour
        public string? OSVersionName { get; set; } // Nom de la version pour les versions d'OS
        public int? OperatingSystemId { get; set; } // Id du système d'exploitation pour lier la version
    }

    public class OperatingSystemModel : BaseModel { }

    public class TagModel : BaseModel { }

    public class LanguageModel : BaseModel { }

    public class CategorieModel : BaseModel { }
}
