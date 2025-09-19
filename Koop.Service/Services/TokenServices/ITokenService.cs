using Koop.Entity.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Koop.Service.Services.TokenService
{
    public interface ITokenService
    {
        Task<string> CreateTokenStringAsync(AppUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}