using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IAuthConfigManager _authConfigManager;
        private string JwtSecret;
        private string JwtIssuer;
        private string JwtAudience;
        private string JwtValidMinutes;
        private string JwtRefreshTokenValidityInDays;

        public TokenService(IAuthConfigManager authConfigManager)
        {
            _authConfigManager = authConfigManager;

            JwtSecret = _authConfigManager.JwtSecret;
            JwtIssuer = _authConfigManager.JwtIssuer;
            JwtAudience = _authConfigManager.JwtAudience;
            JwtValidMinutes = _authConfigManager.JwtValidMinutes;
            JwtRefreshTokenValidityInDays = _authConfigManager.JwtRefreshTokenValidityInDays;
        }



        public JwtSecurityToken CreateToken(List<Claim> authClaims, string audience)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
            _ = int.TryParse(JwtValidMinutes, out int tokenValidityInMinutes);

            var token = new JwtSecurityToken(
                issuer: JwtIssuer,
                audience: audience,
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string? token, bool validateAudience = true)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = validateAudience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidAudience = JwtAudience,
                ValidIssuer = JwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public int GetRefreshTokenValidityDays()
        {
            int days = 0;
            int.TryParse(JwtRefreshTokenValidityInDays, out days);
            return days;
        }

    }
}
