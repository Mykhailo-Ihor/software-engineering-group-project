using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.Application.Services;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF
{
    public partial class ProjectsSummaryWindow : Window
    {
    private readonly ProjectsSummaryViewModel _viewModel;

        public ProjectsSummaryWindow(
            IProjectService projectService,
IUserService userService,
    Auth0Service auth0Service,
            LoginResult currentLoginResult,
 ITaskService taskService,
            ITaskFilterService taskFilterService)
  {
            InitializeComponent();

            _viewModel = new ProjectsSummaryViewModel(
      projectService,
     userService,
    auth0Service,
        currentLoginResult,
      taskService,
                taskFilterService);

       DataContext = _viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
  {
            if (_viewModel.LoadedCommand.CanExecute(null))
          {
     await ((Commands.AsyncRelayCommand)_viewModel.LoadedCommand).ExecuteAsync(null);
      }
        }
    }
}
