using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.Application.Services;
using TaskForge.Domain.Enums;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Auth0Service _auth0Service;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private LoginResult _currentLoginResult;
        private ITaskFilterService _taskFilterService;
        private List<int> _selectedAssigneeIds = new List<int>();

        public MainWindow(Auth0Service auth0Service, IUserService userService, IProjectService projectService, ITaskFilterService filterService,ITaskService taskService)
        {
            InitializeComponent();
            _auth0Service = auth0Service;
            _userService = userService;
            _projectService = projectService;
            _taskFilterService = filterService;
            _taskService = taskService;
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

                var userName = _auth0Service.GetUserName(_currentLoginResult) ?? "Невідомо";
                var userEmail = _auth0Service.GetUserEmail(_currentLoginResult) ?? "Невідомо";
                var userId = _auth0Service.GetUserId(_currentLoginResult) ?? "Невідомо";

                var nameParts = userName.Split(' ', 2);
                var firstName = nameParts.Length > 0 ? nameParts[0] : "";
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                await _userService.AddUserFromAuth0ResponseAsync(firstName, lastName, userEmail, userId);

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

                _currentLoginResult = null;
                UserNameText.Text = string.Empty;
                UserEmailText.Text = string.Empty;
                UserIdText.Text = string.Empty;

                UserInfoPanel.Visibility = Visibility.Collapsed;
                LoginPanel.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
                LoginButton.Visibility = Visibility.Visible;
                ProjectsListView.ItemsSource = null;
                ProjectsListView.Visibility = Visibility.Collapsed;

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
            var userName = _auth0Service.GetUserName(loginResult) ?? "Невідомо";
            var userEmail = _auth0Service.GetUserEmail(loginResult) ?? "Невідомо";
            var userId = _auth0Service.GetUserId(loginResult) ?? "Невідомо";

            UserNameText.Text = userName;
            UserEmailText.Text = userEmail;
            UserIdText.Text = userId;

            LoginPanel.Visibility = Visibility.Collapsed;
            UserInfoPanel.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Visible;
            LogoutButton.IsEnabled = true;
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
            var user = await _userService.GetUserByAuth0IdAsync(userId);
            if (user == null)
            {
                MessageBox.Show("Користувача не знайдено в базі даних.");
                ProjectModalOverlay.Visibility = Visibility.Collapsed;
                return;
            }
            await _projectService.CreateProjectForUserAsync(name, status, description, user.Id, TaskForge.Domain.Enums.Role.Moderator);
            MessageBox.Show($"Проект '{name}' створено!", "Успіх");
            ProjectModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ProjectModalCancel_Click(object sender, RoutedEventArgs e)
        {
            ProjectModalOverlay.Visibility = Visibility.Collapsed;
        }
        private async void ViewProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentLoginResult == null || _currentLoginResult.IsError)
                {
                    MessageBox.Show("Будь ласка, увійдіть в систему, щоб переглянути проєкти.");
                    return;
                }
                var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
                var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user == null)
                {
                    MessageBox.Show("Не вдалося знайти ваші дані в системі.");
                    return;
                }
                var currentUserId = user.Id;
                var userProjects = await _projectService.GetUserProjectsAsync(currentUserId);
                
                ProjectsListView.ItemsSource = userProjects; 
                ProjectsListView.Visibility = Visibility.Visible;

                if (!userProjects.Any())
                {
                    MessageBox.Show("У вас ще немає жодного проєкту. Спробуйте створити новий!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження проєктів: {ex.Message}");
            }
        }

        private void ProjectName_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null) return;

            var projectId = (int)button.Tag;

            var selectedProject = (ProjectsListView.ItemsSource as IEnumerable<ProjectDto>)?
                .FirstOrDefault(p => p.Id == projectId);

            if (selectedProject == null)
            {
                MessageBox.Show("Не вдалося знайти проект");
                return;
            }

            var detailsWindow = new ProjectDetailsWindow(
                projectId,
                selectedProject,
                _projectService,
                _taskService,
                _userService,
                _auth0Service,
                _currentLoginResult,
                _taskFilterService
            );
            detailsWindow.ShowDialog();
        }
    }
}