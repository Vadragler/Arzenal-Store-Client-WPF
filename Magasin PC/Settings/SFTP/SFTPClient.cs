using Magasin_PC;
using Renci.SshNet;
using Renci.SshNet.Common; // Assurez-vous d'avoir installé la bibliothèque SSH.NET via NuGet
using System.Windows;
namespace Magasin_PC.Settings.SFTP
{
    public class SftpClient
    {
        private Renci.SshNet.SftpClient? _client;
        private CancellationTokenSource? _sftpConnectionCancellationTokenSource;


        public bool IsConnected => _client?.IsConnected ?? false;

        public async Task ConnectAsync(string host, string username, string password, int port)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            // Change l'icône pour indiquer que la connexion est en cours
            UpdateConnectionIcon(0);


            if (_client != null && _client.IsConnected)
                return;
            await Task.Run(() =>
            {
                try
                {

                    _client = new Renci.SshNet.SftpClient(host, port, username, password);
                    _client.Connect();


                    // Change l'icône pour indiquer que la connexion a réussi
                    UpdateConnectionIcon(1);
                }
                catch (Exception)
                {
                    // Change l'icône pour indiquer une erreur générale
                    UpdateConnectionIcon(-1);

                }
            });
        }
        public void StartCheckingSftpConnection(string host, string username, string password, int port)
        {
            _sftpConnectionCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => CheckSftpConnectionStatus(host, username, password, port, _sftpConnectionCancellationTokenSource.Token));
        }

        public Renci.SshNet.SftpClient? GetClient() { return _client; }
        private async Task CheckSftpConnectionStatus(string host, string username, string password, int port, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) // Vérifier régulièrement si l'annulation est demandée
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); // Vérifier la connexion toutes les 2 secondes

                if (IsConnected)
                {
                    // Vérifier si la connexion est toujours active
                    if (!await IsSftpConnectionAliveAsync())
                    {
                        // La connexion a été perdue
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UpdateConnectionIcon(-1); // Indiquer que la connexion est perdue
                        });
                    }
                }
                else
                {
                    // La connexion n'est pas active
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateConnectionIcon(-1); // Indiquer que la connexion est perdue
                    });

                    // Attendre 1 minute avant la prochaine tentative de reconnexion
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                    try
                    {
                        // Tenter de se reconnecter
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        ConnectAsync(host, username, password, port));
                    }
                    catch (Exception)
                    {
                        // Gérer l'échec de la reconnexion ici, loguer l'erreur
                    }
                }
            }
        }

        private async Task<bool> IsSftpConnectionAliveAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (_client != null && _client.IsConnected)
                    {
                        // Essaye de lister le répertoire racine pour vérifier la connexion
                        var directories = _client.ListDirectory("/");
                        return directories != null; // La connexion est active si la liste des répertoires est récupérée
                    }
                }
                catch (Exception)
                {
                    // Capturer toute exception ici pour indiquer que la connexion n'est pas active
                    return false;
                }
                return false;
            });
        }

        public void StopCheckingSftpConnection()
        {
            _sftpConnectionCancellationTokenSource?.Cancel();
            _sftpConnectionCancellationTokenSource = null;
        }

        private void UpdateConnectionIcon(int state)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow.ChangeConnectionIcon(state);
                });
            }
            catch
            {

            }
        }

        public void Disconnect()
        {
            try
            {
                if (_client != null && _client.IsConnected)
                {
                    _client.Disconnect();
                    _client.Dispose();
                    _client = null;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
