using Library.Database.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Library.Database.TableModels
{
    public class AspNetUserHomePage
    {
        [Key]
        public string Id { get; set; }

        // requires a public ICollection Reference to this class on SecureUser in order for ef to create the foreign key
        [ForeignKey("UserId")]
        public SecureUser? SecureUser { get; set; }
        public string HomeHtml { get; set; } = "";

        public static void InsertOrUpdate(string html, ApplicationDbContext context, ClaimsPrincipal secureUser)
        {
            if (secureUser.IsInRole("God"))
            {
                AspNetUserHomePage? homePage = context.AspNetUserHomePage.FirstOrDefault(p => p.SecureUser == null);
                if (homePage == null)
                {
                    homePage = new AspNetUserHomePage() { Id = new Guid().ToString(), HomeHtml = html };
                }
                context.AspNetUserHomePage.Attach(homePage);
                homePage.HomeHtml = html;
                context.SaveChanges();
            }
        }

        public static string GetHtml(ApplicationDbContext context, ClaimsPrincipal secureUser)
        {
            if (secureUser.IsInRole("God"))
            {
                AspNetUserHomePage? homePage = context.AspNetUserHomePage.FirstOrDefault(p => p.SecureUser == null);
                return homePage?.HomeHtml?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
