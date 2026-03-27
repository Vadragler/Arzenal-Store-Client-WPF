using Arzenal.StoreManager.Core.Services.Helpers;
using System.Net;
using Xunit;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class CookieContainerExtensionsTests
    {
        [Fact]
        public void GetAllSerializableCookies_ReturnsCookies()
        {
            var container = new CookieContainer();
            container.Add(new Cookie("n1", "v1", "/", "example.com"));
            container.Add(new Cookie("n2", "v2", "/", "example.com"));

            var list = container.GetAllSerializableCookies().ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, c => c.Name == "n1" && c.Value == "v1");
            Assert.Contains(list, c => c.Name == "n2" && c.Value == "v2");
        }
    }
}
