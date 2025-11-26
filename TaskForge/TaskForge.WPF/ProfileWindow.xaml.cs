using System.Windows;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow(Duende.IdentityModel.OidcClient.LoginResult loginResult, Auth0Service auth0Service, IUserService userService)
        {
            InitializeComponent();

            var viewModel = new ProfileViewModel(loginResult, auth0Service, userService);
            viewModel.CloseRequested += () => this.Close();

            DataContext = viewModel;
        }
    }
}
