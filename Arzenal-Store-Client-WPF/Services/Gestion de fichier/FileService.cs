using Microsoft.Win32;

namespace Arzenal_Store_Client_WPF
{
    public class FileService
    {
        public string? SelectFile()
        {

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Zip files (*.zip)|*.zip"
            };

            // ShowDialog is inherently synchronous, so we use Task.Run to avoid blocking the UI thread
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }
    }
}
