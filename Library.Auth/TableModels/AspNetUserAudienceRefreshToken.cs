using Library.Database.Auth;
using Library.Database.Auth.Models;
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
    [Index("UserId", nameof(Audience), IsUnique = true)]

    public class AspNetUserAudienceRefreshToken
    {
        [Key]
        public string Id { get; set; }

        [ForeignKey("UserId")]
        public SecureUser? SecureUser { get; set; }

        public string Audience { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }

        public static void InsertOrUpdate(ApplicationDbContext context, SecureUser secureUser)
        {
            // if the refresh token is null assume logout of all resources
            if(secureUser.RefreshToken== null)
            {
                var ids = from w in context.AspNetUserAudienceRefreshTokens where w.SecureUser.Id == secureUser.Id select w.Id;
                context.AspNetUserAudienceRefreshTokens.RemoveRange(from id in ids.AsEnumerable() select new AspNetUserAudienceRefreshToken { Id = id });
            }
            else
            {
                // else save or create refreshtoken record
                AspNetUserAudienceRefreshToken? refreshModel = context.AspNetUserAudienceRefreshTokens.FirstOrDefault(r => r.SecureUser.Id == secureUser.Id && r.Audience == secureUser.LastAudience);
                if (refreshModel == null)
                {
                    refreshModel = new AspNetUserAudienceRefreshToken()
                    {
                        Id = Guid.NewGuid().ToString(),
                        SecureUser = secureUser,
                        Audience = secureUser.LastAudience,
                        RefreshToken = secureUser.RefreshToken,
                        RefreshTokenExpiryTime = secureUser.RefreshTokenExpiryTime,
                    };

                    context.Add<AspNetUserAudienceRefreshToken>(refreshModel);
                }
                else
                {
                    context.AspNetUserAudienceRefreshTokens.Attach(refreshModel);
                    refreshModel.RefreshToken = secureUser.RefreshToken;
                    refreshModel.RefreshTokenExpiryTime = secureUser.RefreshTokenExpiryTime;
                }
            }
            
            context.SaveChanges();            
        }

        public static AspNetUserAudienceRefreshToken? GetRefreshTokenForAudience(string audience, ApplicationDbContext context, SecureUser secureUser)
        {
            AspNetUserAudienceRefreshToken? refreshModel = context.AspNetUserAudienceRefreshTokens.FirstOrDefault(r => r.SecureUser.Id == secureUser.Id && r.Audience == audience);
            return refreshModel;
        }
    }
}
