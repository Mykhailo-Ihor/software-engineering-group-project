using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.Commands;

namespace TaskForge.WPF.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly Duende.IdentityModel.OidcClient.LoginResult _loginResult;
        private UserDto? _currentUser;

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private BitmapImage _profileAvatar;
        public BitmapImage ProfileAvatar
        {
            get => _profileAvatar;
            set => SetProperty(ref _profileAvatar, value);
        }

        private string _profileInfoText = string.Empty;
        public string ProfileInfoText
        {
            get => _profileInfoText;
            set => SetProperty(ref _profileInfoText, value);
        }

        private Visibility _profileInfoVisibility = Visibility.Collapsed;
        public Visibility ProfileInfoVisibility
        {
            get => _profileInfoVisibility;
            set => SetProperty(ref _profileInfoVisibility, value);
        }

        private bool _isFieldsEnabled = true;
        public bool IsFieldsEnabled
        {
            get => _isFieldsEnabled;
            set => SetProperty(ref _isFieldsEnabled, value);
        }

        private Visibility _saveButtonVisibility = Visibility.Visible;
        public Visibility SaveButtonVisibility
        {
            get => _saveButtonVisibility;
            set => SetProperty(ref _saveButtonVisibility, value);
        }

        public ICommand LoadedCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public ProfileViewModel(Duende.IdentityModel.OidcClient.LoginResult loginResult, Auth0Service auth0Service, IUserService userService)
        {
            _loginResult = loginResult ?? throw new ArgumentNullException(nameof(loginResult));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            _profileAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));

            LoadedCommand = new AsyncRelayCommand(LoadUserProfileAsync);
            SaveCommand = new AsyncRelayCommand(SaveUserProfileAsync, CanSave);
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
        }

        private async Task LoadUserProfileAsync()
        {
            try
            {
                var auth0Id = _auth0Service.GetUserId(_loginResult);
                _currentUser = await _userService.GetUserByAuth0IdAsync(auth0Id);

                if (_currentUser != null)
                {
                    FirstName = _currentUser.FirstName ?? string.Empty;
                    LastName = _currentUser.LastName ?? string.Empty;
                    Email = _currentUser.Email ?? string.Empty;
                }

                var avatarUrl = _auth0Service.GetUserAvatarUrl(_loginResult);
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    ProfileAvatar = new BitmapImage(new Uri(avatarUrl));
                }

                var connection = _auth0Service.GetUserConnection(_loginResult);
                if (connection == "google-oauth2")
                {
                    IsFieldsEnabled = false;
                    SaveButtonVisibility = Visibility.Collapsed;
                    ProfileInfoText = "Інформація профіля недоступна.";
                    ProfileInfoVisibility = Visibility.Visible;
                }
                else if (connection == "Username-Password-Authentication")
                {
                    IsFieldsEnabled = true;
                    SaveButtonVisibility = Visibility.Visible;
                    ProfileInfoVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ProfileInfoText = $"Помилка завантаження профілю: {ex.Message}";
                ProfileInfoVisibility = Visibility.Visible;
            }
        }

        private bool CanSave()
        {
            return IsFieldsEnabled && !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) && !string.IsNullOrWhiteSpace(Email);
        }

        private async Task SaveUserProfileAsync()
        {
            if (_currentUser == null)
            {
                ShowError("Користувача не знайдено.", "Помилка");
                return;
            }

            var firstName = FirstName?.Trim();
            var lastName = LastName?.Trim();
            var email = Email?.Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            {
                ShowError("Будь ласка заповніть всі поля.", "Попередження");
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

                ShowSuccess("Профіль успішно оновлено!.", "Успіх!");
                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError($"Помилка збереження профілю: {ex.Message}", "Помилка");
            }
        }

        private void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccess(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event Action? CloseRequested;
    }
}