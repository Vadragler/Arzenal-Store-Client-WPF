using Magasin_PC.Settings.SFTP;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Collections.ObjectModel;
using System.IO;


namespace Magasin_PC.Settings.Database
{
    public class DatabaseManager
    {

        private MySqlConnection _connection;

        private List<PendingDatabaseAction> _pendingActions = new List<PendingDatabaseAction>();
        public ObservableCollection<OperatingSystemModel> OperatingSystems { get; } = new ObservableCollection<OperatingSystemModel>();

        public ObservableCollection<TagModel> Tag { get; } = new ObservableCollection<TagModel>();

        public ObservableCollection<CategorieModel> Categorie { get; } = new ObservableCollection<CategorieModel>();

        public ObservableCollection<LanguageModel> Language { get; } = new ObservableCollection<LanguageModel>();
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
                else if (tableName == "Languages")
                {
                    query = "INSERT INTO Languages (Name) VALUES (@Name)";
                }
                else if (tableName == "Tags")
                {
                    query = "INSERT INTO Tags (Name) VALUES (@Name)";
                }
                else if (tableName == "Categories")
                {
                    query = "INSERT INTO Categories (Name) VALUES (@Name)";
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
                    else if (tableName == "Languages")
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                    }
                    else if (tableName == "Tags")
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                    }
                    else if (tableName == "Categories")
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                    }

                    await cmd.ExecuteNonQueryAsync();
                }

                // Ajouter à la collection observable
                if (tableName == "OperatingSystems")
                {
                    OperatingSystems.Add(new OperatingSystemModel { Name = name });
                }
                else if (tableName == "Languages")
                {
                    Language.Add(new LanguageModel { Name = name });
                }
                else if (tableName == "Tag")
                {
                    Tag.Add(new TagModel { Name = name });
                }
                else if (tableName == "Categories")
                {
                    Categorie.Add(new CategorieModel { Name = name });
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

        public async Task AddOrUpdateAppAsync(Guid id, string name, string description, string version, string filepath, bool isVisible, string iconepath, string requirements, int category, DateTime releaseDate, DateTime? lastUpdated, long appSize, ObservableCollection<OperatingSystemModel> selectedos, ObservableCollection<LanguageModel> selectedlanguage, ObservableCollection<TagModel> selectedtag, SftpClient _sftpclient)
        {
            SFTPManager _sftpmanager = new SFTPManager(_sftpclient.GetClient());
           string icone = _sftpmanager.UploadFile(iconepath, Path.GetFileNameWithoutExtension(iconepath),true);
            filepath = _sftpmanager.UploadFile(filepath, Path.GetFileNameWithoutExtension(filepath));
            try
            {
                string query = @"INSERT INTO Apps (Id, Name, Description, Version,FilePath, IsVisible, Icone, Requirements, CategoryId, ReleaseDate, LastUpdated, AppSize)
                         VALUES (@Id, @Name, @Description, @Version,@FilePath ,@IsVisible, @Icone, @Requirements, @CategoryId, @ReleaseDate, @LastUpdated, @AppSize)
                         ON DUPLICATE KEY UPDATE 
                         Name = @Name, 
                         Description = @Description, 
                         Version = @Version,
                         FilePath = @FilePath,
                         IsVisible = @IsVisible,
                         Icone = @Icone,
                         Requirements = @Requirements,
                         CategoryId = @CategoryId,
                         ReleaseDate = @ReleaseDate,
                         LastUpdated = @LastUpdated,
                         AppSize = @AppSize";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Version", version);
                    cmd.Parameters.AddWithValue("@FilePath", filepath);
                    cmd.Parameters.AddWithValue("@IsVisible", isVisible);
                    cmd.Parameters.AddWithValue("@Icone", icone);
                    cmd.Parameters.AddWithValue("@Requirements", requirements);
                    cmd.Parameters.AddWithValue("@CategoryId", category);
                    cmd.Parameters.AddWithValue("@ReleaseDate", releaseDate);
                    cmd.Parameters.AddWithValue("@LastUpdated", lastUpdated);
                    cmd.Parameters.AddWithValue("@AppSize", appSize);

                    await cmd.ExecuteNonQueryAsync();
                }

                // Gestion des systèmes d'exploitation
                if (selectedos != null && selectedos.Any())
                    await UpdateAppRelationsAsync(id, "AppOperatingSystems", "OSId", selectedos.Select(os => os.Id).ToList());

                // Gestion des langues
                if (selectedlanguage != null && selectedlanguage.Any())
                    await UpdateAppRelationsAsync(id, "AppLanguages", "LanguageId", selectedlanguage.Select(language => language.Id).ToList());

                // Gestion des tags
                if (selectedtag != null && selectedtag.Any())
                    await UpdateAppRelationsAsync(id, "AppTags", "TagId", selectedtag.Select(tag => tag.Id).ToList());
                
            }
            catch (Exception)
            {
                
            }
        }

        private async Task UpdateAppRelationsAsync(Guid appId, string tableName, string columnId, List<int> selectedIds)
        {
            try
            {
                // 1. Récupérer les ids actuels
                string selectQuery = $"SELECT {columnId} FROM {tableName} WHERE AppId = @AppId";
                List<int> currentIds = new List<int>();

                using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, _connection))
                {
                    selectCmd.Parameters.AddWithValue("@AppId", appId);
                    using (var reader = await selectCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            currentIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                // 2. Déterminer les ids à ajouter et à supprimer
                var idsToAdd = selectedIds.Except(currentIds).ToList();
                var idsToRemove = currentIds.Except(selectedIds).ToList();

                // 3. Suppression des relations désélectionnées
                if (idsToRemove.Count > 0)
                {
                    string deleteQuery = $"DELETE FROM {tableName} WHERE AppId = @AppId AND {columnId} = @Id";
                    foreach (var idToRemove in idsToRemove)
                    {
                        using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, _connection))
                        {
                            deleteCmd.Parameters.AddWithValue("@AppId", appId);
                            deleteCmd.Parameters.AddWithValue("@Id", idToRemove);
                            await deleteCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // 4. Ajout des nouvelles relations sélectionnées
                if (idsToAdd.Count > 0)
                {
                    string insertQuery = $"INSERT INTO {tableName} (AppId, {columnId}) VALUES (@AppId, @Id)";
                    foreach (var idToAdd in idsToAdd)
                    {
                        using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, _connection))
                        {
                            insertCmd.Parameters.AddWithValue("@AppId", appId);
                            insertCmd.Parameters.AddWithValue("@Id", idToAdd);
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        public async Task<bool> UpdateAsync(string tableName, int id, string name)
        {
            //_connection = _mainWindow.GetConnectionInfos();
            try
            {
                string query;

                if (tableName == "OperatingSystems")
                {
                    query = "UPDATE OperatingSystems SET Name = @Name WHERE Id = @Id";
                }
                else if (tableName == "Languages")
                {
                    query = "UPDATE Languages SET Name = @Name WHERE Id = @Id";
                }
                else if (tableName == "Tags")
                {
                    query = "UPDATE Tags SET Name = @Name WHERE Id = @Id";
                }
                else if (tableName == "Categories")
                {
                    query = "UPDATE Categories SET Name = @Name WHERE Id = @Id";
                }
                else
                {
                    throw new ArgumentException("Invalid table name.");
                }
                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception)
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
                string query;
                if (tableName == "OperatingSystems")
                {
                    query = "DELETE FROM OperatingSystems WHERE Id = @Id";
                }
                else if (tableName == "Languages")
                {
                    query = "DELETE FROM Languages WHERE Id = @Id";
                }
                else if (tableName == "Tags")
                {
                    query = "DELETE FROM Tags WHERE Id = @Id";
                }
                else if (tableName == "Categories")
                {
                    query = "DELETE FROM Categories WHERE Id = @Id";
                }
                else
                {
                    throw new ArgumentException("Invalid table name.");
                }
                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch (Exception)
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

    public class OperatingSystemModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class TagModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class LanguageModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class CategorieModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
