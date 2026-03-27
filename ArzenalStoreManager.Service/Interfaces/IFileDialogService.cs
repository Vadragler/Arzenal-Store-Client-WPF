namespace Arzenal.StoreManager.Core.Interfaces
{
    public interface IFileDialogService
    {
        string? SelectFile();
        string? SelectImage();
        string? PickZipFile();
    }
}
