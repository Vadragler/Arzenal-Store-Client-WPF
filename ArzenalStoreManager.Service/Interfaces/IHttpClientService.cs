using static Arzenal.StoreManager.Core.Services.HttpClientService;

namespace Arzenal.StoreManager.Core.Interfaces
{
    public interface IHttpClientService
    {
        Task<bool> CheckConnectionAsync();
        Task<T?> GetAsync<T>(string endpoint);
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<HttpResult<T>> PostWithFullResponseAsync<T>(string endpoint, object data);
        Task<T?> PutAsync<T>(string endpoint, object data);
        Task<T?> PatchAsync<T>(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);
        void SetBearerToken(string token);
    }
}
