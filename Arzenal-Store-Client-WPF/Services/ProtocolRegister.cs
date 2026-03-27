using Microsoft.Win32;

namespace Arzenal.StoreManager.WPF.Services
{
    

    public static class ProtocolRegistrar
    {
        private const string ProtocolName = "arzenal";

        public static void EnsureProtocolRegistered()
        {
            // Vérifie si la clé existe déjà
            using var key = Registry.ClassesRoot.OpenSubKey(ProtocolName);
            // déjà enregistré

            string exePath = Environment.ProcessPath!;
            if (key != null)
                return;
            using var newKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProtocolName}");
            newKey.SetValue("", "URL:Arzenal Protocol");
            newKey.SetValue("URL Protocol", "");

            using var shellKey = newKey.CreateSubKey("shell");
            using var openKey = shellKey.CreateSubKey("open");
            using var commandKey = openKey.CreateSubKey("command");
            commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
        }
    }

}
