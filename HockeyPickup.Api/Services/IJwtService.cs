using System.Security.Claims;

namespace HockeyPickup.Api.Services;

public interface IJwtService
{
    string GenerateToken(string userId, string username);
    bool ValidateToken(string token);
    ClaimsPrincipal GetPrincipalFromToken(string token);
    DateTime GetTokenExpiration(string token);
}
