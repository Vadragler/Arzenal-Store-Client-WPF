using Arzenal.StoreManager.Core.Interfaces;
using Microsoft.Win32;

namespace Arzenal.StoreManager.WPF.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? SelectFile()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            return null;
        }

        public string? SelectImage()
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.ico)|*.png;*.jpg;*.jpeg;*.ico";
            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            return null;
        }

        public string? PickZipFile()
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
