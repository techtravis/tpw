using Library.Database.Auth.TableModels;
using Library.Database.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Library.Database.TableModels;

namespace Library.Database.Auth
{
    public class ApplicationDbContext : IdentityDbContext<Library.Database.Auth.SecureUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AspNetUserHomePage> AspNetUserHomePage { get; set; }
        public DbSet<ImageStore> ImageStore { get; set; }
        public DbSet<LedFeed> LedFeed { get; set; }
        public DbSet<InterviewPrepQuestion> InterviewPrepQuestion { get; set; }
        public DbSet<AppException> AppException { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AspNetUserHomePage>().ToTable("AspNetUserHomePage");
            builder.Entity<ImageStore>().ToTable("ImageStore");
            builder.Entity<LedFeed>().ToTable("LedFeed");
            builder.Entity<InterviewPrepQuestion>().ToTable("InterviewPrepQuestion");
            builder.Entity<AppException>().ToTable("AppException");
        }
    }

}
