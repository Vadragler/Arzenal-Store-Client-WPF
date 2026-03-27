using Arzenal.StoreManager.Core.Models;
using System.Data;

namespace Arzenal.StoreManager.Data.Database
{
    public class DatabaseManager
    {

        private List<PendingDatabaseAction> _pendingActions = new List<PendingDatabaseAction>();


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
                            //bool success = await UpdateAsync(tableName, (int)action.UpdateId!, action.UpdateName! ?? action.OSVersionName!);
                        }
                        else if (action.ActionType == "Insert")
                        {
                            //bool success = await AddAsync(tableName, action.UpdateName! ?? action.OSVersionName!, action.OperatingSystemId);
                        }
                        else if (action.ActionType == "Delete")
                        {
                            //bool success = await DeleteAsync(tableName, (int)action.UpdateId!);
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
