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
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for ProjectDetailsWindow.xaml
    /// </summary>
    public partial class ProjectDetailsWindow : Window
    {
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
            DataContext = new ProjectDetailsViewModel(
                projectId,
                projectDto,
                projectService,
                taskService,
                userService,
                auth0Service,
                currentLoginResult,
                taskFilterService
            );
        }
    }
}
