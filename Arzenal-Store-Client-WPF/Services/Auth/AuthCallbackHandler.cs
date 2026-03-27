using Arzenal.StoreManager.Core.Interfaces;

namespace Arzenal.StoreManager.WPF.Services.Auth
{
    /*public class AuthCallbackHandler : IAuthCallbackHandler
    {
        public TaskCompletionSource<bool> TokenSource { get; set; } = new TaskCompletionSource<bool>();
        public void OnAuthSuccess()
        {
            // Notifier ViewModel Login
            TokenSource.TrySetResult(true);
        }
        public void OnAuthError(string message)
        {
            // Notifier ViewModel Login
            TokenSource.TrySetException(new Exception(message));
        }
        public Task OnUnauthorizedAsync()
        {
            // Effacer token
            // Notifier ViewModel Login
            return Task.CompletedTask;
        }

        public Task OnForbiddenAsync()
        {
            // notifier UI "Accès interdit"
            return Task.CompletedTask;
        }

        public Task OnServerErrorAsync()
        {
            // afficher message global
            return Task.CompletedTask;
        }
    }*/

}
