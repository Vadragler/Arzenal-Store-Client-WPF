using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Services;
using Arzenal.StoreManager.Core.Services.Helpers;
using Arzenal.StoreManager.WPF.Services;
using Arzenal.StoreManager.WPF.Views.MainWindow;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.IO.Pipes;
using System.Web;
using System.Windows;


namespace Arzenal.StoreManager.WPF
{
    public partial class App : Application
    {

        private IServiceProvider _serviceProvider;
        private static Mutex? _mutex;
        public App()
        {
            _serviceProvider = ServiceLocator.ConfigureServices();
        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            
            const string mutexName = "ArzenalStore_WPF_SingleInstance";

            _mutex = new Mutex(true, mutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                // Application déjà ouverte → envoyer l’URI à l’instance existante
                SendArgsToExistingInstance(string.Join(" ", e.Args));
                Shutdown();
                return;
            }
            HttpErrorService.OnError += ShowHttpError;
            StartPipeServer();
            ProtocolRegistrar.EnsureProtocolRegistered();
            AppCookieStore.Load();
            var httpClient = _serviceProvider.GetRequiredService<IHttpClientService>();
            await httpClient.CheckConnectionAsync();

            base.OnStartup(e);
            

            if (e.Args.Length > 0)
                await HandleProtocolArgsAsync(e.Args[0]);

            if (e.Args.Length > 0 && e.Args[0].StartsWith("arzenal://"))
            {
                await HandleProtocolArgsAsync(e.Args[0]);
            }


            // Affiche la fenêtre principale
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            
            mainWindow.Show();
        }
        private void ShowHttpError(HttpError error)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    error.Code.ToString() +" "+
                    error.Message,
                    error.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });

        }

        private void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using var server = new NamedPipeServerStream("ArzenalPipe", PipeDirection.In);
                    server.WaitForConnection();

                    using var reader = new StreamReader(server);
                    string msg = reader.ReadToEnd();

                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        await HandleProtocolArgsAsync(msg);
                    });
                }
            });
        }


        private void SendArgsToExistingInstance(string args)
        {
            using var client = new NamedPipeClientStream(".", "ArzenalPipe", PipeDirection.Out);
            client.Connect(100);

            using var writer = new StreamWriter(client);
            writer.Write(args);
        }

        private async Task HandleProtocolArgsAsync(string uri)
        {
            var uriObj = new Uri(uri);
            var token = HttpUtility.ParseQueryString(uriObj.Query).Get("token");
            var wpfAuthService = _serviceProvider.GetRequiredService<IAuthService>();
            await wpfAuthService.ExchangeTokenForCookiesAsync(token);
        }
    }
}

