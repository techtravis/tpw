using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.Auth.Enumerators
{
    public enum RoleClaims
    {
        AllowEditRssFeedRecord,
        AllowAddRssFeedRecord,
        AllowAddProfileImage,
        AllowChangeOwnProfile,
        AllowChangeOwnPassword
    }
}
