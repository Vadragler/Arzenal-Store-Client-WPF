using Arzenal_Store_Client_WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;


namespace Arzenal_Store_Client_WPF
{
    public partial class App : Application
    {

        private IServiceProvider _serviceProvider;
        public App()
        {
            _serviceProvider = ServiceLocator.ConfigureServices();
        }

        protected override void OnStartup(StartupEventArgs e)
        {


            base.OnStartup(e);

            try
            {
                if (e.Args.Length > 0)
                {
                    string url = e.Args[0]; // arzenalstore://auth?token=abc123  

                    // Parse l'URL  
                    var uri = new Uri(url);
                    var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    string token = queryParams["token"];




                    // Tu peux aussi stocker le token dans une propriété statique ou le passer à la fenêtre  
                    App.Current.Properties["Token"] = token;
                }

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur au démarrage : " + ex.Message + "\n" + ex.StackTrace, "Erreur");
            }
        }
    }
}

