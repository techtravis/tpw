using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.Auth
{
    public interface IAuthConfigManager
    {
        string MainConnection { get; }
        string JwtSecret { get; }
        string JwtIssuer { get; }
        string JwtAudience { get; }
        string JwtValidMinutes { get; }
        string JwtRefreshTokenValidityInDays { get; }
    }
}
