using ChoSV.Models.Entities;
using System.Security.Claims;

namespace ChoSV.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

        Task SaveRefreshTokenAsync(string userId, string refreshToken, string? deviceInfo = null, string? ipAddress = null);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserRefreshTokensAsync(string userId);
    }
}
