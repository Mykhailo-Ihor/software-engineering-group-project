using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WPF.Commands;
using SysApp = System.Windows.Application;
using TaskForge.WPF.Commands.ProjectDetails;

namespace TaskForge.WPF.ViewModels
{
    public class ProjectDetailsViewModel : ViewModelBase
    {
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly ITaskFilterService _taskFilterService;
        private readonly LoginResult _currentLoginResult;
        private readonly ProjectDto _projectDto;
        private readonly int _projectId;
        private int _editingTaskId;
        private List<int> _selectedAssigneeIds = new List<int>();
        private int _currentUserId;

        public List<int> SelectedAssigneeIds => _selectedAssigneeIds;
        public int EditingTaskId
        {
            get => _editingTaskId;
            set => _editingTaskId = value;
        }

        // Project Info Properties
        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        private string _projectDescription;
        public string ProjectDescription
        {
            get => _projectDescription;
            set => SetProperty(ref _projectDescription, value);
        }

        private string _projectStatus;
        public string ProjectStatus
        {
            get => _projectStatus;
            set => SetProperty(ref _projectStatus, value);
        }

        // Visibility Properties
        private bool _isManageModeratorsVisible;
        public bool IsManageModeratorsVisible
        {
            get => _isManageModeratorsVisible;
            set => SetProperty(ref _isManageModeratorsVisible, value);
        }

        private bool _isEditProjectVisible;
        public bool IsEditProjectVisible
        {
            get => _isEditProjectVisible;
            set => SetProperty(ref _isEditProjectVisible, value);
        }

        // Tasks List
        private ObservableCollection<TaskEntity> _tasks;
        public ObservableCollection<TaskEntity> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        // User Filter
        private ObservableCollection<UserFilterItem> _userFilterItems;
        public ObservableCollection<UserFilterItem> UserFilterItems
        {
            get => _userFilterItems;
            set => SetProperty(ref _userFilterItems, value);
        }

        private UserFilterItem _selectedUserFilter;
        public UserFilterItem SelectedUserFilter
        {
            get => _selectedUserFilter;
            set
            {
                if (SetProperty(ref _selectedUserFilter, value))
                {
                    _ = LoadProjectDetailsAsync(value?.Id);
                }
            }
        }

        // Add Task Modal Properties
        private bool _isTaskModalVisible;
        public bool IsTaskModalVisible
        {
            get => _isTaskModalVisible;
            set => SetProperty(ref _isTaskModalVisible, value);
        }

        private string _taskTitle;
        public string TaskTitle
        {
            get => _taskTitle;
            set => SetProperty(ref _taskTitle, value);
        }

        private string _taskDescription;
        public string TaskDescription
        {
            get => _taskDescription;
            set => SetProperty(ref _taskDescription, value);
        }

        private DateTime _taskDueDate;
        public DateTime TaskDueDate
        {
            get => _taskDueDate;
            set => SetProperty(ref _taskDueDate, value);
        }

        private ObservableCollection<Project> _userProjects;
        public ObservableCollection<Project> UserProjects
        {
            get => _userProjects;
            set => SetProperty(ref _userProjects, value);
        }

        private int _selectedProjectId;
        public int SelectedProjectId
        {
            get => _selectedProjectId;
            set => SetProperty(ref _selectedProjectId, value);
        }

        private string _selectedAssigneesText;
        public string SelectedAssigneesText
        {
            get => _selectedAssigneesText;
            set => SetProperty(ref _selectedAssigneesText, value);
        }

        // Edit Task Modal Properties
        private bool _isEditTaskModalVisible;
        public bool IsEditTaskModalVisible
        {
            get => _isEditTaskModalVisible;
            set => SetProperty(ref _isEditTaskModalVisible, value);
        }

        private string _editTaskTitle;
        public string EditTaskTitle
        {
            get => _editTaskTitle;
            set => SetProperty(ref _editTaskTitle, value);
        }

        private string _editTaskDescription;
        public string EditTaskDescription
        {
            get => _editTaskDescription;
            set => SetProperty(ref _editTaskDescription, value);
        }

        private DateTime _editTaskDueDate;
        public DateTime EditTaskDueDate
        {
            get => _editTaskDueDate;
            set => SetProperty(ref _editTaskDueDate, value);
        }

        // Add User Modal Properties
        private bool _isAddUserModalVisible;
        public bool IsAddUserModalVisible
        {
            get => _isAddUserModalVisible;
            set => SetProperty(ref _isAddUserModalVisible, value);
        }

        private string _addUserEmail;
        public string AddUserEmail
        {
            get => _addUserEmail;
            set => SetProperty(ref _addUserEmail, value);
        }

        // Assign Users Modal Properties
        private bool _isAssignUsersModalVisible;
        public bool IsAssignUsersModalVisible
        {
            get => _isAssignUsersModalVisible;
            set => SetProperty(ref _isAssignUsersModalVisible, value);
        }

        private ObservableCollection<UserSelectionViewModel> _usersForAssignment;
        public ObservableCollection<UserSelectionViewModel> UsersForAssignment
        {
            get => _usersForAssignment;
            set => SetProperty(ref _usersForAssignment, value);
        }

        // Manage Moderators Modal Properties
        private bool _isManageModeratorsModalVisible;
        public bool IsManageModeratorsModalVisible
        {
            get => _isManageModeratorsModalVisible;
            set => SetProperty(ref _isManageModeratorsModalVisible, value);
        }

        private ObservableCollection<ModeratorSelectionViewModel> _moderators;
        public ObservableCollection<ModeratorSelectionViewModel> Moderators
        {
            get => _moderators;
            set => SetProperty(ref _moderators, value);
        }

        // Edit Project Modal Properties
        private bool _isEditProjectModalVisible;
        public bool IsEditProjectModalVisible
        {
            get => _isEditProjectModalVisible;
            set => SetProperty(ref _isEditProjectModalVisible, value);
        }

        private string _editProjectName;
        public string EditProjectName
        {
            get => _editProjectName;
            set => SetProperty(ref _editProjectName, value);
        }

        private string _editProjectDescription;
        public string EditProjectDescription
        {
            get => _editProjectDescription;
            set => SetProperty(ref _editProjectDescription, value);
        }

        private string _editProjectStatus;
        public string EditProjectStatus
        {
            get => _editProjectStatus;
            set => SetProperty(ref _editProjectStatus, value);
        }

        private bool _isLeaveProjectModalVisible;
        public bool IsLeaveProjectModalVisible
        {
            get => _isLeaveProjectModalVisible;
            set => SetProperty(ref _isLeaveProjectModalVisible, value);
        }

        private ObservableCollection<PromoteUserViewModel> _usersToPromote;
        public ObservableCollection<PromoteUserViewModel> UsersToPromote
        {
            get => _usersToPromote;
            set => SetProperty(ref _usersToPromote, value);
        }

        private bool _isKickUserModalVisible;
        public bool IsKickUserModalVisible
        {
            get => _isKickUserModalVisible;
            set => SetProperty(ref _isKickUserModalVisible, value);
        }

        private ObservableCollection<UserDto> _usersToKickList;
        public ObservableCollection<UserDto> UsersToKickList
        {
            get => _usersToKickList;
            set => SetProperty(ref _usersToKickList, value);
        }

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand CancelTaskCommand { get; }
        public ICommand AssignUsersCommand { get; }
        public ICommand SaveAssignUsersCommand { get; }
        public ICommand CancelAssignUsersCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand SaveEditTaskCommand { get; }
        public ICommand CancelEditTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand AddUserToProjectCommand { get; }
        public ICommand SaveAddUserCommand { get; }
        public ICommand CancelAddUserCommand { get; }
        public ICommand ManageModeratorsCommand { get; }
        public ICommand SaveModeratorsCommand { get; }
        public ICommand CancelModeratorsCommand { get; }
        public ICommand EditProjectCommand { get; }
        public ICommand SaveEditProjectCommand { get; }
        public ICommand CancelEditProjectCommand { get; }
        public ICommand LeaveProjectCommand { get; }
        public ICommand SaveAndLeaveProjectCommand { get; }
        public ICommand CancelLeaveProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand OpenKickUserModalCommand { get; }
        public ICommand KickUserCommand { get; } 
        public ICommand CloseKickUserModalCommand { get; }

        public ProjectDetailsViewModel(
            int projectId,
            ProjectDto projectDto,
            IProjectService projectService,
            ITaskService taskService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult currentLoginResult,
            ITaskFilterService taskFilterService)
        {
            _projectId = projectId;
            _projectDto = projectDto ?? throw new ArgumentNullException(nameof(projectDto));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _currentLoginResult = currentLoginResult ?? throw new ArgumentNullException(nameof(currentLoginResult));
            _taskFilterService = taskFilterService ?? throw new ArgumentNullException(nameof(taskFilterService));

            // Initialize properties
            _projectName = projectDto.Name;
            _projectDescription = projectDto.Description ?? "Опис відсутній";
            _projectStatus = $"Статус: {projectDto.Status} | Ваша роль: {projectDto.UserRoleInProject}";
            _tasks = new ObservableCollection<TaskEntity>();
            _userFilterItems = new ObservableCollection<UserFilterItem>();
            _userProjects = new ObservableCollection<Project>();
            _usersForAssignment = new ObservableCollection<UserSelectionViewModel>();
            _moderators = new ObservableCollection<ModeratorSelectionViewModel>();
            _selectedProjectId = projectId;
            _taskDueDate = DateTime.Now.AddDays(1);
            _selectedAssigneesText = "Виконавці: не обрано";
            _taskTitle = string.Empty;
            _taskDescription = string.Empty;
            _editTaskTitle = string.Empty;
            _editTaskDescription = string.Empty;
            _addUserEmail = string.Empty;
            _editProjectName = string.Empty;
            _editProjectDescription = string.Empty;
            _editProjectStatus = string.Empty;
            _usersToPromote = new ObservableCollection<PromoteUserViewModel>();

            // Initialize commands
            LoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
            CloseCommand = new RelayCommand(OnClose);
            ClearFilterCommand = new AsyncRelayCommand(OnClearFilterAsync);
            AddTaskCommand = new AsyncRelayCommand(OnAddTaskAsync);
            SaveTaskCommand = new SaveTaskCommand(this, _taskService);
            CancelTaskCommand = new RelayCommand(OnCancelTask);
            AssignUsersCommand = new AsyncRelayCommand(OnAssignUsersAsync);
            SaveAssignUsersCommand = new RelayCommand(OnSaveAssignUsers);
            CancelAssignUsersCommand = new RelayCommand(OnCancelAssignUsers);
            EditTaskCommand = new AsyncRelayCommand(OnEditTaskAsync);
            SaveEditTaskCommand = new SaveEditTaskCommand(this, _taskService);
            CancelEditTaskCommand = new RelayCommand(OnCancelEditTask);
            DeleteTaskCommand = new AsyncRelayCommand(OnDeleteTaskAsync);
            AddUserToProjectCommand = new RelayCommand(OnAddUserToProject);
            SaveAddUserCommand = new AsyncRelayCommand(OnSaveAddUserAsync);
            CancelAddUserCommand = new RelayCommand(OnCancelAddUser);
            ManageModeratorsCommand = new AsyncRelayCommand(OnManageModeratorsAsync);
            SaveModeratorsCommand = new AsyncRelayCommand(OnSaveModeratorsAsync);
            CancelModeratorsCommand = new RelayCommand(OnCancelModerators);
            EditProjectCommand = new RelayCommand(OnEditProject);
            SaveEditProjectCommand = new AsyncRelayCommand(OnSaveEditProjectAsync);
            CancelEditProjectCommand = new RelayCommand(OnCancelEditProject);
            LeaveProjectCommand = new AsyncRelayCommand(OnLeaveProjectAsync);
            SaveAndLeaveProjectCommand = new AsyncRelayCommand(OnSaveAndLeaveProjectAsync);
            CancelLeaveProjectCommand = new RelayCommand(OnCancelLeaveProject);
            DeleteProjectCommand = new AsyncRelayCommand(OnDeleteProjectAsync);
            OpenKickUserModalCommand = new AsyncRelayCommand(OnOpenKickUserModalAsync);
            KickUserCommand = new KickUserCommand(this, _userService, _projectId);
            CloseKickUserModalCommand = new RelayCommand(OnCloseKickUserModal);
        }

        private async Task OnLoadedAsync()
        {
            var auth0Id = _auth0Service.GetUserId(_currentLoginResult);
            var currentUser = await _userService.GetUserByAuth0IdAsync(auth0Id);
            _currentUserId = currentUser.Id;
            var projectUser = await _projectService.GetProjectUserAsync(currentUser.Id, _projectId);

            if (projectUser?.Role == Role.Moderator)
            {
                IsManageModeratorsVisible = true;
                IsEditProjectVisible = true;
            }

            await LoadProjectDetailsAsync();
            await LoadProjectUsersForFilterAsync();
        }

        private async Task LoadProjectDetailsAsync(int? userId = null)
        {
            try
            {
                var tasks = await _taskFilterService.GetTasksForProjectAsync(_projectId, userId);
                Tasks = new ObservableCollection<TaskEntity>(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження деталей проекту: {ex.Message}", "Помилка");
            }
        }

        private async Task LoadProjectUsersForFilterAsync()
        {
            try
            {
                var users = await _userService.GetUsersByProjectIdAsync(_projectId);
                var filterItems = users.Select(u => new UserFilterItem
                {
                    Id = u.Id,
                    Username = $"{u.FirstName} {u.LastName}"
                }).ToList();
                UserFilterItems = new ObservableCollection<UserFilterItem>(filterItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження користувачів проєкту: {ex.Message}", "Помилка");
            }
        }

        private void OnClose()
        {
            SysApp.Current.Windows.OfType<ProjectDetailsWindow>().FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        private async Task OnClearFilterAsync()
        {
            SelectedUserFilter = null;
        }

        private async Task OnAddTaskAsync()
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
                UserProjects = new ObservableCollection<Project>(userProjects);
                SelectedProjectId = _projectId;

                ClearTaskForm();
                IsTaskModalVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearTaskForm()
        {
            TaskTitle = string.Empty;
            TaskDescription = string.Empty;
            TaskDueDate = DateTime.Now.AddDays(1);
            _selectedAssigneeIds.Clear();
            SelectedAssigneesText = "Виконавці: не обрано";
        }

        private async Task OnAssignUsersAsync()
        {
            if (SelectedProjectId == 0)
            {
                MessageBox.Show("Будь ласка, спочатку виберіть проект.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var projectUsers = await _userService.GetUsersByProjectIdAsync(SelectedProjectId);

                var userSelectionVM = projectUsers.Select(u => new UserSelectionViewModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    IsSelected = _selectedAssigneeIds.Contains(u.Id)
                }).ToList();

                UsersForAssignment = new ObservableCollection<UserSelectionViewModel>(userSelectionVM);
                IsAssignUsersModalVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження користувачів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        private void OnSaveAssignUsers()
        {
            _selectedAssigneeIds = UsersForAssignment
  .Where(u => u.IsSelected)
      .Select(u => u.Id)
.ToList();

            SelectedAssigneesText = _selectedAssigneeIds.Count > 0
       ? $"Виконавці: обрано {_selectedAssigneeIds.Count}"
           : "Виконавці: не обрано";

            IsAssignUsersModalVisible = false;
        }

        private void OnCancelAssignUsers()
        {
            IsAssignUsersModalVisible = false;
        }

        private void OnCancelTask()
        {
            IsTaskModalVisible = false;
        }

        private async Task OnEditTaskAsync(object parameter)
        {
            if (parameter is not int taskId) return;

            _editingTaskId = taskId;

            try
            {
                var task = await _taskService.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    MessageBox.Show("Завдання не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                EditTaskTitle = task.Title;
                EditTaskDescription = task.Description;
                EditTaskDueDate = task.DueDate;

                IsEditTaskModalVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні завдання: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelEditTask()
        {
            IsEditTaskModalVisible = false;
            ClearEditTaskFields();
        }

        private void ClearEditTaskFields()
        {
            EditTaskTitle = string.Empty;
            EditTaskDescription = string.Empty;
            EditTaskDueDate = DateTime.Now.AddDays(1);
            _editingTaskId = 0;
        }

        private async Task OnDeleteTaskAsync(object parameter)
        {
            if (parameter is not int taskId) return;

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
                        await LoadProjectDetailsAsync();
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

        private void OnAddUserToProject()
        {
            AddUserEmail = string.Empty;
            IsAddUserModalVisible = true;
        }

        private void OnCancelAddUser()
        {
            IsAddUserModalVisible = false;
        }

        private async Task OnSaveAddUserAsync()
        {
            var email = AddUserEmail?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Будь ласка, введіть email користувача.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    MessageBox.Show($"Користувача з email '{email}' не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var alreadyInProject = await _userService.IsUserInProjectAsync(user.Id, _projectId);
                if (alreadyInProject)
                {
                    MessageBox.Show($"Користувач вже є учасником проекту.", "Увага", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                await _userService.AddUserToProjectAsync(user.Id, _projectId, Role.Member);
                MessageBox.Show($"Користувача '{user.FirstName} {user.LastName}' додано до проекту!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                IsAddUserModalVisible = false;
                await LoadProjectDetailsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні користувача: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnManageModeratorsAsync()
        {
            try
            {
                var projectUsers = await _userService.GetUsersByProjectIdAsync(_projectId);
                var auth0Id = _auth0Service.GetUserId(_currentLoginResult);
                var currentUser = await _userService.GetUserByAuth0IdAsync(auth0Id);

                var moderatorVms = new List<ModeratorSelectionViewModel>();
                foreach (var user in projectUsers)
                {
                    var projectUser = await _projectService.GetProjectUserAsync(user.Id, _projectId);
                    moderatorVms.Add(new ModeratorSelectionViewModel
                    {
                        Id = user.Id,
                        FullName = $"{user.FirstName} {user.LastName}",
                        IsModerator = projectUser?.Role == Role.Moderator,
                        CanChange = user.Id != currentUser.Id
                    });
                }

                Moderators = new ObservableCollection<ModeratorSelectionViewModel>(moderatorVms);
                IsManageModeratorsModalVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження користувачів: {ex.Message}", "Помилка");
            }
        }

        private async Task OnSaveModeratorsAsync()
        {
            try
            {
                foreach (var userRole in Moderators)
                {
                    if (!userRole.CanChange) continue;

                    var newRole = userRole.IsModerator ? Role.Moderator : Role.Member;
                    await _userService.UpdateUserRoleInProjectAsync(userRole.Id, _projectId, newRole);
                }

                MessageBox.Show("Ролі модераторів успішно оновлено.", "Успіх");
                IsManageModeratorsModalVisible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні ролей: {ex.Message}", "Помилка");
            }
        }

        private void OnCancelModerators()
        {
            IsManageModeratorsModalVisible = false;
        }

        private void OnEditProject()
        {
            EditProjectName = _projectDto.Name;
            EditProjectDescription = _projectDto.Description;
            EditProjectStatus = _projectDto.Status;

            IsEditProjectModalVisible = true;
        }

        private async Task OnSaveEditProjectAsync()
        {
            var newName = EditProjectName?.Trim();
            var newDescription = EditProjectDescription?.Trim();
            var newStatus = EditProjectStatus?.Trim();

            if (string.IsNullOrWhiteSpace(newName) || string.IsNullOrWhiteSpace(newStatus))
            {
                MessageBox.Show("Назва та Статус проєкту не можуть бути порожніми.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var updatedProject = await _projectService.UpdateProjectAsync(_projectId, newName, newDescription, newStatus);

                _projectDto.Name = updatedProject.Name;
                _projectDto.Description = updatedProject.Description;
                _projectDto.Status = updatedProject.Status;

                ProjectName = _projectDto.Name;
                ProjectDescription = _projectDto.Description ?? "Опис відсутній";
                ProjectStatus = $"Статус: {_projectDto.Status} | Ваша роль: {_projectDto.UserRoleInProject}";

                MessageBox.Show("Проєкт успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditProjectModalVisible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення проєкту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelEditProject()
        {
            IsEditProjectModalVisible = false;
        }

        private async Task OnLeaveProjectAsync()
        {
            var confirmation = MessageBox.Show(
                "Ви впевнені, що хочете покинути цей проект?",
                "Підтвердження виходу",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation == MessageBoxResult.No)
                return;
            
            try
            {
                var projectUsers = await _projectService.GetProjectUsersAsync(_projectId);
                var currentUserProjectEntry = projectUsers.FirstOrDefault(pu => pu.UserId == _currentUserId);

                if (currentUserProjectEntry == null) return;

                if (currentUserProjectEntry.Role == Role.Member)
                {
                    await _userService.RemoveUserFromProjectAsync(_currentUserId, _projectId);
                    MessageBox.Show("Ви успішно покинули проект.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnClose(); 
                    return;
                }

                var otherModerators = projectUsers.Count(pu => pu.Role == Role.Moderator && pu.UserId != _currentUserId);

                if (otherModerators > 0)
                {
                    await _userService.RemoveUserFromProjectAsync(_currentUserId, _projectId);
                    MessageBox.Show("Ви успішно покинули проект.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnClose(); 
                    return;
                }

                var otherUsers = projectUsers
                    .Where(pu => pu.UserId != _currentUserId)
                    .Select(pu => new PromoteUserViewModel
                    {
                        Id = pu.UserId,
                        FullName = $"{pu.User.FirstName} {pu.User.LastName}",
                        IsSelected = false
                    })
                    .ToList();

                if (!otherUsers.Any())
                {
                    await _userService.RemoveUserFromProjectAsync(_currentUserId, _projectId);
                    MessageBox.Show("Ви успішно покинули проект (ви були останнім учасником).", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnClose();
                    return;
                }

                UsersToPromote = new ObservableCollection<PromoteUserViewModel>(otherUsers);
                IsLeaveProjectModalVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при виході з проекту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnSaveAndLeaveProjectAsync()
        {
            var selectedUsers = UsersToPromote.Where(u => u.IsSelected).ToList();
            if (!selectedUsers.Any())
            {
                MessageBox.Show("Ви повинні обрати хоча б одного користувача, щоб зробити його модератором.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                foreach (var user in selectedUsers)
                {
                    await _userService.UpdateUserRoleInProjectAsync(user.Id, _projectId, Role.Moderator);
                }


                await _userService.RemoveUserFromProjectAsync(_currentUserId, _projectId);

                MessageBox.Show("Ви успішно передали права модератора та покинули проект.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                IsLeaveProjectModalVisible = false;
                OnClose(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelLeaveProject()
        {
            IsLeaveProjectModalVisible = false;
            UsersToPromote.Clear();
        }

        private async Task OnDeleteProjectAsync()
        {
            var confirm1 = MessageBox.Show(
                "Ви дійсно хочете видалити цей проект? Ця дія незворотня і видалить всі пов'язані з ним завдання.",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm1 == MessageBoxResult.No)
                return;

            var confirm2 = MessageBox.Show(
                "ВИ ВПЕВНЕНІ? Проект буде видалено НАЗАВЖДИ.",
                "Остаточне підтвердження",
                MessageBoxButton.YesNo,
                MessageBoxImage.Stop);

            if (confirm2 == MessageBoxResult.No)
                return;

            try
            {
                await _projectService.DeleteProjectAsync(_projectId);

                MessageBox.Show("Проект успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                IsEditProjectModalVisible = false;
                OnClose(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час видалення проекту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnOpenKickUserModalAsync()
        {
            try
            {
                // Отримуємо всіх користувачів проекту
                var users = await _userService.GetUsersByProjectIdAsync(_projectId);

                // Фільтруємо: виключаємо поточного користувача (себе вигнати не можна)
                // Модератор може вигнати іншого модератора, тому перевірку ролі тут не робимо, 
                // лише перевірку ID, щоб не видалити самого себе.
                var kickableUsers = users.Where(u => u.Id != _currentUserId).ToList();

                if (!kickableUsers.Any())
                {
                    MessageBox.Show("У цьому проекті немає інших учасників, яких можна вигнати.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                UsersToKickList = new ObservableCollection<UserDto>(kickableUsers);
                IsKickUserModalVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження списку учасників: {ex.Message}", "Помилка");
            }
        }

        private void OnCloseKickUserModal()
        {
            IsKickUserModalVisible = false;
        }
    }

    public class UserSelectionViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ModeratorSelectionViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public bool IsModerator { get; set; }
        public bool CanChange { get; set; }
    }

    public class UserFilterItem
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }

    public class PromoteUserViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
