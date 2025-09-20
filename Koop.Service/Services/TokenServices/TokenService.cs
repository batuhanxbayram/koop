using Koop.Entity.Entities;
using Koop.Service.Services.TokenServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Service.Services.TokenService
{  
        public class TokenService : ITokenService
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly TokenSettings _settings;

            public TokenService(IOptions<TokenSettings> options, UserManager<AppUser> userManager)
            {
                _userManager = userManager;
                _settings = options.Value;
            }

            public async Task<string> CreateTokenStringAsync(AppUser user)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(_settings.TokenValidityMinutes),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            public string GenerateRefreshToken()
            {
                var randomNumber = new byte[64];
                using var generator = RandomNumberGenerator.Create();
                generator.GetBytes(randomNumber);

                return Convert.ToBase64String(randomNumber);
            }

            public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
                    ValidateLifetime = false // Süresi dolduğu için lifetime kontrolü kapalı
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Geçersiz token.");
                }

                return principal;
            }
        }
    }



