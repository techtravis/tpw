using Library.Auth.TableModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Library.Auth
{
    public class SecureUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        // these ICollections are needed in order to create the foreign keys for the below tables
        public ICollection<ImageStore> ImageStores { get; set; }
        public ICollection<AspNetUserHomePage> AspNetUserHomePage { get; set; }

        //public static bool CanDoX(this IPrincipal principal)
        //{
        //    return ((ClaimsIdentity)principal.Identity).HasClaim(claimType, claimValue);
        //}


    }
}
