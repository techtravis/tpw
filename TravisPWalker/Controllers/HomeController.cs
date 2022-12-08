using Library.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using TravisPWalker.Models;

namespace TravisPWalker.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<SecureUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;


        public HomeController(
            UserManager<SecureUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secured()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpGet]
        public IActionResult SecureLanding()
        {
            string? token = HttpContext?.Session.GetString("Token");

            if (token == null)
            {
                return (RedirectToAction("Index"));
            }

            ViewBag.Message = BuildTokenMessage(token);
            return View();
        }

        private string BuildTokenMessage(string token)
        {
            string result = "";

            ClaimsPrincipal? principal = _tokenService.GetPrincipalFromExpiredToken(token);
            if (principal != null)
            {
                var user = _userManager.FindByNameAsync(principal.Identity?.Name).Result;
                if (user == null) { result = "Invalid user name"; } else
                {
                    result += $"Your Username is: {user.UserName}";

                    if(principal.Claims.Count() > 0)
                    {
                        foreach(var claim in principal.Claims)
                        {
                            result += $"</br>Your Claim for {claim.Type} is: {claim.Value}";
                        }
                    }                    

                    result += "</br>The generated token is:";

                    var data = Enumerable.Range(0, token.Length / 50)
                            .Select(i => token.Substring(i * 50, 50));

                    foreach (string str in data)
                    {
                        result += "</br>" + str;
                    }
                }


            }


            return result;
        }

        private string BuildDetailedMessage(string stringToSplit, int chunkSize)
        {
            var data = Enumerable.Range(0, stringToSplit.Length / chunkSize)
                .Select(i => stringToSplit.Substring(i * chunkSize, chunkSize));

            string result = "The generated token is:";

            foreach (string str in data)
            {
                result += Environment.NewLine + str;
            }

            return result;
        }
    }
}