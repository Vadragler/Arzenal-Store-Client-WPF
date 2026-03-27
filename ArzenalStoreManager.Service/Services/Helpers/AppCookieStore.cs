using System.Net;
using System.Text.Json;

namespace Arzenal.StoreManager.Core.Services.Helpers
{
    public static class AppCookieStore
    {
        private static string CookieFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArzenalStore", "cookies.json");

        public static CookieContainer Container { get; private set; } = new CookieContainer();

        public static void Load()
        {
            if (!File.Exists(CookieFile))
                return;

            var json = File.ReadAllText(CookieFile);
            var cookies = JsonSerializer.Deserialize<List<SerializableCookie>>(json);

            var newContainer = new CookieContainer();

            if (cookies != null)
            {
                foreach (var c in cookies)
                {
                    var cookie = new Cookie(c.Name, c.Value, c.Path, c.Domain)
                    {
                        Secure = c.Secure,
                        HttpOnly = c.HttpOnly
                    };

                    // Only set Expires when it has a meaningful value
                    if (c.Expires.HasValue && c.Expires.Value > DateTime.MinValue)
                        cookie.Expires = c.Expires.Value;

                    newContainer.Add(cookie);
                }
            }

            Container = newContainer;
        }

        public static void Save()
        {
            var dir = Path.GetDirectoryName(CookieFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var cookies = Container.GetAllSerializableCookies();
            var json = JsonSerializer.Serialize(cookies, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(CookieFile, json);
        }

        public class SerializableCookie
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Path { get; set; }
            public string Domain { get; set; }
            public DateTime? Expires { get; set; }
            public bool Secure { get; set; }
            public bool HttpOnly { get; set; }
        }
    }
}
