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
        private readonly UserManager<Library.Database.Auth.SecureUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthenticateController(
            UserManager<Library.Database.Auth.SecureUser> userManager,
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
                var accesstoken = _tokenService.CreateToken(authClaims, model.Audience);
                var refreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_tokenService.GetRefreshTokenValidityDays());

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

            var newAccessToken = _tokenService.CreateToken(principal.Claims.ToList(), audience);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

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

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            string? username = principal.Identity?.Name;

            var user = await _userManager.FindByNameAsync(username == null ? "" : username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            var newAccessToken = _tokenService.CreateToken(principal.Claims.ToList(), audience);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

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
            if (User.IsInRole("God"))
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
        public async Task<IActionResult> AuthCHeck()
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



    }
}
