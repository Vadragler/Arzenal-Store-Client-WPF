using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.IO;


namespace Magasin_PC
{
    public class DatabaseManager
    {
        private MySqlConnection _connection;

        private List<PendingDatabaseAction> _pendingActions = new List<PendingDatabaseAction>();
        public ObservableCollection<OperatingSystemModel> OperatingSystems { get; } = new ObservableCollection<OperatingSystemModel>();
        public ObservableCollection<OSVersionModel> OSVersions { get; } = new ObservableCollection<OSVersionModel>();
        private MainWindow _mainWindow;

        public DatabaseManager(MySqlConnection connection, MainWindow mainWindow)
        {
            _connection = connection;
            _mainWindow = mainWindow;
        }

        public async Task<bool> AddAsync(string tableName, string name, int? operatingSystemId = null)
        {
            try
            {
                string query;

                // Construire la requête en fonction de la table
                if (tableName == "OperatingSystems")
                {
                    query = "INSERT INTO OperatingSystems (Name) VALUES (@Name)";
                }
                else if (tableName == "OSVersions")
                {
                    if (operatingSystemId == null)
                    {
                        throw new ArgumentException("OperatingSystemId is required for OSVersions.");
                    }
                    query = "INSERT INTO OSVersions (Version, OSId) VALUES (@Version, @OSId)";
                }
                else
                {
                    throw new ArgumentException("Invalid table name.");
                }

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    if (tableName == "OperatingSystems")
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                    }
                    else if (tableName == "OSVersions")
                    {
                        cmd.Parameters.AddWithValue("@Version", name);
                        cmd.Parameters.AddWithValue("@OSId", operatingSystemId);
                    }

                    await cmd.ExecuteNonQueryAsync();
                }

                // Ajouter à la collection observable
                if (tableName == "OperatingSystems")
                {
                    OperatingSystems.Add(new OperatingSystemModel { Name = name });
                }
                else if (tableName == "OSVersions")
                {
                    OSVersions.Add(new OSVersionModel { Version = name, OperatingSystemId = (int)operatingSystemId! });
                }

                return true;
            }
            catch (Exception)
            {
                // Enregistrer l'action en attente si l'insertion échoue
                SavePendingAction("Insert", tableName, null, name, operatingSystemId);
                return false;
            }
        }


        public async Task<bool> UpdateAsync(string tableName, int id, string name)
        {
           //_connection = _mainWindow.GetConnectionInfos();
            try
            {
                string query = tableName == "OperatingSystems"
                    ? "UPDATE OperatingSystems SET Name = @Name WHERE Id = @Id"
                    : "UPDATE OSVersions SET Version = @Version WHERE Id = @Id";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    await cmd.ExecuteNonQueryAsync();
                }
                
                return true;
            }catch(Exception)
            {
                SavePendingAction("Update", tableName, id, name);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string tableName, int id)
        {
            //_connection = _mainWindow.GetConnectionInfos();
            try
            {
                string query = tableName == "OperatingSystems"
                    ? "DELETE FROM OperatingSystems WHERE Id = @Id"
                    : "DELETE FROM OSVersions WHERE Id = @Id";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
                return true;
            }catch(Exception)
            {
                SavePendingAction("Delete", tableName, id, null);
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




        public async Task OnDatabaseReconnect()
        {
            //_connection = _mainWindow.GetConnectionInfos();
            await ApplyPendingChanges("OperatingSystems");
            await ApplyPendingChanges("OSVersions");
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

    public class OperatingSystemModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class OSVersionModel
    {
        public int Id { get; set; }
        public required string Version { get; set; }
        public int OperatingSystemId { get; set; } // Clé étrangère vers OperatingSystems
    }
}
