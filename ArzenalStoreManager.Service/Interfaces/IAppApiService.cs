using System.Collections.ObjectModel;
using static Arzenal.StoreManager.Core.Services.HttpClientService;

namespace Arzenal.StoreManager.Core.Interfaces
{
    public interface IAppApiService
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task <HttpResult<T>> PostWithFullResponseAsync<T>(string endpoint, object body, string token);
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<T?> PutAsync<T>(string endpoint, object data);
        Task<T?> PatchAsync<T>(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);
        Task<ObservableCollection<T>> GetCollectionAsync<T>(string endpoint);
    }

}
