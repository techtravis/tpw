using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Auth
{
    public class UserToken
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
