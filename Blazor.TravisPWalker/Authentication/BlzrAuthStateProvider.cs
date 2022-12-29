using Blazored.LocalStorage;
using Library.Database.Auth;
using Library.Database.Auth.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Text.Json;

namespace Blazor.TravisPWalker.Authentication
{
    public class BlzrAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private Blazored.LocalStorage.ILocalStorageService _localStorage;

        public BlzrAuthStateProvider(ProtectedSessionStorage sessionStorage, ITokenService tokenService, IConfiguration configuration, ILocalStorageService localStorage)
        {
            // Session storage is browser tab specific
            // this means opening a new tab and navigating to a page will not retain authentication state.
            _sessionStorage = sessionStorage;
            _tokenService = tokenService;
            _configuration = configuration;
            _localStorage= localStorage;
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
                else
                {
                    // check if the token has expired, if so lets refresh it
                    DateTime current = DateTime.Now;
                    DateTime expiry = DateTime.Parse(userSession.expiration);
                    if(current > expiry)
                    {
                        string blazorUrl = _configuration.GetValue<string>("URLS:blazor");
                        string apiUrl = _configuration.GetValue<string>("URLS:api");

                        UserToken userToken = new UserToken() { AccessToken = userSession.accessToken, RefreshToken = userSession.refreshToken, Audience = blazorUrl };
                        HttpClient client = new HttpClient();                        
                        string usermodel = JsonSerializer.Serialize(userToken);
                        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(usermodel);
                        var content = new ByteArrayContent(messageBytes);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                        var response = client.PostAsync($"{apiUrl}/api/Authenticate/refresh-token", content).Result;
                        string result;
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            TokenViewModel? token = JsonSerializer.Deserialize<TokenViewModel>(result);
                            if (token != null)
                            {                                
                                await UpdateAuthenticationState(token);
                                userSession.accessToken = token.accessToken;
                            }
                        }
                    }
                }

                // should probably call the API instead of the library directly. But just showing it can work this way as well
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

        public async Task<TokenViewModel> GetUserSessionAsync()
        {
            try
            {
                var userSessionStorageResult = await _sessionStorage.GetAsync<TokenViewModel>("UserSession");
                var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;                
                // if not logged in set to anonymous
                if (userSession == null)
                {
                    return await Task.FromResult(new TokenViewModel());
                }
                else
                {
                    // check if the token has expired, if so lets refresh it
                    DateTime current = DateTime.Now;
                    DateTime expiry = DateTime.Parse(userSession.expiration);
                    if (current > expiry)
                    {
                        string blazorUrl = _configuration.GetValue<string>("URLS:blazor");
                        string apiUrl = _configuration.GetValue<string>("URLS:api");

                        UserToken userToken = new UserToken() { AccessToken = userSession.accessToken, RefreshToken = userSession.refreshToken, Audience = blazorUrl };
                        HttpClient client = new HttpClient();
                        string usermodel = JsonSerializer.Serialize(userToken);
                        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(usermodel);
                        var content = new ByteArrayContent(messageBytes);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                        var response = client.PostAsync($"{apiUrl}/api/Authenticate/refresh-token", content).Result;
                        string result;
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            TokenViewModel? token = JsonSerializer.Deserialize<TokenViewModel>(result);
                            if (token != null)
                            {
                                await UpdateAuthenticationState(token);
                                userSession.expiration = token.expiration;
                                userSession.accessToken = token.accessToken;
                                userSession.refreshToken = token.refreshToken;
                            }
                        }
                    }
                }

                return await Task.FromResult(userSession);
            }
            catch
            {
                return await Task.FromResult(new TokenViewModel());
            }
        }

        public async Task UpdateAuthenticationState(TokenViewModel token)
        {
            ClaimsPrincipal? claimsPrincipal;

            if(token != null)
            {
                UserCookieViewModel userCookie = new UserCookieViewModel();
                userCookie.AccessToken = token.accessToken;
                userCookie.RefreshToken = token.refreshToken;
                userCookie.RefreshTokenExpiryTime = token.expiration;

                await _localStorage.SetItemAsync("token", userCookie);
                await _sessionStorage.SetAsync("UserSession", token);                
                claimsPrincipal = _tokenService.GetPrincipalFromToken(token.accessToken);
            }
            else
            {
                await _localStorage.ClearAsync();
                await _sessionStorage.DeleteAsync("UserSession");
                claimsPrincipal = _anonymous;
            }

            var state = new AuthenticationState(claimsPrincipal == null ? new ClaimsPrincipal() : claimsPrincipal);
            NotifyAuthenticationStateChanged(Task.FromResult(state));            
        }
    }
}
