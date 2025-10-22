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
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Repositories;
using Duende.IdentityModel.OidcClient;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for ProjectDetailsWindow.xaml
    /// </summary>
    public partial class ProjectDetailsWindow : Window
    {
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private readonly UserRepository _userRepository;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult _currentLoginResult;
        private readonly int _projectId;
        private List<int> _selectedAssigneeIds = new List<int>();

        public ProjectDetailsWindow(
            int projectId,
            ProjectDto projectDto,
            ProjectRepository projectRepository,
            TaskRepository taskRepository,
            UserRepository userRepository,
            Auth0Service auth0Service,
            LoginResult currentLoginResult
        )
        {
            InitializeComponent();
            _projectId = projectId;
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _auth0Service = auth0Service;
            _currentLoginResult = currentLoginResult;

            ProjectNameText.Text = projectDto.Name;
            ProjectDescriptionText.Text = projectDto.Description ?? "Опис відсутній";
            ProjectStatusText.Text = $"Статус: {projectDto.Status} | Ваша роль: {projectDto.UserRoleInProject}";

            LoadProjectDetails();
        }

        private async Task LoadProjectDetails()
        {
            try
            {
                var tasks = await _projectRepository.GetTasksByProjectIdAsync(_projectId);
                TasksListView.ItemsSource = tasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження деталей проекту: {ex.Message}", "Помилка");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var auth0Id = _auth0Service.GetUserId(_currentLoginResult);
                var currentUser = await _userRepository.GetUserByAuth0IdAsync(auth0Id);
                if (currentUser == null)
                {
                    MessageBox.Show("Не вдалося визначити поточного користувача", "Помилка");
                    return;
                }

                var userProjects = await _projectRepository.GetProjectsForUserAsync(currentUser.Id);
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
                var projectUsers = await _userRepository.GetUsersByProjectIdAsync(projectId);

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

                var newTask = await _taskRepository.CreateTaskAsync(
                    TaskTitleBox.Text,
                    TaskDescriptionBox.Text,
                    TaskDueDateBox.SelectedDate.Value,
                    projectId
                );

                foreach (var userId in _selectedAssigneeIds)
                {
                    await _taskRepository.AssignUserToTaskAsync(newTask.Id, userId);
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
    }
    public class UserSelectionViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public bool IsSelected { get; set; }
    }
}
