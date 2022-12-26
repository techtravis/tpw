using Library.Database.Auth.Models;
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
        ClaimsPrincipal? GetPrincipalFromToken(string? token, bool validateAudience = true, bool notExpired = true);
        TokenViewModel? GetTokenForAudience(string curToken, string curRefreshToken, string audience);
    }
}
