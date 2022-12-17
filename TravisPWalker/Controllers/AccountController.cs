using Library.Database.Auth;
using Library.Database.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace TravisPWalker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Library.Database.Auth.SecureUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AccountController(
            UserManager<Library.Database.Auth.SecureUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<AccountController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //[AllowAnonymous]
        //[HttpPost]
        //public IActionResult Login2(LoginModel userModel)
        //{
        //    var user = _userManager.FindByNameAsync(userModel.UserName).Result;
        //    if (user != null && _userManager.CheckPasswordAsync(user, userModel.Password).Result)
        //    {
        //        var userRoles = _userManager.GetRolesAsync(user).Result;

        //        var authClaims = new List<Claim>
        //        {
        //            new Claim(ClaimTypes.Name, user.UserName),
        //            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //            //DOH forgot about NameIdentifier claim that sets the userid and is required for things like _userManager.GetUserAsync(User)  
        //            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        };

        //        // lets add the claims for the roles the user is in
        //        foreach (var userRole in userRoles)
        //        {
        //            // the actual role claim
        //            authClaims.Add(new Claim(ClaimTypes.Role, userRole));

        //            // the claims associated to the role
        //            var role = _roleManager.FindByNameAsync(userRole).Result;
        //            if (role != null)
        //            {
        //                var roleClaims = _roleManager.GetClaimsAsync(role).Result;
        //                foreach (Claim roleClaim in roleClaims)
        //                {
        //                    authClaims.Add(roleClaim);
        //                }
        //            }
        //        }

        //        // take all those claims and now wrap it into the token.
        //        var token = _tokenService.CreateToken(authClaims, _configuration.GetValue<string>("JWT:Audience"));
        //        var refreshToken = _tokenService.GenerateRefreshToken();

        //        user.RefreshToken = refreshToken;
        //        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_tokenService.GetRefreshTokenValidityDays());

        //        _ = _userManager.UpdateAsync(user).Result;

        //        // set the tokens
        //        _httpContextAccessor.HttpContext?.Session.Set("AccessToken", Encoding.ASCII.GetBytes(new JwtSecurityTokenHandler().WriteToken(token)));
        //        _httpContextAccessor.HttpContext?.Session.Set("RefreshToken", Encoding.ASCII.GetBytes(refreshToken));
        //        _httpContextAccessor.HttpContext?.Session.Set("RefreshTokenExpiryTime", Encoding.ASCII.GetBytes(user.RefreshTokenExpiryTime.ToString()));

        //        return Redirect("/Home/SecureLanding");
        //    }
        //    return Unauthorized();
        //}


        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(LoginModel userModel)
        {
            if(userModel.UserName != null && userModel.Password != null)
            {
                userModel.Audience = _configuration.GetValue<string>("JWT:Audience");
                HttpClient client = new HttpClient();

                string apiUrl = _configuration.GetValue<string>("URLS:api");
                string usermodel = JsonSerializer.Serialize(userModel);
                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(usermodel);
                var content = new ByteArrayContent(messageBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var response = client.PostAsync($"{apiUrl}/api/Authenticate/login", content).Result;
                string result;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                    TokenViewModel? token = JsonSerializer.Deserialize<TokenViewModel>(result);
                    if(token != null)
                    {
                        _httpContextAccessor.HttpContext?.Session.Set("AccessToken", Encoding.ASCII.GetBytes(token.accessToken));
                        _httpContextAccessor.HttpContext?.Session.Set("RefreshToken", Encoding.ASCII.GetBytes(token.refreshToken));
                        _httpContextAccessor.HttpContext?.Session.Set("RefreshTokenExpiryTime", Encoding.ASCII.GetBytes(token.expiration));
                        return Redirect("/Home/SecureLanding");
                    }                    
                }                
            }            

            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Logout(string? returnUrl = null)
        {
            string? token = HttpContext?.Session.GetString("AccessToken");
            ClaimsPrincipal? principal = _tokenService.GetPrincipalFromExpiredToken(token);
            if (principal != null)
            {
                var user = _userManager.FindByNameAsync(principal.Identity?.Name).Result;
                if (user == null) return BadRequest("Invalid user name");

                user.RefreshToken = null;
                _ = _userManager.UpdateAsync(user).Result;

                _httpContextAccessor.HttpContext?.Session.Set("AccessToken", Encoding.ASCII.GetBytes(""));
                _httpContextAccessor.HttpContext?.Session.Set("RefreshToken", Encoding.ASCII.GetBytes(""));
            }

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage("Home/Index");
            }
        }


    }
}
