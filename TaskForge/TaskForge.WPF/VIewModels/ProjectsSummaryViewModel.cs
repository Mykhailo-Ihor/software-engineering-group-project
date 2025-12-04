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
using TaskForge.WPF.Commands;
using SysApp = System.Windows.Application;

namespace TaskForge.WPF.ViewModels
{
    public class ProjectsSummaryViewModel : ViewModelBase
    {
        private readonly IProjectService _projectService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
   private readonly LoginResult _currentLoginResult;
     private readonly ITaskService _taskService;
        private readonly ITaskFilterService _taskFilterService;
        private int _currentUserId;

      // Store all projects for filtering
      private List<ProjectDto> _allProjects = new();

     private ObservableCollection<ProjectDto> _projects;
        public ObservableCollection<ProjectDto> Projects
     {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

     private bool _isProjectsListVisible;
        public bool IsProjectsListVisible
        {
     get => _isProjectsListVisible;
   set => SetProperty(ref _isProjectsListVisible, value);
        }

        // Search/Filter Properties
        private string _searchText = string.Empty;
        public string SearchText
        {
      get => _searchText;
            set
        {
     if (SetProperty(ref _searchText, value))
        {
   ApplyFilter();
       }
          }
        }

        private bool _isSearchVisible;
   public bool IsSearchVisible
        {
        get => _isSearchVisible;
      set => SetProperty(ref _isSearchVisible, value);
 }

        // Header bindings
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

        // Add Project Modal Properties
        private bool _isAddProjectModalVisible;
        public bool IsAddProjectModalVisible
        {
            get => _isAddProjectModalVisible;
       set => SetProperty(ref _isAddProjectModalVisible, value);
      }

    private string _addProjectName;
        public string AddProjectName
        {
   get => _addProjectName;
            set => SetProperty(ref _addProjectName, value);
        }

     private string _addProjectDescription;
        public string AddProjectDescription
        {
       get => _addProjectDescription;
     set => SetProperty(ref _addProjectDescription, value);
     }

        // Edit Project Modal Properties
        private bool _isEditProjectModalVisible;
        public bool IsEditProjectModalVisible
   {
          get => _isEditProjectModalVisible;
            set => SetProperty(ref _isEditProjectModalVisible, value);
      }

        private int _editingProjectId;

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

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand CloseCommand { get; }
public ICommand AddProjectCommand { get; }
        public ICommand SaveAddProjectCommand { get; }
        public ICommand CancelAddProjectCommand { get; }
        public ICommand EditProjectCommand { get; }
      public ICommand SaveEditProjectCommand { get; }
        public ICommand CancelEditProjectCommand { get; }
      public ICommand LeaveProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
      public ICommand OpenProjectDetailsCommand { get; }
  public ICommand ProfileCommand { get; }
 public ICommand LogoutCommand { get; }
        public ICommand ToggleSearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public ProjectsSummaryViewModel(
       IProjectService projectService,
 IUserService userService,
            Auth0Service auth0Service,
            LoginResult currentLoginResult,
            ITaskService taskService,
   ITaskFilterService taskFilterService)
        {
     _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
       _userService = userService ?? throw new ArgumentNullException(nameof(userService));
   _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
       _currentLoginResult = currentLoginResult ?? throw new ArgumentNullException(nameof(currentLoginResult));
         _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
   _taskFilterService = taskFilterService ?? throw new ArgumentNullException(nameof(taskFilterService));

_projects = new ObservableCollection<ProjectDto>();
            _userName = string.Empty;
         _userEmail = string.Empty;
     _addProjectName = string.Empty;
    _addProjectDescription = string.Empty;
        _editProjectName = string.Empty;
     _editProjectDescription = string.Empty;
        _editProjectStatus = "Active";
     _userAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));

            // Initialize commands
        LoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
        CloseCommand = new RelayCommand(OnClose);
            AddProjectCommand = new RelayCommand(OnAddProject);
            SaveAddProjectCommand = new AsyncRelayCommand(OnSaveAddProjectAsync);
            CancelAddProjectCommand = new RelayCommand(OnCancelAddProject);
            EditProjectCommand = new AsyncRelayCommand(OnEditProjectAsync);
   SaveEditProjectCommand = new AsyncRelayCommand(OnSaveEditProjectAsync);
            CancelEditProjectCommand = new RelayCommand(OnCancelEditProject);
     LeaveProjectCommand = new AsyncRelayCommand(OnLeaveProjectAsync);
    DeleteProjectCommand = new AsyncRelayCommand(OnDeleteProjectAsync);
            OpenProjectDetailsCommand = new AsyncRelayCommand(OnOpenProjectDetailsAsync);
            ProfileCommand = new AsyncRelayCommand(OnProfileAsync);
            LogoutCommand = new AsyncRelayCommand(OnLogoutAsync);
 ToggleSearchCommand = new RelayCommand(OnToggleSearch);
         ClearSearchCommand = new RelayCommand(OnClearSearch);

  // Set user info from login result
            LoadUserInfo();
        }

    private void LoadUserInfo()
 {
            if (_currentLoginResult != null && !_currentLoginResult.IsError)
     {
       UserName = _auth0Service.GetUserName(_currentLoginResult) ?? "User";
          UserEmail = _auth0Service.GetUserEmail(_currentLoginResult) ?? "";
              var avatarUrl = _auth0Service.GetUserAvatarUrl(_currentLoginResult);
                if (!string.IsNullOrEmpty(avatarUrl))
{
      UserAvatar = new BitmapImage(new Uri(avatarUrl));
        }
            }
        }

        private async Task OnLoadedAsync()
        {
        await LoadUserProjectsAsync();
        }

   private async Task LoadUserProjectsAsync()
        {
       try
      {
          if (_currentLoginResult == null || _currentLoginResult.IsError)
     {
         MessageBox.Show("Помилка автентифікації.", "Помилка");
        CloseWindow();
            return;
    }

         var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
    var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
    if (user == null)
       {
         MessageBox.Show("Не вдалося знайти ваші дані в системі.", "Помилка");
 CloseWindow();
   return;
        }

           _currentUserId = user.Id;

      var userProjects = await _projectService.GetUserProjectsAsync(user.Id);
     _allProjects = userProjects.ToList();
      
      ApplyFilter();
  }
            catch (Exception ex)
            {
    MessageBox.Show($"Помилка завантаження проектів: {ex.Message}", "Помилка");
    }
    }

 private void ApplyFilter()
        {
 IEnumerable<ProjectDto> filtered = _allProjects;

    if (!string.IsNullOrWhiteSpace(SearchText))
{
        var searchLower = SearchText.Trim().ToLowerInvariant();
                filtered = _allProjects.Where(p => 
          p.Name.ToLowerInvariant().Contains(searchLower) ||
       (p.Description?.ToLowerInvariant().Contains(searchLower) ?? false));
            }

        Projects = new ObservableCollection<ProjectDto>(filtered);
   IsProjectsListVisible = Projects.Any();
    }

        private void OnToggleSearch()
        {
            IsSearchVisible = !IsSearchVisible;
            if (!IsSearchVisible)
  {
   SearchText = string.Empty;
            }
        }

        private void OnClearSearch()
        {
    SearchText = string.Empty;
        }

        private void CloseWindow()
        {
          SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        private void OnClose()
        {
        CloseWindow();
   }

        #region Add Project

        private void OnAddProject()
  {
   AddProjectName = string.Empty;
            AddProjectDescription = string.Empty;
    IsAddProjectModalVisible = true;
        }

private async Task OnSaveAddProjectAsync()
        {
            var name = AddProjectName?.Trim();
      var description = AddProjectDescription?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
     MessageBox.Show("Будь ласка, введіть назву проекту.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
    return;
   }

      try
     {
        await _projectService.CreateProjectForUserAsync(name, "Active", description, _currentUserId, Domain.Enums.Role.Moderator);
    MessageBox.Show($"Проект '{name}' успішно створено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
   IsAddProjectModalVisible = false;
    await LoadUserProjectsAsync();
        }
            catch (Exception ex)
            {
       MessageBox.Show($"Помилка при створенні проекту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

     private void OnCancelAddProject()
  {
          IsAddProjectModalVisible = false;
      }

        #endregion

    #region Edit Project

        private async Task OnEditProjectAsync(object? parameter)
        {
            if (parameter is not int projectId) return;

     _editingProjectId = projectId;
          var project = Projects.FirstOrDefault(p => p.Id == projectId);
     if (project == null)
        {
        MessageBox.Show("Проект не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
   return;
 }

     EditProjectName = project.Name;
    EditProjectDescription = project.Description ?? string.Empty;
     EditProjectStatus = project.Status ?? "Active";
    IsEditProjectModalVisible = true;
        }

     private async Task OnSaveEditProjectAsync()
        {
    var name = EditProjectName?.Trim();
            var description = EditProjectDescription?.Trim();
            var status = EditProjectStatus?.Trim() ?? "Active";

            if (string.IsNullOrWhiteSpace(name))
    {
     MessageBox.Show("Будь ласка, введіть назву проекту.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
         return;
    }

            try
        {
      await _projectService.UpdateProjectAsync(_editingProjectId, name, description ?? string.Empty, status);
            MessageBox.Show("Проект успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            IsEditProjectModalVisible = false;
              await LoadUserProjectsAsync();
        }
       catch (Exception ex)
            {
 MessageBox.Show($"Помилка при оновленні проекту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
        }

        private void OnCancelEditProject()
        {
   IsEditProjectModalVisible = false;
        }

        #endregion

        #region Leave Project

        private async Task OnLeaveProjectAsync(object? parameter)
        {
            if (parameter is not int projectId) return;

      var project = Projects.FirstOrDefault(p => p.Id == projectId);
  if (project == null) return;

            var result = MessageBox.Show(
  $"Ви впевнені, що хочете покинути проект '{project.Name}'?",
     "Підтвердження",
                MessageBoxButton.YesNo,
     MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
              try
      {
            // Get the project users and find the current user's association
  var projectUsers = await _projectService.GetProjectUsersAsync(projectId);
   var currentProjectUser = projectUsers.FirstOrDefault(pu => pu.UserId == _currentUserId);

        if (currentProjectUser != null)
                 {
          // For now, show a message that leaving is not directly supported via service
              // The user should be removed through the project details or by the moderator
             MessageBox.Show("Для виходу з проекту зверніться до модератора проекту.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
     }
             else
          {
     MessageBox.Show("Ви не є учасником цього проекту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
         }
                }
      catch (Exception ex)
  {
      MessageBox.Show($"Помилка при виході з проекту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
         }
     }
      }

        #endregion

        #region Delete Project

   private async Task OnDeleteProjectAsync(object? parameter)
        {
            if (parameter is not int projectId) return;

   var project = Projects.FirstOrDefault(p => p.Id == projectId);
      if (project == null) return;

     var result = MessageBox.Show(
  $"Ви впевнені, що хочете видалити проект '{project.Name}'? Цю дію неможливо скасувати.",
      "Підтвердження видалення",
     MessageBoxButton.YesNo,
     MessageBoxImage.Warning);

if (result == MessageBoxResult.Yes)
      {
     try
  {
       await _projectService.DeleteProjectAsync(projectId);
         MessageBox.Show("Проект успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
      await LoadUserProjectsAsync();
       }
  catch (Exception ex)
    {
         MessageBox.Show($"Помилка при видаленні проекту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
         }
            }
     }

        #endregion

        #region Open Project Details

        private async Task OnOpenProjectDetailsAsync(object? parameter)
        {
            if (parameter is not int projectId) return;

       var selectedProject = Projects.FirstOrDefault(p => p.Id == projectId);
          if (selectedProject == null)
      {
      MessageBox.Show("Не вдалося знайти проект.", "Помилка");
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
            await LoadUserProjectsAsync();
        }

        #endregion

    #region Header Commands

        private async Task OnProfileAsync()
 {
       var profileWindow = new ProfileWindow(_currentLoginResult, _auth0Service, _userService);
        var currentWindow = SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
         if (currentWindow != null)
         {
 profileWindow.Owner = currentWindow;
            }
            profileWindow.ShowDialog();

            // Refresh user info after profile update
            if (_currentLoginResult != null && !_currentLoginResult.IsError)
       {
          var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
          var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user != null)
        {
         UserName = $"{user.FirstName} {user.LastName}";
            UserEmail = user.Email;
     }
}
      }

        private async Task OnLogoutAsync()
        {
       try
            {
   await _auth0Service.LogoutAsync();
      CloseWindow();
        }
    catch (Exception ex)
  {
  MessageBox.Show($"Помилка при виході: {ex.Message}", "Помилка виходу",
  MessageBoxButton.OK, MessageBoxImage.Error);
   }
        }

        #endregion
    }
}
