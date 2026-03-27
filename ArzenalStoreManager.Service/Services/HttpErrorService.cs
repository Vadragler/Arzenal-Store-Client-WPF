using Arzenal.StoreManager.Core.Services.Helpers;
// Ajoutez les using nécessaires pour HttpError si ce type existe dans un autre namespace
// using Arzenal.StoreManager.Core.Models; // <-- décommentez et ajustez si besoin

namespace Arzenal.StoreManager.Core.Services
{
    public static class HttpErrorService
    {
        // Ajoutez l'événement OnError si ce n'est pas déjà fait ailleurs
        public static event Action<HttpError>? OnError;

        public static void UnauthorizedAccess()
        {
            AppSessionState.Instance.IsConnected = false;
            OnError?.Invoke(HttpError.Unauthorized());
        }

        public static void BadRequest(string? message)
             => OnError?.Invoke(HttpError.BadRequest(message));
        

        public static void Duplicate(string? message)
            => OnError?.Invoke(HttpError.Conflict(message));

        public static void NotFound(string? message)
            => OnError?.Invoke(HttpError.NotFound(message));

        public static void Forbidden(string? message)
            => OnError?.Invoke(HttpError.Forbidden(message));

        public static void ServerError(int code,string? message)
            => OnError?.Invoke(HttpError.Server(code, message));

    }

    public record HttpError(
    int Code,
    string Title,
    string Message)
    {
        public static HttpError Unauthorized()
            => new(401, "Non autorisé", "Session expirée.");

        public static HttpError Forbidden(string? msg)
            => new(403, "Accès refusé", msg ?? "Droits insuffisants.");

        public static HttpError BadRequest(string? msg)
            => new(400, "Requête invalide", msg ?? "Données incorrectes.");

        public static HttpError Conflict(string? msg)
            => new(409, "Conflit", msg ?? "Élément déjà existant.");

        public static HttpError NotFound(string? msg)
            => new(404, "Introuvable", msg ?? "Ressource absente.");

        public static HttpError Server(int code, string? msg)
            => new(code, "Erreur serveur", msg ?? "Erreur interne.");
    }

}
