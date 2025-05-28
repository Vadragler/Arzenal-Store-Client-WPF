namespace Arzenal_Store_Client_WPF.Services
{
    public interface IDialogService
    {
        Task ShowConfirmationDialog(string message, Action<bool> onConfirm);
    }

    public class DialogService : IDialogService
    {
        public async Task ShowConfirmationDialog(string message, Action<bool> onConfirm)
        {
            var confirmationWindow = new ConfirmationWindow(); // Utilise ta propre fenêtre de confirmation

            // On suppose que tu as un contrôle TextBlock ou autre pour afficher le message
            //confirmationWindow.MessageText.Text = message;

            // Attendre le résultat de la fenêtre
            var result = confirmationWindow.ShowDialog();

            // Si l'utilisateur a confirmé, renvoyer true, sinon false
            onConfirm(result ?? false);
        }
    }

}
