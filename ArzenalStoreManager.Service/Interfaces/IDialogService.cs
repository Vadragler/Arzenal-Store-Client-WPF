using Arzenal.Dto.DTOs.AppDto;
using Arzenal.StoreManager.Core.Models;

namespace Arzenal.StoreManager.Core.Interfaces
{
    public interface IDialogService
    {
        Task ShowConfirmationDialog(string message, Action<bool> onConfirm);
        Task<bool> ShowConfirmationDialogAsync(string message);
        Task<bool> ShowAppEditDialogAsync(ReadAppDto app);

        Task<bool> ShowSettingsDialogAsync();
    }
}
