using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Magasin_PC
{
    public class DatabaseManager
    {
        private MySqlConnection _connection;
        private List<PendingDatabaseAction> _pendingActions = new List<PendingDatabaseAction>();
        public ObservableCollection<OperatingSystemModel> OperatingSystems { get; } = new ObservableCollection<OperatingSystemModel>();
        public ObservableCollection<OSVersionModel> OSVersions { get; } = new ObservableCollection<OSVersionModel>();
        private MainWindow _mainWindow;

        public DatabaseManager(MySqlConnection connection)
        {
            _connection = connection;
        }

        public async Task AddAsync(string tableName, string name, int? operatingSystemId = null)
        {
            string query = tableName == "OperatingSystems"
                ? "INSERT INTO OperatingSystems (Name) VALUES (@Name)"
                : "INSERT INTO OSVersions (Version, OperatingSystemId) VALUES (@Version, @OperatingSystemId)";

            using (var cmd = new MySqlCommand(query, _connection))
            {
                if (tableName == "OperatingSystems")
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Version", name);
                    cmd.Parameters.AddWithValue("@OperatingSystemId", operatingSystemId);
                }
                await cmd.ExecuteNonQueryAsync();
            }

            // Ajouter à la collection observable
            if (tableName == "OperatingSystems")
            {
                OperatingSystems.Add(new OperatingSystemModel { Name = name });
            }
            else
            {
                OSVersions.Add(new OSVersionModel { Version = name, OperatingSystemId = (int)operatingSystemId });
            }
        }

        public async Task UpdateAsync(string tableName, int id, string name)
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
        }

        public async Task DeleteAsync(string tableName, int id)
        {
            string query = tableName == "OperatingSystems"
                ? "DELETE FROM OperatingSystems WHERE Id = @Id"
                : "DELETE FROM OSVersions WHERE Id = @Id";

            using (var cmd = new MySqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task ApplyPendingChanges(string tableName)
        {
            foreach (var action in _pendingActions.Where(a => a.TableName == tableName))
            {
                try
                {
                    if (action.ActionType == "Update")
                    {
                        await UpdateAsync(tableName, (int)action.UpdateId, action.UpdateName ?? action.OSVersionName);
                    }
                    else if (action.ActionType == "Insert")
                    {
                        await AddAsync(tableName, action.UpdateName ?? action.OSVersionName, action.OperatingSystemId);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'application des modifications en attente pour {tableName} : {ex.Message}");
                }
            }

            _pendingActions.RemoveAll(a => a.TableName == tableName);
        }

        public async Task OnDatabaseReconnect()
        {
            _connection = _mainWindow.GetConnectionInfos();
            await ApplyPendingChanges("OperatingSystems");
            await ApplyPendingChanges("OSVersions");
        }
    }

    public class PendingDatabaseAction
    {
        public string ActionType { get; set; } // "Insert" ou "Update"
        public string TableName { get; set; }
        public int? UpdateId { get; set; } // Utilisé pour les modifications
        public string UpdateName { get; set; } // Nom à insérer ou mettre à jour
        public string OSVersionName { get; set; } // Nom de la version pour les versions d'OS
        public int? OperatingSystemId { get; set; } // Id du système d'exploitation pour lier la version
    }

    public class OperatingSystemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OSVersionModel
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public int OperatingSystemId { get; set; } // Clé étrangère vers OperatingSystems
    }
}
