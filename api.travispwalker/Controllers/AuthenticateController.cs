using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Library.Database.Auth.Models;
using Library.Database.Auth;
using System.Text.Json;

namespace api.travispwalker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManagerExtension _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthenticateController(
            UserManagerExtension userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            model.Audience = model.Audience == null ? _configuration.GetValue<string>("JWT:ValidAudience") : model.Audience;

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = _userManager.GetRolesAsync(user).Result;

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    //DOH forgot about NameIdentifier claim that sets the userid and is required for things like _userManager.GetUserAsync(User)  
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                };

                // lets add the claims for the roles the user is in
                foreach (var userRole in userRoles)
                {
                    // the actual role claim
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                    // the claims associated to the role
                    var role = _roleManager.FindByNameAsync(userRole).Result;
                    if (role != null)
                    {
                        var roleClaims = _roleManager.GetClaimsAsync(role).Result;
                        foreach (Claim roleClaim in roleClaims)
                        {
                            authClaims.Add(roleClaim);
                        }
                    }
                }

                // take all those claims and now wrap it into the token.
                var accesstoken = CreateToken(authClaims, model.Audience);
                var refreshToken = GenerateRefreshToken();

                user.LastAudience = model.Audience;
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(GetRefreshTokenValidityDays());

                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(accesstoken),
                    RefreshToken = refreshToken,
                    Expiration = accesstoken.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("newtoken")]
        public async Task<IActionResult> NewToken(UserToken tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;
            string? audience = tokenModel.Audience;

            var principal = _tokenService.GetPrincipalFromToken(accessToken, false);
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            string? username = principal.Identity?.Name;

            var user = await _userManager.FindByNameAsync(username == null ? "" : username);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid refresh token");
            }

            var newAccessToken = CreateToken(principal.Claims.ToList(), audience);
            var newRefreshToken = GenerateRefreshToken();

            user.LastAudience = tokenModel.Audience;
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken,
                Expiration = newAccessToken.ValidTo
            });
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(UserToken tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;
            string? audience = tokenModel.Audience;

            var principal = _tokenService.GetPrincipalFromToken(accessToken, false, false);
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            string? username = principal.Identity?.Name;

            var user = await _userManager.FindByNameAsync(username == null ? "" : username);
            var audienceToken = await _userManager.GetUserTokenForAudience(user, tokenModel.Audience);

            if (user == null || audienceToken?.RefreshToken != refreshToken || audienceToken?.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            var newAccessToken = CreateToken(principal.Claims.ToList(), audience);
            var newRefreshToken = GenerateRefreshToken();

            user.LastAudience = tokenModel.Audience;
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken,
                Expiration = newAccessToken.ValidTo
            });
        }

        [Authorize]
        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            if (User.IsInRole("God") || User.Identity?.Name == username)
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null) return BadRequest("Invalid user name");

                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);
                return NoContent();
            }
            else
            {
                return StatusCode(403);
            }            
        }

        [Authorize]
        [HttpPost]
        [Route("revoke-all")]
        public async Task<IActionResult> RevokeAll()
        {
            if (User.IsInRole("God"))
            {
                var users = _userManager.Users.ToList();
                foreach (var user in users)
                {
                    user.RefreshToken = null;
                    await _userManager.UpdateAsync(user);
                }
                return NoContent();
            }
            else
            {
                return StatusCode(403);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("auth-check")]
        public async Task<IActionResult> AuthCheck()
        {
            if(User.IsInRole("God") || User.IsInRole("Admin") || User.IsInRole("Advanced") || User.IsInRole("Basic"))
            {
                return StatusCode(202);
            }
            else
            {
                return StatusCode(403);
            }

        }


        private JwtSecurityToken CreateToken(List<Claim> authClaims, string audience)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWT:Secret")));
            _ = int.TryParse(_configuration.GetValue<string>("JWT:ValidMinutes"), out int tokenValidityInMinutes);

            //remove old aud claims which will now be duplicates..
            var audClaims = authClaims.Where(a => a.Type == "aud").ToList();
            foreach(var claim in audClaims)
            {
                authClaims.Remove(claim);
            }

            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("JWT:Issuer"),
                audience: audience,
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private int GetRefreshTokenValidityDays()
        {
            int days = 0;
            int.TryParse(_configuration.GetValue<string>("JWT:RefreshTokenValidityInDays"), out days);
            return days;
        }

    }
}
