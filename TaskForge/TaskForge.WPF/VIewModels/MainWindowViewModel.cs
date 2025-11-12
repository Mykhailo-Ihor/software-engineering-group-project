using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.Application.Services;
using TaskForge.Domain.Enums;
using TaskForge.WPF.Commands;
using SysApp = System.Windows.Application;

namespace TaskForge.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Auth0Service _auth0Service;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private readonly IExpenseService _expenseService;
        private readonly ITaskFilterService _taskFilterService;
        private readonly IPasswordService _passwordService;
        private readonly ISubscriptionService _subscriptionService;
        private LoginResult _currentLoginResult;

        // Visibility Properties
        private bool _isMainContentVisible;
        public bool IsMainContentVisible
        {
            get => _isMainContentVisible;
            set => SetProperty(ref _isMainContentVisible, value);
        }

        private bool _isUserInfoVisible;
        public bool IsUserInfoVisible
        {
            get => _isUserInfoVisible;
            set => SetProperty(ref _isUserInfoVisible, value);
        }

        private bool _isLoginPanelVisible;
        public bool IsLoginPanelVisible
        {
            get => _isLoginPanelVisible;
            set => SetProperty(ref _isLoginPanelVisible, value);
        }

        private bool _isLoginButtonVisible;
        public bool IsLoginButtonVisible
        {
            get => _isLoginButtonVisible;
            set => SetProperty(ref _isLoginButtonVisible, value);
        }

        private bool _isLogoutButtonVisible;
        public bool IsLogoutButtonVisible
        {
            get => _isLogoutButtonVisible;
            set => SetProperty(ref _isLogoutButtonVisible, value);
        }

        private bool _isProjectsListVisible;
        public bool IsProjectsListVisible
        {
            get => _isProjectsListVisible;
            set => SetProperty(ref _isProjectsListVisible, value);
        }

        // User Info Properties
        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private string _userEmail;
        public string UserEmail
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        private BitmapImage _userAvatar;
        public BitmapImage UserAvatar
        {
            get => _userAvatar;
            set => SetProperty(ref _userAvatar, value);
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private bool _isLoginButtonEnabled;
        public bool IsLoginButtonEnabled
        {
            get => _isLoginButtonEnabled;
            set => SetProperty(ref _isLoginButtonEnabled, value);
        }

        private bool _isLogoutButtonEnabled;
        public bool IsLogoutButtonEnabled
        {
            get => _isLogoutButtonEnabled;
            set => SetProperty(ref _isLogoutButtonEnabled, value);
        }

        // Project Modal Properties
        private bool _isProjectModalVisible;
        public bool IsProjectModalVisible
        {
            get => _isProjectModalVisible;
            set => SetProperty(ref _isProjectModalVisible, value);
        }

        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        private string _projectStatus;
        public string ProjectStatus
        {
            get => _projectStatus;
            set => SetProperty(ref _projectStatus, value);
        }

        private string _projectDescription;
        public string ProjectDescription
        {
            get => _projectDescription;
            set => SetProperty(ref _projectDescription, value);
        }

        // Projects List
        private ObservableCollection<ProjectDto> _projects;
        public ObservableCollection<ProjectDto> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        // Commands
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand CreateProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand CancelProjectCommand { get; }
        public ICommand ViewProjectsCommand { get; }
        public ICommand OpenProjectDetailsCommand { get; }
        public ICommand ViewExpensesCommand { get; }
        public ICommand OpenPasswordManagerCommand { get; }
        public ICommand OpenSubscriptionManagerCommand { get; }

        public MainWindowViewModel(
            Auth0Service auth0Service,
            IUserService userService,
            IProjectService projectService,
            ITaskFilterService filterService,
            ITaskService taskService,
            IExpenseService expenseService,
            IPasswordService passwordService,
            ISubscriptionService subscriptionService)
        {
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _taskFilterService = filterService ?? throw new ArgumentNullException(nameof(filterService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

            // Initialize properties
            _projects = new ObservableCollection<ProjectDto>();
            _isMainContentVisible = false;
            _isUserInfoVisible = false;
            _isLoginPanelVisible = true;
            _isLoginButtonVisible = true;
            _isLogoutButtonVisible = false;
            _isProjectsListVisible = false;
            _isLoginButtonEnabled = true;
            _isLogoutButtonEnabled = true;
            _userName = string.Empty;
            _userEmail = string.Empty;
            _statusText = string.Empty;
            _projectName = string.Empty;
            _projectStatus = "Active";
            _projectDescription = string.Empty;
            _userAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));

            // Initialize commands
            LoginCommand = new AsyncRelayCommand(OnLoginAsync);
            LogoutCommand = new AsyncRelayCommand(OnLogoutAsync);
            ProfileCommand = new AsyncRelayCommand(OnProfileAsync);
            CreateProjectCommand = new RelayCommand(OnCreateProject);
            SaveProjectCommand = new AsyncRelayCommand(OnSaveProjectAsync);
            CancelProjectCommand = new RelayCommand(OnCancelProject);
            ViewProjectsCommand = new AsyncRelayCommand(OnViewProjectsAsync);
            OpenProjectDetailsCommand = new RelayCommand(OnOpenProjectDetails);
            ViewExpensesCommand = new RelayCommand(OnViewExpenses);
            OpenPasswordManagerCommand = new RelayCommand(OnOpenPasswordManager); 
            OpenSubscriptionManagerCommand = new RelayCommand(OnOpenSubscriptionManager);
        }

        private async Task OnLoginAsync()
        {
            try
            {
                IsLoginButtonEnabled = false;
                StatusText = "Відкриття браузера для входу...";

                _currentLoginResult = await _auth0Service.LoginAsync();

                if (_currentLoginResult.IsError)
                {
                    StatusText = $"Помилка: {_currentLoginResult.Error}";
                    IsLoginButtonEnabled = true;
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
                IsLogoutButtonEnabled = true;
                StatusText = "Успішний вхід!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при вході: {ex.Message}", "Помилка",
              MessageBoxButton.OK, MessageBoxImage.Error);
                IsLoginButtonEnabled = true;
                StatusText = "";
            }
        }

        private async Task OnLogoutAsync()
        {
            try
            {
                IsLogoutButtonEnabled = false;
                StatusText = "Вихід...";

                await _auth0Service.LogoutAsync();

                _currentLoginResult = null;
                UserName = string.Empty;
                UserEmail = string.Empty;
                IsMainContentVisible = false;
                IsUserInfoVisible = false;
                IsLoginPanelVisible = true;
                IsLogoutButtonVisible = false;
                IsLoginButtonVisible = true;
                Projects.Clear();
                IsProjectsListVisible = false;

                IsLoginButtonEnabled = true;

                StatusText = "Ви вийшли з системи";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при виході: {ex.Message}", "Помилка",
               MessageBoxButton.OK, MessageBoxImage.Error);
                IsLogoutButtonEnabled = true;
            }
        }

        private void ShowUserInfo(LoginResult loginResult)
        {
            var userName = _auth0Service.GetUserName(loginResult) ?? "Невідомо";
            var userEmail = _auth0Service.GetUserEmail(loginResult) ?? "Невідомо";
            var avatarUrl = _auth0Service.GetUserAvatarUrl(loginResult);

            UserName = userName;
            UserEmail = userEmail;

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                UserAvatar = new BitmapImage(new Uri(avatarUrl));
            }
            else
            {
                UserAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));
            }

            IsMainContentVisible = true;
            IsUserInfoVisible = true;
            IsLoginPanelVisible = false;
            IsLoginButtonVisible = false;
            IsLogoutButtonVisible = true;
        }

        private async Task OnProfileAsync()
        {
            var profileWindow = new ProfileWindow(_currentLoginResult, _auth0Service, _userService);
            var mainWindow = SysApp.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                profileWindow.Owner = mainWindow;
            }
            profileWindow.ShowDialog();

            if (_currentLoginResult != null && !_currentLoginResult.IsError)
            {
                var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
                var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user != null)
                {
                    UserName = $"{user.FirstName} {user.LastName}";
                    UserEmail = user.Email;
                    var avatarUrl = _auth0Service.GetUserAvatarUrl(_currentLoginResult);
                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        UserAvatar = new BitmapImage(new Uri(avatarUrl));
                    }
                    else
                    {
                        UserAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));
                    }
                }
            }
        }

        private void OnCreateProject()
        {
            ProjectName = string.Empty;
            ProjectStatus = "Active";
            ProjectDescription = string.Empty;
            IsProjectModalVisible = true;
        }

        private async Task OnSaveProjectAsync()
        {
            var name = ProjectName?.Trim();
            var status = ProjectStatus?.Trim();
            var description = ProjectDescription?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(status))
            {
                MessageBox.Show("Будь ласка, заповніть всі обов'язкові поля.");
                return;
            }

            if (_currentLoginResult == null)
            {
                MessageBox.Show("Будь ласка, увійдіть, щоб створити проект.");
                IsProjectModalVisible = false;
                return;
            }

            var userId = _auth0Service.GetUserId(_currentLoginResult);
            var user = await _userService.GetUserByAuth0IdAsync(userId);
            if (user == null)
            {
                MessageBox.Show("Користувача не знайдено в базі даних.");
                IsProjectModalVisible = false;
                return;
            }

            await _projectService.CreateProjectForUserAsync(name, status, description, user.Id, Role.Moderator);
            MessageBox.Show($"Проект '{name}' створено!", "Успіх");
            IsProjectModalVisible = false;
        }

        private void OnCancelProject()
        {
            IsProjectModalVisible = false;
        }

        private async Task OnViewProjectsAsync()
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

                Projects = new ObservableCollection<ProjectDto>(userProjects);
                IsProjectsListVisible = true;

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

        private void OnOpenProjectDetails(object parameter)
        {
            if (parameter is not int projectId) return;

            var selectedProject = Projects.FirstOrDefault(p => p.Id == projectId);

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

        private void OnViewExpenses()
        {
            try
            {
                if (_currentLoginResult == null || _currentLoginResult.IsError)
                {
                    MessageBox.Show("Будь ласка, увійдіть в систему, щоб переглянути витрати.");
                    return;
                }

                var summaryWindow = new FinancialSummaryWindow(
                    _expenseService,
                    _userService,
                    _auth0Service,
                    _currentLoginResult
                );

                var mainWindow = SysApp.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null)
                {
                    summaryWindow.Owner = mainWindow;
                }
                summaryWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відкриття вікна витрат: {ex.Message}", "Помилка");
            }
        }

        private void OnOpenPasswordManager()
        {
            var passwordManagerWindow = new PasswordManagerWindow(
             _passwordService,
            _auth0Service,
             _userService,
            _currentLoginResult);
            passwordManagerWindow.ShowDialog();
        }

        private void OnOpenSubscriptionManager()
        {
            try
            {
                if (_currentLoginResult == null || _currentLoginResult.IsError)
                {
                    MessageBox.Show("Будь ласка, увійдіть в систему, щоб керувати підписками.");
                    return;
                }

                var subscriptionWindow = new SubscriptionSummaryWindow(
                    _subscriptionService,
                    _userService,
                    _auth0Service,
                    _currentLoginResult
                );

                var mainWindow = SysApp.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null)
                {
                    subscriptionWindow.Owner = mainWindow;
                }
                subscriptionWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відкриття вікна підписок: {ex.Message}", "Помилка");
            }
        }
    }
}
