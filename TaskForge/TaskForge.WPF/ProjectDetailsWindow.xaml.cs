using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.Application.Services;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Services;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for ProjectDetailsWindow.xaml
    /// </summary>
    public partial class ProjectDetailsWindow : Window
    {
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly ITaskFilterService _taskFilterService;
        private readonly LoginResult _currentLoginResult;
        private readonly int _projectId;
        private List<int> _selectedAssigneeIds = new List<int>();

        public ProjectDetailsWindow(
            int projectId,
            ProjectDto projectDto,
            IProjectService projectService,
            ITaskService taskService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult currentLoginResult,
            ITaskFilterService taskFilterService
        )
        {
            InitializeComponent();
            _projectId = projectId;
            _projectService = projectService;
            _taskService = taskService;
            _userService = userService;
            _auth0Service = auth0Service;
            _currentLoginResult = currentLoginResult;
            _taskFilterService = taskFilterService;
            ProjectNameText.Text = projectDto.Name;
            ProjectDescriptionText.Text = projectDto.Description ?? "Опис відсутній";
            ProjectStatusText.Text = $"Статус: {projectDto.Status} | Ваша роль: {projectDto.UserRoleInProject}";

        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProjectDetails();
            await LoadProjectUsersForFilter();
        }
        private async Task LoadProjectDetails(int? userId = null)
        {
            try
            {
                var tasks = await _taskFilterService.GetTasksForProjectAsync(_projectId, userId);
                TasksListView.ItemsSource = tasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження деталей проекту: {ex.Message}", "Помилка");
            }
        }

        private async Task LoadProjectUsersForFilter()
        {
            try
            {
                var users = await _userService.GetUsersByProjectIdAsync(_projectId);
                UserFilterComboBox.ItemsSource = users.Select(u => new { Id = u.Id, Username = $"{u.FirstName} {u.LastName}" }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження користувачів проєкту: {ex.Message}", "Помилка");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private async void UserFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserFilterComboBox.SelectedValue is int userId)
            {
                await LoadProjectDetails(userId);
            }
        }

        private async void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            UserFilterComboBox.SelectedItem = null;
            await LoadProjectDetails();
        }

        private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var auth0Id = _auth0Service.GetUserId(_currentLoginResult);
                var currentUser = await _userService.GetUserByAuth0IdAsync(auth0Id);
                if (currentUser == null)
                {
                    MessageBox.Show("Не вдалося визначити поточного користувача", "Помилка");
                    return;
                }

                var userProjects = await _projectService.GetProjectsForUserAsync(currentUser.Id);
                TaskProjectComboBox.ItemsSource = userProjects;
                TaskProjectComboBox.SelectedValue = _projectId;

                ClearTaskForm();

                TaskModalOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearTaskForm()
        {
            TaskTitleBox.Text = "";
            TaskDescriptionBox.Text = "";
            TaskDueDateBox.SelectedDate = DateTime.Now.AddDays(1);
            _selectedAssigneeIds.Clear();
            SelectedAssigneesText.Text = "Виконавці: не обрано";
        }

        private async void AssignUsersButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskProjectComboBox.SelectedValue == null)
            {
                MessageBox.Show("Будь ласка, спочатку виберіть проект.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var projectId = (int)TaskProjectComboBox.SelectedValue;
                var projectUsers = await _userService.GetUsersByProjectIdAsync(projectId);

                var userSelectionVM = projectUsers.Select(u => new UserSelectionViewModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    IsSelected = _selectedAssigneeIds.Contains(u.Id)
                }).ToList();

                UsersForAssignmentListBox.ItemsSource = userSelectionVM;
                AssignUsersModalOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження користувачів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignUsersSave_Click(object sender, RoutedEventArgs e)
        {
            var selectedUsers = UsersForAssignmentListBox.ItemsSource as List<UserSelectionViewModel>;
            if (selectedUsers == null) return;

            _selectedAssigneeIds = selectedUsers
                .Where(u => u.IsSelected)
                .Select(u => u.Id)
                .ToList();

            SelectedAssigneesText.Text = _selectedAssigneeIds.Count > 0
                ? $"Виконавці: обрано {_selectedAssigneeIds.Count}"
                : "Виконавці: не обрано";

            AssignUsersModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void AssignUsersCancel_Click(object sender, RoutedEventArgs e)
        {
            AssignUsersModalOverlay.Visibility = Visibility.Collapsed;
        }

        private async void TaskModalOk_Click(object sender, RoutedEventArgs e)
        {
            if (TaskProjectComboBox.SelectedValue == null)
            {
                MessageBox.Show("Будь ласка, виберіть проект.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TaskTitleBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть назву завдання.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TaskDueDateBox.SelectedDate == null)
            {
                MessageBox.Show("Будь ласка, виберіть термін виконання.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var projectId = (int)TaskProjectComboBox.SelectedValue;

                var newTask = await _taskService.CreateTaskAsync(
                    TaskTitleBox.Text,
                    TaskDescriptionBox.Text,
                    TaskDueDateBox.SelectedDate.Value,
                    projectId
                );

                foreach (var userId in _selectedAssigneeIds)
                {
                    await _taskService.AssignUserToTaskAsync(newTask.Id, userId);
                }

                MessageBox.Show($"Завдання '{newTask.Title}' успішно створено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                TaskModalOverlay.Visibility = Visibility.Collapsed;

                await LoadProjectDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення завдання: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskModalCancel_Click(object sender, RoutedEventArgs e)
        {
            TaskModalOverlay.Visibility = Visibility.Collapsed;
        }
        private async void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null) return;

            var taskId = (int)button.Tag;

            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити це завдання? Цю дію неможливо скасувати.",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _taskService.DeleteTaskAsync(taskId);

                    if (success)
                    {
                        MessageBox.Show("Завдання успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                        // Оновлюємо список завдань після видалення
                        await LoadProjectDetails();
                    }
                    else
                    {
                        MessageBox.Show("Не вдалося знайти завдання для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка під час видалення завдання: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AddUserToProjectButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserEmailBox.Text = string.Empty;
            AddUserModalOverlay.Visibility = Visibility.Visible;
        }

        private void AddUserModalCancel_Click(object sender, RoutedEventArgs e)
        {
            AddUserModalOverlay.Visibility = Visibility.Collapsed;
        }

        private async void AddUserModalOk_Click(object sender, RoutedEventArgs e)
        {
            var email = AddUserEmailBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Будь ласка, введіть email користувача.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                // Find user by email
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    MessageBox.Show($"Користувача з email '{email}' не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Check if already in project
                var alreadyInProject = await _userService.IsUserInProjectAsync(user.Id, _projectId);
                if (alreadyInProject)
                {
                    MessageBox.Show($"Користувач вже є учасником проекту.", "Увага", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // Add user to project
                await _userService.AddUserToProjectAsync(user.Id, _projectId, TaskForge.Domain.Enums.Role.Member);
                MessageBox.Show($"Користувача '{user.FirstName} {user.LastName}' додано до проекту!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                AddUserModalOverlay.Visibility = Visibility.Collapsed;
                await LoadProjectDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні користувача: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    public class UserSelectionViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public bool IsSelected { get; set; }
    }
}
