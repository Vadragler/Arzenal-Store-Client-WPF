using Arzenal.Dto.DTOs.FileDto;
using Arzenal.StoreManager.Core.Dtos;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Services.Helpers;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Arzenal.StoreManager.Core.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _client;

        public HttpClientService(HttpClient client)
        {
            _client = client;
            _client.DefaultRequestHeaders.Remove("X-Device-Name");
            _client.DefaultRequestHeaders.Remove("Fingerprint");
            _client.DefaultRequestHeaders.Remove("User-Agent");
            _client.DefaultRequestHeaders.Add("X-Device-Name", Environment.MachineName);
            _client.DefaultRequestHeaders.Add("Fingerprint", DeviceHelper.GenerateFingerprint());
            _client.DefaultRequestHeaders.Add("User-Agent", "ArzenalStoreManager");
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                _client.DefaultRequestHeaders.Remove("X-Device-Name");
                _client.DefaultRequestHeaders.Remove("Fingerprint");
                _client.DefaultRequestHeaders.Remove("User-Agent");
                _client.DefaultRequestHeaders.Add("X-Device-Name", Environment.MachineName);
                _client.DefaultRequestHeaders.Add("Fingerprint", DeviceHelper.GenerateFingerprint());
                _client.DefaultRequestHeaders.Add("User-Agent", "ArzenalStoreManager");
                var debug = await _client.GetAsync("api/Health/debug/claims");
                if (debug.IsSuccessStatusCode)
                {
                    var claims = await debug.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Claims: {claims}");
                }
                var response = await _client.GetAsync("api/auth/ping"); // un endpoint simple 200 OK
                if (response.IsSuccessStatusCode)
                {
                    AppSessionState.Instance.IsConnected = true;
                    return true;
                }
                else
                {
                    response = await _client.PostAsync("api/auth/refresh", null);
                    if (response.IsSuccessStatusCode)
                    {

                        response = await _client.GetAsync("api/auth/ping"); // un endpoint simple 200 OK
                        if (response.IsSuccessStatusCode)
                        {
                            AppSessionState.Instance.IsConnected = true;
                            return true;
                        }
                    }
                }
            }
            catch
            {
                
            }

            AppSessionState.Instance.IsConnected = false;
            return false;
        }


        public void SetBearerToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            if (typeof(T) == typeof(Stream))
            {
                return await GetFileStreamAsync<T>(endpoint);
            }

            var response = await _client.GetAsync(endpoint);
            return await HandleResponse<T>(response);
        }

        private async Task<T> GetFileStreamAsync<T>(string endpoint)
        {
            var response = await _client.GetAsync(endpoint);
            HandleHttpError(response);
            if (response.IsSuccessStatusCode)
            {
                var contentStream = await response.Content.ReadAsStreamAsync();
                return (T)(object)contentStream;
            }
            return default!;
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            if (data is UploadFileClientDto upload)
                return await PostFileStreamAsync<T>(upload);

            var response = await _client.PostAsJsonAsync(endpoint, data);
            return await HandleResponse<T>(response);
        }

        private async Task<T?> PostFileStreamAsync<T>(UploadFileClientDto dto)
        {
            UploadFileDto fileDto = dto.ApiDto;
            var fileNameEncoded = Uri.EscapeDataString(fileDto.FileName);
            // Construire l'URL avec les infos de route
            var endpoint = $"api/appfiles/upload/apps/{fileDto.AppId}/versions/{fileDto.Version}/files/{fileDto.Type}/filename/{fileNameEncoded}";
            if (!string.IsNullOrEmpty(fileDto.Platform))
                endpoint += $"/{fileDto.Platform}";

            using var stream = File.OpenRead(dto.LocalPath);
            Console.WriteLine($"Taille du fichier : {stream.Length} bytes");

            using var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await _client.PostAsync(endpoint, content);
            return await HandleResponse<T>(response);
        }



        public async Task<HttpResult<T>> PostWithFullResponseAsync<T>(string endpoint, object data)
        {
            var response = await _client.PostAsJsonAsync(endpoint, data);
            return await HandleFullResponse<T>(response);
        }


        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            var response = await _client.PutAsJsonAsync(endpoint, data);
            return await HandleResponse<T>(response);
        }

        public async Task<T?> PatchAsync<T>(string endpoint, object data)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
            {
                Content = JsonContent.Create(data)
            };

            var response = await _client.SendAsync(request);
            return await HandleResponse<T>(response);
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            var response = await _client.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }

        private static async Task<T?> HandleResponse<T>(HttpResponseMessage response)
        {
            HandleHttpError(response);
            if(response.IsSuccessStatusCode)
            {
            // Lire le contenu brut
            var content = await response.Content.ReadAsStringAsync();

            // Si vide → renvoyer default
            if (string.IsNullOrWhiteSpace(content))
                return default;

            // Si ce n'est pas du JSON → renvoyer default
            if (!content.TrimStart().StartsWith("{") &&
                !content.TrimStart().StartsWith("["))
                return default;

                try
                {
                    return JsonSerializer.Deserialize<T>(content);
                }
                catch
                {
                    // Le JSON ne correspond PAS au modèle attendu
                    return default;
                }
            }
            return default;
        }


        private static async Task<HttpResult<T>> HandleFullResponse<T>(HttpResponseMessage response)
        {
            var result = new HttpResult<T>
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers
            };
            if (response.IsSuccessStatusCode)
            {
                // Vérifie si le body existe
                if (response.Content != null && response.Content.Headers.ContentLength > 0)
                {
                    try
                    {
                        result.Body = await response.Content.ReadFromJsonAsync<T>();
                    }
                    catch (Exception ex)
                    {
                        // Si le JSON est vide ou incorrect, renvoie null
                        Debug.WriteLine($"Erreur JSON: {ex}");
                        result.Body = default;
                    }
                }
            }
            return result;
        }

        private static void HandleHttpError(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var code = (int)response.StatusCode;

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:    // 401
                    HttpErrorService.UnauthorizedAccess();
                    break;
                case HttpStatusCode.Forbidden:        // 403
                    HttpErrorService.Forbidden(response.ReasonPhrase);
                    break;
                case HttpStatusCode.NotFound:         // 404
                    HttpErrorService.NotFound(response.ReasonPhrase);
                    break;
                case HttpStatusCode.BadRequest:       // 400
                    HttpErrorService.BadRequest(response.ReasonPhrase);
                    break;
                default:
                    HttpErrorService.ServerError(code, response.ReasonPhrase);
                    break;
            }
        }


        public class HttpResult<T>
        {
            public T? Body { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public HttpResponseHeaders Headers { get; set; } = null!;
        }

        public class ResponseWithHeaders<T>
        {
            public T? Data { get; set; }
            public HttpResponseHeaders Headers { get; set; } = null!;
            public HttpStatusCode StatusCode { get; set; }
        }
    }
}
