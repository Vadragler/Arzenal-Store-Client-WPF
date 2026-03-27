using Arzenal.Dto.DTOs.AuthDto;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Services.Helpers;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static Arzenal.StoreManager.Core.Services.HttpClientService;

namespace Arzenal.StoreManager.WPF.Services.Auth
{
    public class WpfAuthService : IAuthService
    {
        private readonly IAppApiService _apiService;
        private readonly IHttpClientService _httpClient;
        public TaskCompletionSource<bool> CookiesReady { get; } = new TaskCompletionSource<bool>();
        public event Action<bool> ConnectionStateChanged = delegate { };

        public WpfAuthService(IAppApiService apiService, IHttpClientService httpClient)
        {
            _apiService = apiService;
            _httpClient = httpClient;
        }

        public async Task<bool> AuthenticateAsync()
        {
            string redirectUri = $"arzenal://callback/";
            var uriBuilder = new UriBuilder("https://arzenal.ovh/login");
            var query = HttpUtility.ParseQueryString(string.Empty);

            query["redirect_uri"] = redirectUri;
            query["Id"] = "ArzenalStoreManager";

            uriBuilder.Query = query.ToString();
            string authUrl = uriBuilder.ToString();


            // Ouvre le navigateur pour login
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
            return true;
        }

        private string GenerateFingerprint()
        {
            var cpuId = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "";
            var os = Environment.OSVersion.Version;
            var osString = $"{os.Major}.{os.Minor}";
            var machineName = Environment.MachineName;
            var raw = $"{cpuId}-{osString}-{machineName}";

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(hash);
        }

        public async Task ExchangeTokenForCookiesAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            Guid userId = userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;

            var deviceInfo = new CreateRefreshTokenDto
            {
                UserId = userId,
                DeviceName = Environment.MachineName,
                Fingerprint = GenerateFingerprint(),
                UserAgent = "ArzenalStoreManager",
                CreatedByIp = await new System.Net.Http.HttpClient().GetStringAsync("https://api.ipify.org")
            };
            var cookieHeaders = await _apiService.PostWithFullResponseAsync<HttpResult<object>>("api/auth/GenerateRefreshToken", deviceInfo, token);

            // Récupère les cookies
            var cookieContainer = new CookieContainer();
            if (cookieHeaders.Headers != null)
            {
                foreach (var header in cookieHeaders.Headers.GetValues("Set-Cookie"))
                    cookieContainer.SetCookies(new Uri("https://arzenal.ovh"), header);
            }

            lock (AppCookieStore.Container)
            {
                foreach (Cookie c in cookieContainer.GetCookies(new Uri("https://arzenal.ovh")))
                    AppCookieStore.Container.Add(new Uri("https://arzenal.ovh"), c);
            }

            AppCookieStore.Save();

            CookiesReady.TrySetResult(true);
            await _httpClient.CheckConnectionAsync();
            ConnectionStateChanged?.Invoke(true);
        }
    }
}
