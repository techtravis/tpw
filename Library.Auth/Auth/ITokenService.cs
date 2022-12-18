using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.Auth
{
    public interface ITokenService
    {
        JwtSecurityToken CreateToken(List<Claim> authClaims, string audience);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
        ClaimsPrincipal? GetPrincipalFromToken(string? token, bool validateAudience = true);
        int GetRefreshTokenValidityDays();
    }
}
