using Library.Database.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.Auth
{
    // Don't know why I always get confused with Roles and Claims...
    // But in case I have a brain fart again..
    // good description at  https://stackoverflow.com/questions/50401190/asp-net-core-identity-use-aspnetuserclaims-or-aspnetroleclaims
    public static class Seeder
    {
        public static async Task SeedRolesAsync(UserManagerExtension userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            foreach(var role in Enum.GetValues(typeof(Enumerators.Roles)).Cast<Enumerators.Roles>())
            {
                await roleManager.CreateAsync(new IdentityRole(role.ToString()));
            }

            var user = userManager.FindByNameAsync("techtravis@gmail.com").Result;
            await userManager.AddToRoleAsync(user, Library.Database.Auth.Enumerators.Roles.Advanced.ToString());
            await userManager.AddToRoleAsync(user, Library.Database.Auth.Enumerators.Roles.God.ToString());
        }

        public static async Task SeedRoleClaimsAsync(UserManagerExtension userManager, RoleManager<IdentityRole> roleManager)
        {
            // when we make RoleClaims there is no built in method to prevent duplicats
            // so we need to take a little extra care by getting the list of claims for the role
            // before deciding if we need to insert the seed
            // this is kinda  messy,  will need to circle back and clean it up..

            // Get the roles
            List<IdentityRole> roles = roleManager.Roles.ToList();

            // Get the BasicRole
            IdentityRole BasicRole = (IdentityRole)(roles.Where(r => r.Name == Enumerators.Roles.Basic.ToString()).Single());
            
            // Get all the claims for the BasicRole
            List<Claim> BasicClaims = roleManager.GetClaimsAsync(BasicRole).Result.ToList();

            // check if the claim exists, if not insert it
            Claim? changeOwnPassword = BasicClaims.Where(c => c.Value == Enumerators.RoleClaims.AllowChangeOwnPassword.ToString()).SingleOrDefault();
            if(changeOwnPassword == null)
            {
                await roleManager.AddClaimAsync(
                BasicRole,
                new Claim(ClaimTypes.AuthorizationDecision, Enumerators.RoleClaims.AllowChangeOwnPassword.ToString())
                );
            }

            Claim? changeOwnProfile = BasicClaims.Where(c => c.Value == Enumerators.RoleClaims.AllowChangeOwnProfile.ToString()).SingleOrDefault();
            if (changeOwnProfile == null)
            {
                await roleManager.AddClaimAsync(
                BasicRole,
                new Claim(ClaimTypes.AuthorizationDecision, Enumerators.RoleClaims.AllowChangeOwnProfile.ToString())
                );
            }

            Claim? addProfileImage = BasicClaims.Where(c => c.Value == Enumerators.RoleClaims.AllowAddProfileImage.ToString()).SingleOrDefault();
            if (addProfileImage == null)
            {
                await roleManager.AddClaimAsync(
                BasicRole,
                new Claim(ClaimTypes.AuthorizationDecision, Enumerators.RoleClaims.AllowAddProfileImage.ToString())
                );
            }

            // Get the AdvandedRole
            IdentityRole AdvancedRole = (IdentityRole)(roles.Where(r => r.Name == Enumerators.Roles.Advanced.ToString()).Single());

            // Get all the claims for the BasicRole
            List<Claim> AdvancedClaims = roleManager.GetClaimsAsync(AdvancedRole).Result.ToList();
            Claim? addRssFeedRecord = AdvancedClaims.Where(c => c.Value == Enumerators.RoleClaims.AllowAddRssFeedRecord.ToString()).SingleOrDefault();
            if (addRssFeedRecord == null)
            {
                await roleManager.AddClaimAsync(
                AdvancedRole,
                new Claim(ClaimTypes.AuthorizationDecision, Enumerators.RoleClaims.AllowAddRssFeedRecord.ToString())
                );
            }


            // Get the GodRole
            IdentityRole GodRole = (IdentityRole)(roles.Where(r => r.Name == Enumerators.Roles.God.ToString()).Single());

            // Get all teh claims for the GodRole
            List<Claim> GodClaims = roleManager.GetClaimsAsync(GodRole).Result.ToList();
            Claim? editRssFeedRecord = GodClaims.Where(c => c.Value == Enumerators.RoleClaims.AllowEditRssFeedRecord.ToString()).SingleOrDefault();
            if (editRssFeedRecord == null)
            {
                await roleManager.AddClaimAsync(
                GodRole,
                new Claim(ClaimTypes.AuthorizationDecision, Enumerators.RoleClaims.AllowEditRssFeedRecord.ToString())
                );
            }

        }
    }
}


