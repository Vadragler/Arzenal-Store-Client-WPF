using Newtonsoft.Json;
using System.IO;


namespace Magasin_PC.Settings.Database
{
    public class DatabaseConfig
    {
        public string? Server { get; set; }
        public string? Database { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? Port { get; set; }
    }

    public class StorageConfig
    {
        public string? DockerHost { get; set; }
        public string? DockerPort { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
    }


    public static class ConfigManager
    {
        private const string ConfigFilePath = "dbconfig.json";
        private const string StorageConfigFilePath = "storageconfig.json";

        public static DatabaseConfig? LoadConfig()
        {

            if (!File.Exists(ConfigFilePath))
            {
                return null;
            }
            else
            {

                var configJson = File.ReadAllText(ConfigFilePath);
                var config = JsonConvert.DeserializeObject<DatabaseConfig>(configJson);

                // Déchiffre le mot de passe après chargement
                config!.Password = EncryptionHelper.Decrypt(config.Password!);


                return config;
            }
        }


        public static void SaveConfig(DatabaseConfig config)
        {
            // Chiffre le mot de passe avant de le sauvegarder
            config.Password = EncryptionHelper.Encrypt(config.Password!);

            var configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, configJson);
        }

        // Méthodes pour la configuration de stockage Docke
        public static StorageConfig? LoadStorageConfig()
        {
            if (!File.Exists(StorageConfigFilePath))
            {
                return null;
            }
            else
            {

                var configJson = File.ReadAllText(StorageConfigFilePath);
                var config = JsonConvert.DeserializeObject<StorageConfig>(configJson);
                config!.Password = EncryptionHelper.Decrypt(config.Password!);

                return config;
            }
        }

        public static void SaveStorageConfig(StorageConfig config)
        {
            config.Password = EncryptionHelper.Encrypt(config.Password!);

            var configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(StorageConfigFilePath, configJson);
        }

    }
}
