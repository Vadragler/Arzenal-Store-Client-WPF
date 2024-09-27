using Magasin_PC;
using Renci.SshNet;
using Renci.SshNet.Common; // Assurez-vous d'avoir installé la bibliothèque SSH.NET via NuGet
using System.Windows;

public class SftpManager
{
    private SftpClient _client;
    private CancellationTokenSource _sftpConnectionCancellationTokenSource;

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
            
                _client = new SftpClient(host, port, username, password);
                _client.Connect();


                // Change l'icône pour indiquer que la connexion a réussi
                UpdateConnectionIcon(1);
            }
        catch (SshConnectionException sshEx)
        {
                // Change l'icône pour indiquer une erreur de connexion
                UpdateConnectionIcon(-1);
                ShowErrorMessage($"Erreur de connexion SSH : {sshEx.Message}");
            }
        catch (SshAuthenticationException authEx)
        {
                // Change l'icône pour indiquer une erreur d'authentification
                UpdateConnectionIcon(-1);
                ShowErrorMessage($"Erreur d'authentification : {authEx.Message}");
            }
        catch (Exception ex)
        {
                // Change l'icône pour indiquer une erreur générale
                    UpdateConnectionIcon(-1);
                ShowErrorMessage($"Erreur lors de la connexion SFTP : {ex.Message}");
            }
        });
    }
    public void StartCheckingSftpConnection(string host, string username, string password, int port)
    {
        _sftpConnectionCancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => CheckSftpConnectionStatus(host, username, password, port, _sftpConnectionCancellationTokenSource.Token));
    }

    private async Task CheckSftpConnectionStatus(string host, string username, string password, int port, CancellationToken cancellationToken)
    {
        int retryCount = 0;
        const int maxRetries = 5;

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
                    retryCount = 0; // Réinitialiser le compteur
                }
            }
            else
            {
                // La connexion n'est pas active
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateConnectionIcon(-1); // Indiquer que la connexion est perdue
                });

                retryCount++;
                if (retryCount <= maxRetries)
                {
                    // Attendre 1 minute avant la prochaine tentative de reconnexion
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                    try
                    {
                        // Tenter de se reconnecter
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        ConnectAsync(host, username, password, port));
                        if (IsConnected)
                        {
                            // Si la reconnexion réussit, réinitialiser le compteur
                            retryCount = 0;
                        }
                        else
                        {
                            // Journaliser que la reconnexion a échoué
                            Console.WriteLine($"Tentative de reconnexion {retryCount}/{maxRetries} échouée.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Gérer l'échec de la reconnexion ici, loguer l'erreur
                        Console.WriteLine($"Échec de la reconnexion : {ex.Message}");
                    }
                }
                else
                {
                    // Arrêter la boucle après max tentatives
                    MessageBox.Show("La reconnexion SFTP a échoué après plusieurs tentatives.");
                    break;
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
            catch (Exception ex)
            {
                // Capturer toute exception ici pour indiquer que la connexion n'est pas active
                Console.WriteLine($"Erreur lors de la vérification de la connexion SFTP : {ex.Message}");
                return false;
            }
            return false;
        });
    }




    private async Task TryReconnectSftpAsync(string host, string username, string password, int port)
    {
        const int maxAttempts = 5;

        // Ne tente de te reconnecter que si tu n'es pas déjà connecté
        if (IsConnected) return;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await ConnectAsync(host, username, password, port);
                if (IsConnected)
                {
                    break; // Si la connexion réussit, sortir de la boucle
                }
            }
            catch
            {
                // Attendre une minute avant la prochaine tentative de reconnexion, sauf pour la dernière tentative
                if (attempt < maxAttempts)
                {
                    await Task.Delay(60000); // Attendre 1 minute
                }
                else
                {
                    MessageBox.Show("La connexion SFTP a échoué après plusieurs tentatives.");
                }
            }
        }
    }


    public void StopCheckingSftpConnection()
    {
        _sftpConnectionCancellationTokenSource?.Cancel();
        _sftpConnectionCancellationTokenSource = null;
    }

    private void UpdateConnectionIcon(int state)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.ChangeConnectionIcon(state);
        });
    }

    private void ShowErrorMessage(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(message);
        });
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
        catch (Exception ex)
        {
            // Gérer les erreurs lors de la déconnexion
            Console.WriteLine($"Erreur lors de la déconnexion SFTP : {ex.Message}");
            // Ici, tu peux ajouter une logique pour notifier l'utilisateur
        }
    }
}
