using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF
{
    public partial class SubscriptionSummaryWindow : Window
    {
        private readonly SubscriptionSummaryViewModel _viewModel;

        public SubscriptionSummaryWindow(
            ISubscriptionService subscriptionService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult currentLoginResult)
        {
            InitializeComponent();

            _viewModel = new SubscriptionSummaryViewModel(
                subscriptionService,
                userService,
                auth0Service,
                currentLoginResult);

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