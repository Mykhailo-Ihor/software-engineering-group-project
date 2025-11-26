using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.Profile
{
    public class LoadProfileCommand : AsyncRelayCommand
    {
        private readonly ProfileViewModel _viewModel;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult _loginResult;

        public LoadProfileCommand(
            ProfileViewModel viewModel,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult loginResult)
            : base(async _ => await ExecuteLoadProfileAsync(viewModel, userService, auth0Service, loginResult))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _loginResult = loginResult ?? throw new ArgumentNullException(nameof(loginResult));
        }

        private static async Task ExecuteLoadProfileAsync(
            ProfileViewModel viewModel,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult loginResult)
        {
            try
            {
                var auth0Id = auth0Service.GetUserId(loginResult);
                var currentUser = await userService.GetUserByAuth0IdAsync(auth0Id);
                viewModel.CurrentUser = currentUser;

                if (currentUser != null)
                {
                    viewModel.FirstName = currentUser.FirstName ?? string.Empty;
                    viewModel.LastName = currentUser.LastName ?? string.Empty;
                    viewModel.Email = currentUser.Email ?? string.Empty;
                }

                var avatarUrl = auth0Service.GetUserAvatarUrl(loginResult);
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    viewModel.ProfileAvatar = new BitmapImage(new Uri(avatarUrl));
                }

                var connection = auth0Service.GetUserConnection(loginResult);
                if (connection == "google-oauth2")
                {
                    viewModel.IsFieldsEnabled = false;
                    viewModel.SaveButtonVisibility = Visibility.Collapsed;
                    viewModel.ProfileInfoText = "Інформація профіля недоступна.";
                    viewModel.ProfileInfoVisibility = Visibility.Visible;
                }
                else if (connection == "Username-Password-Authentication")
                {
                    viewModel.IsFieldsEnabled = true;
                    viewModel.SaveButtonVisibility = Visibility.Visible;
                    viewModel.ProfileInfoVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                viewModel.ProfileInfoText = $"Помилка завантаження профілю: {ex.Message}";
                viewModel.ProfileInfoVisibility = Visibility.Visible;
            }
        }
    }
}