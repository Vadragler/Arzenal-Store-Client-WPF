using Arzenal.StoreManager.Core.Services.Helpers;
using System.Net;
using System.Reflection;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class AppCookieStoreTests
    {
        [Fact]
        public void SaveAndLoad_PreservesCookies()
        {
            // Prepare temp file
            var tempFile = Path.Combine(Path.GetTempPath(), $"cookies_{Guid.NewGuid()}.json");

            // Set private static CookieFile field to temp file
            var fileField = typeof(AppCookieStore).GetField("CookieFile", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(fileField);
            fileField.SetValue(null, tempFile);

            // Create a cookie container with one cookie
            var container = new CookieContainer();
            var cookie = new Cookie("name", "value", "/", "example.com") { HttpOnly = true, Secure = true };
            container.Add(cookie);

            // Set the static Container property via reflection
            var prop = typeof(AppCookieStore).GetProperty("Container", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(prop);
            prop.SetValue(null, container);

            // Save to file
            AppCookieStore.Save();
            Assert.True(File.Exists(tempFile));

            // Clear container and load
            prop.SetValue(null, new CookieContainer());
            AppCookieStore.Load();

            // Check cookie exists
            var loaded = AppCookieStore.Container.GetCookies(new Uri("https://example.com"));
            Assert.Contains(loaded.Cast<Cookie>(), c => c.Name == "name" && c.Value == "value");

            // Cleanup
            try { File.Delete(tempFile); } catch { }
        }
    }
}
