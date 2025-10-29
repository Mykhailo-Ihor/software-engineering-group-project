using System;
using System.Windows;
using System.Windows.Media.Imaging;
using TaskForge.Application.Interfaces;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Enums;

namespace TaskForge.WPF
{
    public partial class ProfileWindow : Window
    {
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly Duende.IdentityModel.OidcClient.LoginResult _loginResult;
        private UserDto _currentUser;

        public ProfileWindow(Duende.IdentityModel.OidcClient.LoginResult loginResult, Auth0Service auth0Service, IUserService userService)
        {
            InitializeComponent();
            _loginResult = loginResult;
            _auth0Service = auth0Service;
            _userService = userService;
            LoadUserProfile();
        }

        private async void LoadUserProfile()
        {
            var auth0Id = _auth0Service.GetUserId(_loginResult);
            _currentUser = await _userService.GetUserByAuth0IdAsync(auth0Id);
            if (_currentUser != null)
            {
                FirstNameBox.Text = _currentUser.FirstName;
                LastNameBox.Text = _currentUser.LastName;
                EmailBox.Text = _currentUser.Email;
            }
            var avatarUrl = _auth0Service.GetUserAvatarUrl(_loginResult);
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                ProfileAvatarBrush.ImageSource = new BitmapImage(new Uri(avatarUrl));
            }
            else
            {
                ProfileAvatarBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));
            }
            var connection = _auth0Service.GetUserConnection(_loginResult);
            if (connection == "google-oauth2")
            {
                FirstNameBox.IsEnabled = false;
                LastNameBox.IsEnabled = false;
                EmailBox.IsEnabled = false;
                SaveButton.Visibility = Visibility.Collapsed;
                ProfileInfoText.Text = "Редагування профілю недоступне.";
                ProfileInfoText.Visibility = Visibility.Visible;
            }
            else if (connection == "Username-Password-Authentication")
            {
                FirstNameBox.IsEnabled = true;
                LastNameBox.IsEnabled = true;
                EmailBox.IsEnabled = true;
                SaveButton.Visibility = Visibility.Visible;
                ProfileInfoText.Visibility = Visibility.Collapsed;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User not loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var firstName = FirstNameBox.Text.Trim();
            var lastName = LastNameBox.Text.Trim();
            var email = EmailBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Будь ласка заповніть всі поля.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await _userService.UpdateUserProfileAsync(_currentUser.Id, firstName, lastName, email);
                var connection = _auth0Service.GetUserConnection(_loginResult);
                if (connection == "Username-Password-Authentication")
                {
                    var auth0Api = new Auth0ManagementApiController();
                    await auth0Api.UpdateAuth0UserProfileAsync(_currentUser.Auth0UserId, firstName, lastName, email);
                }
                MessageBox.Show("Профіль оновлено успішно!.", "Успіх!", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка редагування профілю: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
