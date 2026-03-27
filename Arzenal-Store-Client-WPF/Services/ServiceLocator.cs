using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Services;
using Arzenal.StoreManager.Core.Services.Helpers;
using Arzenal.StoreManager.WPF.Services.Auth;
using Arzenal.StoreManager.WPF.ViewModel;
using Arzenal.StoreManager.WPF.Views.MainWindow;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Arzenal.StoreManager.WPF.Services
{
    public static class ServiceLocator
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Core / API
            services.AddHttpClient<IHttpClientService, HttpClientService>(client =>
            {
                client.BaseAddress = new Uri("https://arzenal.ovh");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    CookieContainer = AppCookieStore.Container,
                    UseCookies = true
                };
                return handler;
            });
            services.AddSingleton<IAppApiService, AppApiService>();
            services.AddSingleton<IAuthService, WpfAuthService>();


            // UI / WPF
            services.AddSingleton<IUiDispatcher, UiDispatcher>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IWindowService, WindowService>();

            // Dialog
            services.AddSingleton<IDialogService, DialogService>();

            // ViewModels
            services.AddSingleton<MainViewModel>();

            // Windows
            services.AddSingleton<MainWindow>();


            return services.BuildServiceProvider();
        }
    }

}
