using Microsoft.Win32;

namespace MagasinPC
{
    public class FileService
    {
        public string? SelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Zip files (*.zip)|*.zip";
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }
    }
}
