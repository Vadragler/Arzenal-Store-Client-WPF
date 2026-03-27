using Arzenal.StoreManager.Core.Interfaces;
using System.Collections.ObjectModel;
using static Arzenal.StoreManager.Core.Services.HttpClientService;

namespace Arzenal.StoreManager.Core.Services.Helpers
{
    public class AppApiService : IAppApiService
    {
        private readonly IHttpClientService _httpClient;

        public AppApiService(IHttpClientService httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task EnsureTokenAsync()
        {
            await _httpClient.CheckConnectionAsync();
        }

        public async Task<HttpResult<T>> PostWithFullResponseAsync<T>(string endpoint, object body, string token)
        {
            _httpClient.SetBearerToken(token);
            return await _httpClient.PostWithFullResponseAsync<T>(endpoint, body);
        }
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            await EnsureTokenAsync();
            return await _httpClient.GetAsync<T>(endpoint);
        }
        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            await EnsureTokenAsync();
            return await _httpClient.PostAsync<T>(endpoint, data);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            await EnsureTokenAsync();
            return await _httpClient.PutAsync<T>(endpoint, data);
        }

        public async Task<T?> PatchAsync<T>(string endpoint, object data)
        {
            await EnsureTokenAsync();
            return await _httpClient.PatchAsync<T>(endpoint, data);
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            await EnsureTokenAsync();
            return await _httpClient.DeleteAsync(endpoint);
        }

        public async Task<ObservableCollection<T>> GetCollectionAsync<T>(string endpoint)
        {
            await EnsureTokenAsync();
            var items = await _httpClient.GetAsync<T[]>(endpoint);
            return new ObservableCollection<T>(items ?? Array.Empty<T>());
        }
    }
}
