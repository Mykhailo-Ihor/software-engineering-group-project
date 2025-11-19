using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF
{
    public partial class PasswordManagerWindow : Window
    {
        public PasswordManagerWindow(IPasswordService passwordService, Auth0Service auth0Service, IUserService userService, LoginResult? loginResult)
        {
            InitializeComponent();

            // Create and set the ViewModel as DataContext
            var viewModel = new PasswordManagerViewModel(passwordService, userService, auth0Service, loginResult);
            DataContext = viewModel;

            // Trigger the LoadPasswordsCommand immediately after setting DataContext
            if (viewModel.LoadPasswordsCommand.CanExecute(null))
            {
                viewModel.LoadPasswordsCommand.Execute(null);
            }
        }
    }
}