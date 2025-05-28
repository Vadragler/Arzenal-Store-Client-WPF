using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Arzenal_Store_Client_WPF.Services.API
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Vérifier si le token est disponible dans les propriétés de l'application
            if (App.Current.Properties.Contains("Token"))
            {
                var token = App.Current.Properties["Token"] as string;
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    // Log possible ici ou gestion spécifique selon le code
                    return default;
                }
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseData);
            }
            catch (HttpRequestException ex)
            {
                // Logguer l'erreur ou afficher un message
                Console.WriteLine($"Erreur HTTP : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur inconnue : {ex.Message}");
            }

            return default;
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    // Log possible ici ou gestion spécifique selon le code
                    return default;
                }
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseData);
            }
            catch (HttpRequestException ex)
            {
                // Logguer l'erreur ou afficher un message
                Console.WriteLine($"Erreur HTTP : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur inconnue : {ex.Message}");
            }
            return default;
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    // Log possible ici ou gestion spécifique selon le code
                    return default;
                }
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseData);
            }
            catch (HttpRequestException ex)
            {
                // Logguer l'erreur ou afficher un message
                Console.WriteLine($"Erreur HTTP : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur inconnue : {ex.Message}");
            }
            return default;
        }

        public async Task<T> PatchAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    // Log possible ici ou gestion spécifique selon le code
                    return default;
                }
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseData);
            }
            catch (HttpRequestException ex)
            {
                // Logguer l'erreur ou afficher un message
                Console.WriteLine($"Erreur HTTP : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur inconnue : {ex.Message}");
            }
            return default;
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    // Log possible ici ou gestion spécifique selon le code
                    return default;
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                // Logguer l'erreur ou afficher un message
                Console.WriteLine($"Erreur HTTP : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur inconnue : {ex.Message}");
            }
            return default;
        }
    }
}
