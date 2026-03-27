namespace Arzenal.StoreManager.Core.Interfaces
{
    public interface ITokenService
    {
        string? GetJwt();
        Task<string?> RefreshJwtAsync();
        bool IsJwtExpired(string? token);
    }
}
