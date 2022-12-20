using Library.Database.Auth;
using Library.Database.Auth.TableModels;
using Library.Database.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using TravisPWalker.Models;
using Library.Database.Auth.Models;
using System.Text.Json;
using System;

namespace TravisPWalker.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<Library.Database.Auth.SecureUser> _userManager;        
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _ApplicationDbContext;


        public HomeController(
            UserManager<Library.Database.Auth.SecureUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<HomeController> logger,
            ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
            _ApplicationDbContext= applicationDbContext;
        }


        public IActionResult Index()
        {
            var aspNetUserHomePage = _ApplicationDbContext.AspNetUserHomePage.First(p => p.SecureUser == null);
            HomePageEditViewModel hpvm = new HomePageEditViewModel(aspNetUserHomePage.HomeHtml);

            return View(hpvm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secured()
        {
            return View(User?.Claims);
        }

        [Authorize]
        public IActionResult BlazorSite()
        {
            string audience = _configuration.GetValue<string>("URLS:blazor");
            string? token = HttpContext?.Session.GetString("AccessToken");
            string? refreshToken = HttpContext?.Session.GetString("RefreshToken");
            UserToken userToken = new UserToken() { AccessToken = token, RefreshToken = refreshToken, Audience = audience };

            HttpClient client = new HttpClient();

            string apiUrl = _configuration.GetValue<string>("URLS:api");
            string usermodel = JsonSerializer.Serialize(userToken);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(usermodel);
            var content = new ByteArrayContent(messageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = client.PostAsync($"{apiUrl}/api/Authenticate/newtoken", content).Result;
            string result="";
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
                string blazorUrl = _configuration.GetValue<string>("URLS:blazor");
                return Redirect($"{blazorUrl}/tokenaccess?Token={result}&RedirectURL=fetchdata");
            }

            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [Route("/Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpGet]
        public IActionResult SecureLanding()
        {
            string? token = HttpContext?.Session.GetString("AccessToken");

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
                            if(claim.Type == "exp")
                            {
                                DateTime expireTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                expireTime = expireTime.AddSeconds(Double.Parse(claim.Value));
                                result += $"</br>Your Claim for {claim.Type} is: {expireTime} UTC | {expireTime.ToLocalTime()} LOCAL | epoch:{claim.Value}";
                            }
                            else
                            {
                                result += $"</br>Your Claim for {claim.Type} is: {claim.Value}";
                            }
                            
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

        public ActionResult HomeEdit()
        {
            HomePageEditViewModel model = new HomePageEditViewModel();
            model.Html = AspNetUserHomePage.GetHtml(_ApplicationDbContext, User);

            return View(model);
        }

        [HttpPost]
        public ActionResult HomeEdit(HomePageEditViewModel model)
        {
            // write html to the database for the user
            AspNetUserHomePage.InsertOrUpdate(model.Html, _ApplicationDbContext, User);

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Swagger()
        {
            string swaggerUrl = _configuration.GetValue<string>("URLS:apiswagger");
            return Redirect(swaggerUrl);
        }

        //need to move this to the api once that is setup...
        [HttpPost]
        public ActionResult UploadImage()
        {
            var files = Request.Form.Files;
            if(files.Count > 0)
            {
                var file = files[0];
                string newFileID = ImageStore.InsertAndProvideId(_ApplicationDbContext, User, _userManager, file);

                FileUploadResultViewModel uploadResult = new FileUploadResultViewModel
                {
                    uploaded = String.IsNullOrEmpty(newFileID)?0:1,
                    fileName = newFileID,
                    url = String.IsNullOrEmpty(newFileID)?"":$"/Home/GetImage?imageId={newFileID}"
                };
                return Json(uploadResult);
            }
            else
            {
                FileUploadResultViewModel uploadResult = new FileUploadResultViewModel
                {
                    uploaded = 0,
                    fileName = "",
                    url = ""
                };
                return Json(uploadResult);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public FileResult GetImage(string imageId)
        {
            ImageStore? image = ImageStore.GetImage(_ApplicationDbContext, imageId);
            if(image != null)
            {
                return File(image.Bytes, image.MimeType, $"{image.Name}{image.Extension}");
            }
            else
            {
                return null;
            }            
        }

    }
}