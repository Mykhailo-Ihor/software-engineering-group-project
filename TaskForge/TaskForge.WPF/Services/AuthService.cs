using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
//using IdentityModel.OidcClient;
using System;
using System.Threading.Tasks;

namespace TaskForge.WPF
{
    public class Auth0Service
    {
        private readonly Auth0Client _auth0Client;

        public Auth0Service()
        {
            // Замініть на ваші дані з Auth0 Dashboard
            var options = new Auth0ClientOptions
            {
                Domain = "dev-4o6nraj4dugki1zl.us.auth0.com", // Наприклад: myapp.eu.auth0.com
                ClientId = "dbNBZAMnuH3rYesM83BmaK1VTQgeayzL",
                RedirectUri = "http://localhost/callback",
                PostLogoutRedirectUri = "http://localhost",
                Scope = "openid profile email"
            };

            _auth0Client = new Auth0Client(options);
        }

        /// <summary>
        /// Логін користувача (відкриває браузер для Auth0 Universal Login)
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