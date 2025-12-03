using System;
using System.Threading.Tasks;
using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.PasswordManager
{
    /// <summary>
    /// Command to add a new password for the authenticated user
    /// </summary>
    public class AddPasswordCommand : AsyncRelayCommand
    {
        private readonly PasswordManagerViewModel _viewModel;
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult? _loginResult;

        public AddPasswordCommand(
          PasswordManagerViewModel viewModel,
          IPasswordService passwordService,
          IUserService userService,
          Auth0Service auth0Service,
          LoginResult? loginResult)
            : base(async _ => await ExecuteAddPasswordAsync(viewModel, passwordService, userService, auth0Service, loginResult))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _loginResult = loginResult;
        }

        private static async Task ExecuteAddPasswordAsync(
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
                   "���� �����, ������, ��� �������� �����.",
                       "������� ��������������",
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
                       "����������� �� �������� � ������.",
                         "�������",
                     MessageBoxButton.OK,
               MessageBoxImage.Error);
                return;
            }

            // Read input values from viewModel properties
            var passwordFromUI = viewModel.GetAddPassword?.Invoke() ?? "";
            var password = new Password
            {
                Url = viewModel.Url,
                Login = viewModel.Login,
                PasswordEncrypted = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(passwordFromUI)),
                Note = viewModel.Note,
                Category = viewModel.SelectedCategory,
                UserId = user.Id
            };

            // Add password via password service
            await passwordService.AddPasswordAsync(password);

            // Close the modal
            viewModel.IsAddModalVisible = false;

            // Reload passwords by executing LoadPasswordsCommand
            if (viewModel.LoadPasswordsCommand.CanExecute(null))
            {
                await ((AsyncRelayCommand)viewModel.LoadPasswordsCommand).ExecuteAsync(null);
            }
        }
    }
}
