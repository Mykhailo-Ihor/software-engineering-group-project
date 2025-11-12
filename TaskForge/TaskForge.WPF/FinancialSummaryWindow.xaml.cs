using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF
{
    public partial class FinancialSummaryWindow : Window
    {
        private readonly FinancialSummaryViewModel _viewModel;

        public FinancialSummaryWindow(
            IExpenseService expenseService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult currentLoginResult)
        {
            InitializeComponent();

            // Set DataContext to ViewModel
            _viewModel = new FinancialSummaryViewModel(
                   expenseService,
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