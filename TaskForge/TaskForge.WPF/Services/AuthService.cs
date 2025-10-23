using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using System;
using System.Threading.Tasks;

namespace TaskForge.WPF
{
    public class Auth0Service
    {
        private readonly Auth0Client _auth0Client;

        public Auth0Service()
        {
            var options = new Auth0ClientOptions
            {
                Domain = "dev-ki8p3p3wo311vi24.us.auth0.com",
                ClientId = "o9CH63Qr9LfNfY5uKFlaz9Z3NOfUjRVc",
                RedirectUri = "http://localhost:7890/callback",
                PostLogoutRedirectUri = "http://localhost:7890",
            };

            _auth0Client = new Auth0Client(options);
        }

        /// <summary>
        /// Логін користувача
        /// </summary>
        public async Task<LoginResult> LoginAsync()
        {
            try
            {
                var loginResult = await _auth0Client.LoginAsync();
                return loginResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка при логіні: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Вихід користувача
        /// </summary>
        public async Task<BrowserResultType> LogoutAsync()
        {
            try
            {
                var logoutResult = await _auth0Client.LogoutAsync();
                return logoutResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка при виході: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Отримання інформації про користувача
        /// </summary>
        public string GetUserEmail(LoginResult loginResult)
        {
            return loginResult?.User?.FindFirst(c => c.Type == "email")?.Value;
        }

        public string GetUserName(LoginResult loginResult)
        {
            return loginResult?.User?.FindFirst(c => c.Type == "name")?.Value;
        }

        public string GetUserId(LoginResult loginResult)
        {
            return loginResult?.User?.FindFirst(c => c.Type == "sub")?.Value;
        }

        public string GetAccessToken(LoginResult loginResult)
        {
            return loginResult?.AccessToken;
        }
    }
}