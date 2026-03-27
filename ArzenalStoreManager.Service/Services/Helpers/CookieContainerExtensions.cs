using System.Collections;
using System.Net;
using System.Reflection;

namespace Arzenal.StoreManager.Core.Services.Helpers
{
    public static class CookieContainerExtensions
    {
        public static List<object> DirtyReadDomainTable(this CookieContainer container)
        {
            var field = typeof(CookieContainer)
                .GetField("m_domainTable", BindingFlags.NonPublic | BindingFlags.Instance);

            return ((IDictionary)field.GetValue(container))
                .Cast<DictionaryEntry>()
                .Select(d => d.Value)
                .ToList();
        }

        public static IEnumerable<Cookie> GetAllCookies(this CookieContainer container)
        {
            var domainEntries = container.DirtyReadDomainTable();

            foreach (var domainEntry in domainEntries)
            {
                var listField = domainEntry.GetType()
                    .GetField("m_list", BindingFlags.NonPublic | BindingFlags.Instance);

                var pathList = (IDictionary)listField.GetValue(domainEntry);

                foreach (var pathEntry in pathList.Values)
                {
                    foreach (Cookie cookie in (CookieCollection)pathEntry)
                        yield return cookie;
                }
            }
        }

        public static IEnumerable<AppCookieStore.SerializableCookie> GetAllSerializableCookies(
            this CookieContainer container)
        {
            return container.GetAllCookies().Select(c => new AppCookieStore.SerializableCookie
            {
                Name = c.Name,
                Value = c.Value,
                Path = c.Path,
                Domain = c.Domain,
                Expires = c.Expired ? null : c.Expires,
                Secure = c.Secure,
                HttpOnly = c.HttpOnly
            });
        }
    }

}
