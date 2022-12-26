using Library.Database.Auth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Library.Database.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IAuthConfigManager _authConfigManager;
        private string JwtSecret;
        private string JwtIssuer;
        private string[] JwtAudiences;
        private string JwtValidMinutes;
        private string JwtRefreshTokenValidityInDays;

        public TokenService(IAuthConfigManager authConfigManager)
        {
            _authConfigManager = authConfigManager;

            JwtSecret = _authConfigManager.JwtSecret;
            JwtIssuer = _authConfigManager.JwtIssuer;
            JwtAudiences = _authConfigManager.JwtAudiences;
            JwtValidMinutes = _authConfigManager.JwtValidMinutes;
            JwtRefreshTokenValidityInDays = _authConfigManager.JwtRefreshTokenValidityInDays;
        }


        public ClaimsPrincipal? GetPrincipalFromToken(string? token, bool validateAudience = true, bool notExpired = true)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = notExpired,
                ValidateAudience = (validateAudience && notExpired)?true:false,
                ValidateLifetime = notExpired,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidAudiences = JwtAudiences,
                ValidIssuer = JwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch 
            {
                throw new SecurityTokenException("Invalid token");
            }            
        }

        public TokenViewModel? GetTokenForAudience(string curToken, string curRefreshToken, string audience)
        {
            UserToken userToken = new UserToken() { AccessToken = curToken, RefreshToken = curRefreshToken, Audience = audience };

            HttpClient client = new HttpClient();
            string usermodel = JsonSerializer.Serialize(userToken);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(usermodel);
            var content = new ByteArrayContent(messageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = client.PostAsync($"{JwtIssuer}/api/Authenticate/newtoken", content).Result;
            string result = "";
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
                TokenViewModel? tokenModel = JsonSerializer.Deserialize<TokenViewModel>(result);

                return tokenModel;
            }
            else
            {
                TokenViewModel? tokenModel = RefreshToken(curToken, curRefreshToken, audience);

                return tokenModel;
            }
        }

        private TokenViewModel? RefreshToken(string curToken, string curRefreshToken, string audience)
        {
            UserToken userToken = new UserToken() { AccessToken = curToken, RefreshToken = curRefreshToken, Audience = audience };

            HttpClient client = new HttpClient();
            string usermodel = JsonSerializer.Serialize(userToken);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(usermodel);
            var content = new ByteArrayContent(messageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = client.PostAsync($"{JwtIssuer}/api/Authenticate/refresh-token", content).Result;
            string result = "";
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
                TokenViewModel? tokenModel = JsonSerializer.Deserialize<TokenViewModel>(result);

                return tokenModel;
            }
            return null;
        }

    }
}
