using System;
using System.Threading.Tasks;
using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.Profile
{
    public class SaveProfileCommand : AsyncRelayCommand
    {
        private readonly ProfileViewModel _viewModel;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult _loginResult;

        public SaveProfileCommand(
            ProfileViewModel viewModel,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult loginResult)
            : base(async _ => await ExecuteSaveProfileAsync(viewModel, userService, auth0Service, loginResult), _ => CanExecuteSaveProfile(viewModel))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _loginResult = loginResult ?? throw new ArgumentNullException(nameof(loginResult));
        }

        private static bool CanExecuteSaveProfile(ProfileViewModel viewModel)
        {
            return viewModel.IsFieldsEnabled && !string.IsNullOrWhiteSpace(viewModel.FirstName) && !string.IsNullOrWhiteSpace(viewModel.Email);
        }

        private static async Task ExecuteSaveProfileAsync(
            ProfileViewModel viewModel,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult loginResult)
        {
            if (viewModel.CurrentUser == null)
            {
                MessageBox.Show("Користувача не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var firstName = viewModel.FirstName?.Trim();
            var lastName = viewModel.LastName?.Trim();
            var email = viewModel.Email?.Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Будь ласка заповніть поля Ім'я та Електронна пошта.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await userService.UpdateUserProfileAsync(viewModel.CurrentUser.Id, firstName, lastName, email);

                var connection = auth0Service.GetUserConnection(loginResult);
                if (connection == "Username-Password-Authentication")
                {
                    var auth0Api = new Auth0ManagementApiController();
                    await auth0Api.UpdateAuth0UserProfileAsync(viewModel.CurrentUser.Auth0UserId, firstName, lastName, email);
                }

                MessageBox.Show("Профіль успішно оновлено!.", "Успіх!", MessageBoxButton.OK, MessageBoxImage.Information);
                viewModel.RequestClose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження профілю: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}