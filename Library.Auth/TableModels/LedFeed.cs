using Library.Database.Auth;
using Library.Database.Auth.TableModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.TableModels
{
    public class LedFeed
    {
        [Key]
        public string LedMessageId { get; set; }
        [ForeignKey("AddedById")]
        public SecureUser? AddedByUser { get; set; }
        public string Name { get; set; } = "";
        public string DisplayText { get; set; } = "";
        public Int16? EventYear { get; set; } = 2023;
        public Int16? EventMonth { get; set; } = 11;
        public Int16? EventDay { get; set; } = 1;
        public Int16 Row { get; set; } = 0;
        public bool Active { get; set; } = true;
        public DateTime? ShowUntil { get; set; }

        public static void InsertOrUpdate(LedFeed ledFeed, ApplicationDbContext context, ClaimsPrincipal secureUser)
        {
            if (secureUser.IsInRole("God"))
            {
                LedFeed ledFeedMatch = context.LedFeed.FirstOrDefault(l => l.LedMessageId == ledFeed.LedMessageId);
                if (ledFeedMatch == null)
                {
                    ledFeedMatch = ledFeed;
                    context.Add<LedFeed>(ledFeedMatch);
                }
                else
                {
                    context.LedFeed.Attach(ledFeedMatch);
                    ledFeedMatch.Name = ledFeed.Name;
                    ledFeedMatch.DisplayText = ledFeed.DisplayText;
                    ledFeedMatch.EventYear= ledFeed.EventYear;
                    ledFeedMatch.EventMonth= ledFeed.EventMonth;
                    ledFeedMatch.EventDay= ledFeed.EventDay;
                    ledFeedMatch.Row = ledFeed.Row;
                    ledFeedMatch.Active = ledFeed.Active;
                    ledFeedMatch.ShowUntil = ledFeed.ShowUntil;
                }
                context.SaveChanges();                
            }
        }

        public static LedFeed? GetById(string id, ApplicationDbContext context, ClaimsPrincipal secureUser)
        {
            LedFeed? ledFeed = context.LedFeed.FirstOrDefault(l => l.LedMessageId == id);
            return ledFeed;
        }
    }
}
