using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.Auth
{
    public class AuthConfigManager : IAuthConfigManager
    {
        private readonly IConfiguration _configuration;
        public AuthConfigManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string MainConnection
        {
            get
            {
                return _configuration["ConnectionStrings:Main"];
            }
        }

        public string JwtSecret
        {
            get
            {
                return _configuration["JWT:Secret"];
            }
        }
        public string JwtIssuer
        {
            get
            {
                return _configuration["JWT:Issuer"];
            }
        }
        public string JwtAudience
        {
            get
            {
                return _configuration["JWT:Audience"];
            }
        }
        public string JwtValidMinutes
        {
            get
            {
                return _configuration["JWT:ValidMinutes"];
            }
        }
        public string JwtRefreshTokenValidityInDays
        {
            get
            {
                return _configuration["JWT:RefreshTokenValidityInDays"];
            }
        }
    }
}
