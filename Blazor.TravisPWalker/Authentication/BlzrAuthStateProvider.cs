using Blazored.LocalStorage;
using Library.Database.Auth;
using Library.Database.Auth.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
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

        /// <summary>
        /// Oh boy.   So when we get the user session.  The session can be null.  If it is we want to check local storage for a "Cookie"
        /// And build the session from that if the refresh token on the cookie is valid
        /// If the user session is not null.  Then we want refresh the token if it has expired...  if this fails because the token is invalid
        /// Then we need to get the refresh token from the local storage if it is present and try to refresh the token, session, cookie this way
        /// 
        /// This in theory should allow the user state to persist between tabs and browser instances.
        /// So much so that if the user somehow triggers a refresh on one tab or window. The next time the other window has to reauthenticate.
        /// The local storage in theory should have the most up to date token in the local storage.
        /// </summary>
        /// <returns></returns>
        public async Task<TokenViewModel> GetUserSessionAsync()
        {
            try
            {
                var userSessionStorageResult = await _sessionStorage.GetAsync<TokenViewModel>("UserSession");
                var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;                
                // if not logged in set to anonymous
                if (userSession == null)
                {
                    TokenViewModel? tvm = await UpdateAuthWithLocalStorageCookie();
                    if (tvm != null)
                    {
                        userSession = new TokenViewModel();
                        userSession.expiration = tvm.expiration;
                        userSession.accessToken = tvm.accessToken;
                        userSession.refreshToken = tvm.refreshToken;
                        return await Task.FromResult(userSession);
                    }

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
                        else
                        {
                            TokenViewModel? tvm =  await UpdateAuthWithLocalStorageCookie();
                            if(tvm != null)
                            {
                                userSession.expiration = tvm.expiration;
                                userSession.accessToken = tvm.accessToken;
                                userSession.refreshToken = tvm.refreshToken;
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
                userCookie.expires = token.refreshTokenExpiryTime.ToString();

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

        private async Task<TokenViewModel?> UpdateAuthWithLocalStorageCookie()
        {
            string? Token = await _localStorage.GetItemAsync<string>("token");
            UserCookieViewModel? userCookie = JsonSerializer.Deserialize<UserCookieViewModel>(Token);

            string blazorUrl = _configuration.GetValue<string>("URLS:blazor");
            UserToken userToken = new UserToken() { AccessToken = userCookie?.AccessToken, RefreshToken = userCookie?.RefreshToken, Audience = blazorUrl };

            HttpClient client = new HttpClient();

            string apiUrl = _configuration.GetValue<string>("URLS:api");
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
                    if (userCookie == null) { userCookie = new UserCookieViewModel(); }
                    userCookie.AccessToken = token.accessToken;
                    userCookie.RefreshToken = token.refreshToken;
                    userCookie.expires = token.refreshTokenExpiryTime.ToString();

                    await UpdateAuthenticationState(token);
                    return token;
                }
            }
            return null;
        }
    }
}
