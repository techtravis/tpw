using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
