using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF
{
    public partial class PasswordManagerWindow : Window
    {
        private PasswordManagerViewModel _viewModel;

        public PasswordManagerWindow(IPasswordService passwordService, Auth0Service auth0Service, IUserService userService, LoginResult? loginResult)
        {
            InitializeComponent();

            // Create and set the ViewModel as DataContext
            _viewModel = new PasswordManagerViewModel(passwordService, userService, auth0Service, loginResult);
            DataContext = _viewModel;

            _viewModel.GetAddPassword = () => AddPasswordBox.Password;
            _viewModel.GetEditPassword = () => EditPasswordBox.Password;

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // Trigger the LoadPasswordsCommand immediately after setting DataContext
            if (_viewModel.LoadPasswordsCommand.CanExecute(null))
            {
                _viewModel.LoadPasswordsCommand.Execute(null);
            }
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PasswordManagerViewModel.IsAddModalVisible) && _viewModel.IsAddModalVisible)
            {
                AddPasswordBox.Clear();
            }
            else if (e.PropertyName == nameof(PasswordManagerViewModel.IsEditModalVisible) && _viewModel.IsEditModalVisible)
            {
                EditPasswordBox.Password = _viewModel.PasswordText ?? "";
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}