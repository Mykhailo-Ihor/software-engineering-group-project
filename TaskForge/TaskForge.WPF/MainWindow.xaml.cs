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
        private readonly IExpenseService _expenseService;
        private LoginResult _currentLoginResult;
        private ITaskFilterService _taskFilterService;
        private List<int> _selectedAssigneeIds = new List<int>();

        public MainWindow(
            Auth0Service auth0Service, 
            IUserService userService, 
            IProjectService projectService, 
            ITaskFilterService filterService,
            ITaskService taskService,
            IExpenseService expenseService
            )
        {
            InitializeComponent();
            _auth0Service = auth0Service;
            _userService = userService;
            _projectService = projectService;
            _taskFilterService = filterService;
            _taskService = taskService;
            _expenseService = expenseService;
            InitializeExpenseComboBoxes();

            MainContentPanel.Visibility = Visibility.Collapsed;
            UserInfoPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Visible;
            LogoutButton.Visibility = Visibility.Collapsed;
            ProjectsListView.Visibility = Visibility.Collapsed;
            //ExpensesListView.Visibility = Visibility.Collapsed;
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
                LogoutButton.IsEnabled = true;
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
                MainContentPanel.Visibility = Visibility.Collapsed;
                UserInfoPanel.Visibility = Visibility.Collapsed;
                LoginPanel.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
                LoginButton.Visibility = Visibility.Visible;
                ProjectsListView.ItemsSource = null;
                ProjectsListView.Visibility = Visibility.Collapsed;
                //ExpensesListView.Visibility = Visibility.Collapsed;

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
            var avatarUrl = _auth0Service.GetUserAvatarUrl(loginResult);

            UserNameText.Text = userName;
            UserEmailText.Text = userEmail;
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                UserAvatarBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(avatarUrl));
            }
            else
            {
                UserAvatarBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));
            }
            MainContentPanel.Visibility = Visibility.Visible;
            UserInfoPanel.Visibility = Visibility.Visible;
            LoginPanel.Visibility = Visibility.Collapsed;
            LoginButton.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Visible;
        }

        private async void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow(_currentLoginResult, _auth0Service, _userService);
            profileWindow.Owner = this;
            profileWindow.ShowDialog();

            if (_currentLoginResult != null && !_currentLoginResult.IsError)
            {
                var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
                var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user != null)
                {
                    UserNameText.Text = $"{user.FirstName} {user.LastName}";
                    UserEmailText.Text = user.Email;
                    var avatarUrl = _auth0Service.GetUserAvatarUrl(_currentLoginResult);
                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        UserAvatarBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(avatarUrl));
                    }
                    else
                    {
                        UserAvatarBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));
                    }
                }
            }
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

        private void InitializeExpenseComboBoxes()
        {
            // Заповнюємо валюти
            ExpenseCurrencyBox.ItemsSource = Enum.GetValues(typeof(Currency));
            ExpenseCurrencyBox.SelectedIndex = 0;

            // Заповнюємо категорії
            ExpenseCategoryBox.ItemsSource = Enum.GetValues(typeof(ExpenceCategory));
            ExpenseCategoryBox.SelectedIndex = 0;
        }

        private void AddExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            ExpenseModalOverlay.Visibility = Visibility.Visible;
            ExpenseAmountBox.Text = string.Empty;
            ExpenseCurrencyBox.SelectedIndex = 0;
            ExpenseCategoryBox.SelectedIndex = 0;
            ExpenseDateBox.SelectedDate = DateTime.Today;
            ExpenseDescriptionBox.Text = string.Empty;
        }

        private async void ExpenseModalOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валідація
                if (string.IsNullOrWhiteSpace(ExpenseAmountBox.Text))
                {
                    MessageBox.Show("Будь ласка, введіть суму витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(ExpenseAmountBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Будь ласка, введіть коректну суму (число більше 0).", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ExpenseCurrencyBox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, виберіть валюту.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ExpenseCategoryBox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, виберіть категорію.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ExpenseDateBox.SelectedDate.HasValue)
                {
                    MessageBox.Show("Будь ласка, виберіть дату витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Отримання поточного користувача
                if (_currentLoginResult == null)
                {
                    MessageBox.Show("Будь ласка, увійдіть в систему.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ExpenseModalOverlay.Visibility = Visibility.Collapsed;
                    return;
                }

                var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
                var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user == null)
                {
                    MessageBox.Show("Не вдалося знайти ваші дані в системі.");
                    ExpenseModalOverlay.Visibility = Visibility.Collapsed;
                    return;
                }

                // Отримання значень з ComboBox'ів напряму як enum
                var currency = (Currency)ExpenseCurrencyBox.SelectedItem;
                var category = (ExpenceCategory)ExpenseCategoryBox.SelectedItem;

                var date = ExpenseDateBox.SelectedDate.Value;
                var description = ExpenseDescriptionBox.Text.Trim();

                // Створення витрати
                await _expenseService.CreateExpenseAsync(amount, currency, category, date, description, user.Id);

                MessageBox.Show("Витрату успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                ExpenseModalOverlay.Visibility = Visibility.Collapsed;

                // Якщо список витрат вже відображається, оновлюємо його
                //if (ExpensesListView.Visibility == Visibility.Visible)
                //{
                //    await LoadUserExpenses();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні витрати: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExpenseModalCancel_Click(object sender, RoutedEventArgs e)
        {
            ExpenseModalOverlay.Visibility = Visibility.Collapsed;
        }

        private async void ViewExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentLoginResult == null || _currentLoginResult.IsError)
                {
                    MessageBox.Show("Будь ласка, увійдіть в систему, щоб переглянути витрати.");
                    return;
                }

                // Створюємо та відкриваємо нове вікно, передаючи необхідні сервіси
                var summaryWindow = new FinancialSummaryWindow(
                    _expenseService,
                    _userService,
                    _auth0Service,
                    _currentLoginResult
                );

                summaryWindow.Owner = this; // Встановлюємо головне вікно як власника
                summaryWindow.ShowDialog(); // Відкриваємо модально
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відкриття вікна витрат: {ex.Message}", "Помилка");
            }
        }
        private async Task LoadUserExpenses()
        {
            try
            {
                if (_currentLoginResult == null || _currentLoginResult.IsError)
                {
                    MessageBox.Show("Будь ласка, увійдіть в систему, щоб переглянути витрати.");
                    return;
                }

                var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
                var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user == null)
                {
                    MessageBox.Show("Не вдалося знайти ваші дані в системі.");
                    return;
                }

                var userExpenses = await _expenseService.GetUserExpensesAsync(user.Id);

                if (!userExpenses.Any())
                {
                    MessageBox.Show("У вас ще немає записів про витрати. Спробуйте додати нову витрату!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження витрат: {ex.Message}");
            }
        }
    }
}