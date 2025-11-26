using System;
using System.Threading.Tasks;
using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.WPF.Common;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.PasswordManager
{
    /// <summary>
    /// Command to load passwords for the authenticated user
    /// </summary>
    public class LoadPasswordsCommand : AsyncRelayCommand
    {
        private readonly PasswordManagerViewModel _viewModel;
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult? _loginResult;

        public LoadPasswordsCommand(
            PasswordManagerViewModel viewModel,
            IPasswordService passwordService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult? loginResult)
            : base(async _ => await ExecuteLoadPasswordsAsync(viewModel, passwordService, userService, auth0Service, loginResult))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _loginResult = loginResult;
        }

        private static async Task ExecuteLoadPasswordsAsync(
            PasswordManagerViewModel viewModel,
            IPasswordService passwordService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult? loginResult)
        {
            // Check if login result is valid
            if (loginResult == null || loginResult.IsError)
            {
                MessageBox.Show(
                    "Будь ласка, увійдіть, щоб переглянути паролі.",
                    "Потрібна автентифікація",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Get Auth0 user ID
            var auth0UserId = auth0Service.GetUserId(loginResult);

            // Fetch user from database
            var user = await userService.GetUserByAuth0IdAsync(auth0UserId);

            if (user == null)
            {
                MessageBox.Show(
                    "Користувача не знайдено в системі.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Fetch passwords using password service
            var passwords = await passwordService.GetPasswordsByUserIdAsync(user.Id);

            // Clear and repopulate the passwords collection
            viewModel.Passwords.Clear();
            foreach (var password in passwords)
            {
                viewModel.Passwords.Add(new PasswordDisplayItem
                {
                    Password = new Password
                    {
                        Id = password.Id,
                        Url = password.Url,
                        Login = password.Login,
                        PasswordEncrypted = viewModel.DecryptPassword(password.PasswordEncrypted),
                        Note = password.Note,
                        Category = password.Category,
                        UserId = password.UserId
                    },
                    IsRevealed = false
                });
            }
        }
    }
}
