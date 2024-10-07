using Renci.SshNet;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Magasin_PC.Settings.SFTP
{
    public class SFTPManager
    {
        private readonly Renci.SshNet.SftpClient _sftpClient;

        public SFTPManager(Renci.SshNet.SftpClient sftpClient)
        {
            _sftpClient = sftpClient;
        }

        public ImageSource LoadIconFromSFTP(string iconPath)
        {
            byte[] iconData = GetFileContentAsByteArray(iconPath);

            using (var stream = new MemoryStream(iconData))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public string UploadFile(string localFilePath, string destinationFileName, bool isIcon = false)
        {
            string destinationPath = isIcon ? $"/path/icons/{destinationFileName}" : $"/path/apps/{destinationFileName}";
            try
            {
                
                if (!_sftpClient.IsConnected)
                {
                    throw new Exception("SFTP client is not connected.");
                }

                using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                {
                    _sftpClient.UploadFile(fileStream, destinationFileName);
                }

                return destinationFileName;
            }
            catch (Exception ex)
            {
                // Gestion de l'erreur
                Console.WriteLine($"Erreur lors de l'upload du fichier : {ex.Message}");
                return null;
            }
        }

        public async Task UploadDirectoryAsync(string localDirectory, string remoteDirectory)
        {
            if (_sftpClient != null && _sftpClient.IsConnected)
            {
                // Créer le dossier distant s'il n'existe pas
                if (!_sftpClient.Exists(remoteDirectory))
                {
                    _sftpClient.CreateDirectory(remoteDirectory);
                }

                // Lister les fichiers dans le répertoire distant
                var remoteFiles = _sftpClient.ListDirectory(remoteDirectory)
                    .Where(f => !f.IsDirectory && !f.IsSymbolicLink)
                    .Select(f => f.Name)
                    .ToList();

                // Lister les fichiers dans le répertoire local
                var localFiles = Directory.GetFiles(localDirectory).Select(Path.GetFileName).ToList();

                // Supprimer les fichiers dans le dossier distant qui ne sont pas dans le dossier local
                foreach (var remoteFile in remoteFiles)
                {
                    if (!localFiles.Contains(remoteFile))
                    {
                        string remoteFilePath = Path.Combine(remoteDirectory, remoteFile).Replace("\\", "/");
                        _sftpClient.DeleteFile(remoteFilePath);
                        Console.WriteLine($"Fichier supprimé : {remoteFilePath}");
                    }
                }

                // Envoyer les fichiers manquants ou à réécrire
                foreach (var localFile in localFiles)
                {
                    string localFilePath = Path.Combine(localDirectory, localFile);
                    string remoteFilePath = Path.Combine(remoteDirectory, localFile).Replace("\\", "/");

                    using (var fileStream = File.OpenRead(localFilePath))
                    {
                        await Task.Run(() => _sftpClient.UploadFile(fileStream, remoteFilePath, true));
                        Console.WriteLine($"Fichier envoyé : {remoteFilePath}");
                    }
                }
            }
            else
            {
                Console.WriteLine("La connexion SFTP n'est pas ouverte.");
            }
        }

        public byte[] GetFileContentAsByteArray(string remoteFilePath)
        {
            if (_sftpClient != null && _sftpClient.IsConnected)
            {
                using (var memoryStream = new MemoryStream())
                {
                    _sftpClient.DownloadFile(remoteFilePath, memoryStream);
                    return memoryStream.ToArray();
                }
            }
            else
            {
                throw new Exception("La connexion SFTP n'est pas ouverte.");
            }
        }


        public bool DownloadFile(string remoteFilePath, string localFilePath)
        {
            try
            {
                if (!_sftpClient.IsConnected)
                {
                    throw new Exception("SFTP client is not connected.");
                }

                using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    _sftpClient.DownloadFile(remoteFilePath, fileStream);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Gestion de l'erreur
                Console.WriteLine($"Erreur lors du téléchargement du fichier : {ex.Message}");
                return false;
            }
        }

        // Autres méthodes possibles, par exemple pour supprimer des fichiers, créer des dossiers, etc.
        public async Task DeleteFileAsync(string remoteFilePath)
        {
            if (_sftpClient != null && _sftpClient.IsConnected)
            {
                if (_sftpClient.Exists(remoteFilePath))
                {
                    await Task.Run(() => _sftpClient.DeleteFile(remoteFilePath));
                }
                else
                {
                    Console.WriteLine("Le fichier n'existe pas sur le serveur.");
                }
            }
            else
            {
                Console.WriteLine("La connexion SFTP n'est pas ouverte.");
            }
        }

    }
}
