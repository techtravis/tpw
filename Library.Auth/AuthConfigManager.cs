using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Auth
{
    public class AuthConfigManager: IAuthConfigManager
    {
        private readonly IConfiguration _configuration;
        public AuthConfigManager(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public string MainConnection
        {
            get
            {
                return this._configuration["ConnectionStrings:Main"];
            }
        }

        public string JwtSecret {
            get
            {
                return this._configuration["JWT:Secret"];
            }
        }
        public string JwtIssuer {
            get
            {
                return this._configuration["JWT:ValidIssuer"];
            }
        }
        public string JwtValidAudience {
            get
            {
                return this._configuration["JWT:ValidAudience"];
            }
        }
        public string JwtValidMinutes {
            get
            {
                return this._configuration["JWT:ValidMinutes"];
            }
        }
        public string JwtRefreshTokenValidityInDays {
            get
            {
                return this._configuration["JWT:RefreshTokenValidityInDays"];
            }
        }
    }
}
