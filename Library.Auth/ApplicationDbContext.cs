using Library.Auth.TableModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Library.Auth
{
    public class ApplicationDbContext : IdentityDbContext<SecureUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AspNetUserHomePage> AspNetUserHomePage { get; set; }
        public DbSet<ImageStore> ImageStore { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AspNetUserHomePage>().ToTable("AspNetUserHomePage");
            builder.Entity<ImageStore>().ToTable("ImageStore");
        }
    }

}
