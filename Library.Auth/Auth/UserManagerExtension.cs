using Library.Database.Auth;
using Library.Database.TableModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    public class UserManagerExtension : UserManager<SecureUser>
    {
        private readonly UserStore<SecureUser, IdentityRole, ApplicationDbContext, string, IdentityUserClaim<string>,
        IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
        _store;

        public UserManagerExtension(IUserStore<SecureUser> store, 
                                    IOptions<IdentityOptions> optionsAccessor, 
                                    IPasswordHasher<SecureUser> passwordHasher, 
                                    IEnumerable<IUserValidator<SecureUser>> userValidators, 
                                    IEnumerable<IPasswordValidator<SecureUser>> passwordValidators, 
                                    ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
                                    IServiceProvider services, ILogger<UserManager<SecureUser>> logger) : 
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _store = (UserStore<SecureUser, IdentityRole, ApplicationDbContext, string, IdentityUserClaim<string>,
            IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>)store;
        }

        public override Task<IdentityResult> UpdateAsync(SecureUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return UpdateUserAsync(user);
        }

        protected override async Task<IdentityResult> UpdateUserAsync(SecureUser user)
        {
            var result = await ValidateUserAsync(user);
            if (!result.Succeeded)
            {
                return result;
            }
            await UpdateNormalizedUserNameAsync(user);
            await UpdateNormalizedEmailAsync(user);

            //Implement Insert or Update of Refresh Token for audience
            var ctx = _store.Context;
            
            AspNetUserAudienceRefreshToken.InsertOrUpdate(ctx, user);

            return await Store.UpdateAsync(user, CancellationToken);
        }

        public async Task<AspNetUserAudienceRefreshToken?> GetUserTokenForAudience(SecureUser user, string? audience)
        {
            var ctx = _store.Context;
            if(audience== null)
            {
                return null;
            }
            return await Task.FromResult(AspNetUserAudienceRefreshToken.GetRefreshTokenForAudience(audience, ctx, user));
        }
    }
}
