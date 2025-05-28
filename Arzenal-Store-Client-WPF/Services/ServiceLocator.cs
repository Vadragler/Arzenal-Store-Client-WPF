using Arzenal_Store_Client_WPF.Services.API;
using Arzenal_Store_Client_WPF.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Arzenal_Store_Client_WPF.Services
{
    public static class ServiceLocator
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5181");
            });
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<MainViewModel>();

            return services.BuildServiceProvider();
        }
    }

}
