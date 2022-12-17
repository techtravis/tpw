using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.Auth
{
    public class UserToken
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Audience { get; set; }
    }
}
