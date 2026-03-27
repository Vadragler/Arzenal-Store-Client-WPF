using System.Security.Cryptography;
using System.Text;

namespace Arzenal.StoreManager.Core.Services.Helpers
{
    public class DeviceHelper
    {
        public static string GenerateFingerprint()
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
    }
}
