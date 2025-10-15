using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TaskForge.Infrastructure.Repositories; // Add namespace for UserRepository
using TaskForge.Domain.Enums;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Auth0Service _auth0Service;
        private readonly UserRepository _userRepository; // Add UserRepository field
        private readonly ProjectRepository _projectRepository;
        private LoginResult _currentLoginResult;

        public MainWindow(Auth0Service auth0Service, UserRepository userRepository, ProjectRepository projectRepository)
        {
            InitializeComponent();
            _auth0Service = auth0Service;
            _userRepository = userRepository; // Initialize UserRepository
            _projectRepository = projectRepository;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginButton.IsEnabled = false;
                StatusText.Text = "Відкриття браузера для входу...";

                _currentLoginResult = await _auth0Service.LoginAsync();

                if (_currentLoginResult.IsError)
                {
                    StatusText.Text = $"Помилка: {_currentLoginResult.Error}";
                    LoginButton.IsEnabled = true;
                    return;
                }

                // Extract user details
                var userName = _auth0Service.GetUserName(_currentLoginResult) ?? "Невідомо";
                var userEmail = _auth0Service.GetUserEmail(_currentLoginResult) ?? "Невідомо";
                var userId = _auth0Service.GetUserId(_currentLoginResult) ?? "Невідомо";

                // Split userName into firstName and lastName
                var nameParts = userName.Split(' ', 2);
                var firstName = nameParts.Length > 0 ? nameParts[0] : "";
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                // Add user to the database
                await _userRepository.AddUserFromAuth0ResponseAsync(firstName, lastName, userEmail, userId);

                // Show user info
                ShowUserInfo(_currentLoginResult);
                
                StatusText.Text = "Успішний вхід!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при вході: {ex.Message}", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoginButton.IsEnabled = true;
                StatusText.Text = "";
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogoutButton.IsEnabled = false;
                StatusText.Text = "Вихід...";

                await _auth0Service.LogoutAsync();

                // Очистити інформацію про користувача
                _currentLoginResult = null;
                UserNameText.Text = string.Empty;
                UserEmailText.Text = string.Empty;
                UserIdText.Text = string.Empty;

                // Перемкнути панелі
                UserInfoPanel.Visibility = Visibility.Collapsed;
                LoginPanel.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
                LoginButton.Visibility = Visibility.Visible;

                // Увімкнути кнопку логіну
                LoginButton.IsEnabled = true;

                StatusText.Text = "Ви вийшли з системи";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при виході: {ex.Message}", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LogoutButton.IsEnabled = true;
            }
        }

        private void ShowUserInfo(LoginResult loginResult)
        {
            // Отримати дані користувача
            var userName = _auth0Service.GetUserName(loginResult) ?? "Невідомо";
            var userEmail = _auth0Service.GetUserEmail(loginResult) ?? "Невідомо";
            var userId = _auth0Service.GetUserId(loginResult) ?? "Невідомо";

            // Відобразити дані
            UserNameText.Text = userName;
            UserEmailText.Text = userEmail;
            UserIdText.Text = userId;

            // Перемкнути панелі
            LoginPanel.Visibility = Visibility.Collapsed;
            UserInfoPanel.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Visible;
            LogoutButton.IsEnabled = true;
            // Якщо потрібно використовувати Access Token для API запитів:
            // var accessToken = _auth0Service.GetAccessToken(loginResult);
            // Console.WriteLine($"Access Token: {accessToken}");
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectModalOverlay.Visibility = Visibility.Visible;
            ProjectNameBox.Text = string.Empty;
            ProjectStatusBox.Text = "Active";
            ProjectDescriptionBox.Text = string.Empty;
        }

        private async void ProjectModalOk_Click(object sender, RoutedEventArgs e)
        {
            var name = ProjectNameBox.Text.Trim();
            var status = ProjectStatusBox.Text.Trim();
            var description = ProjectDescriptionBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(status))
            {
                MessageBox.Show("Будь ласка, заповніть всі обов'язкові поля.");
                return;
            }
            if (_currentLoginResult == null)
            {
                MessageBox.Show("Будь ласка, увійдіть, щоб створити проект.");
                ProjectModalOverlay.Visibility = Visibility.Collapsed;
                return;
            }
            var userId = _auth0Service.GetUserId(_currentLoginResult);
            var user = await _userRepository.GetUserByAuth0IdAsync(userId);
            if (user == null)
            {
                MessageBox.Show("Користувача не знайдено в базі даних.");
                ProjectModalOverlay.Visibility = Visibility.Collapsed;
                return;
            }
            await _projectRepository.CreateProjectForUserAsync(name, status, description, user.Id, TaskForge.Domain.Enums.Role.Moderator);
            MessageBox.Show($"Проект '{name}' створено!", "Успіх");
            ProjectModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ProjectModalCancel_Click(object sender, RoutedEventArgs e)
        {
            ProjectModalOverlay.Visibility = Visibility.Collapsed;
        }
    }
}