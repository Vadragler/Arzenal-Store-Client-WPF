namespace Arzenal.StoreManager.Core.Interfaces
{
    public interface IAuthService
    {
        public Task<bool> AuthenticateAsync();

        public Task ExchangeTokenForCookiesAsync(string token);

        event Action<bool> ConnectionStateChanged;

    }
}
