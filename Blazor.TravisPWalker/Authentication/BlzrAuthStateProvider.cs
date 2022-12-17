using Library.Database.Auth;
using Library.Database.Auth.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace Blazor.TravisPWalker.Authentication
{
    public class BlzrAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ITokenService _tokenService;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public BlzrAuthStateProvider(ProtectedSessionStorage sessionStorage, ITokenService tokenService)
        {
            _sessionStorage = sessionStorage;
            _tokenService = tokenService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionStorageResult = await _sessionStorage.GetAsync<TokenViewModel>("UserSession");
                var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;
                // if not logged in set to anonymous
                if (userSession == null)
                {
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }

                // Feel like this should use the token service.. will circle back
                var claimsPrincipal = _tokenService.GetPrincipalFromToken(userSession.accessToken);
                var state = new AuthenticationState(claimsPrincipal == null ? new ClaimsPrincipal() : claimsPrincipal);
                NotifyAuthenticationStateChanged(Task.FromResult(state));
                return state;
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        public async Task UpdateAuthenticationState(TokenViewModel token)
        {
            ClaimsPrincipal? claimsPrincipal;

            if(token != null)
            {
                await _sessionStorage.SetAsync("UserSession", token);
                claimsPrincipal = _tokenService.GetPrincipalFromToken(token.accessToken);
            }
            else
            {
                await _sessionStorage.DeleteAsync("UserSession");
                claimsPrincipal = _anonymous;
            }

            var state = new AuthenticationState(claimsPrincipal == null ? new ClaimsPrincipal() : claimsPrincipal);
            NotifyAuthenticationStateChanged(Task.FromResult(state));            
        }
    }
}
