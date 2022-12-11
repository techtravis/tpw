﻿using Library.Auth;
using Library.Auth.TableModels;
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

namespace TravisPWalker.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<SecureUser> _userManager;        
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _ApplicationDbContext;


        public HomeController(
            UserManager<SecureUser> userManager,
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

        private async Task<byte[]> GetByteArrayFromImageAsync(IFormFile file)
        {
            using (var target = new MemoryStream())
            {
                await file.CopyToAsync(target);
                return target.ToArray();
            }
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