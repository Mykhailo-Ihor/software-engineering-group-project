using System;
using System.Threading.Tasks;
using System.Windows;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.FinancialSummary
{
    public class LoadExpensesCommand : AsyncRelayCommand
    {
        private readonly FinancialSummaryViewModel _viewModel;
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult? _loginResult;

        public LoadExpensesCommand(
            FinancialSummaryViewModel viewModel,
            IExpenseService expenseService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult? loginResult)
            : base(async _ => await ExecuteLoadExpensesAsync(viewModel, expenseService, userService, auth0Service, loginResult))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _loginResult = loginResult;
        }

        private static async Task ExecuteLoadExpensesAsync(
            FinancialSummaryViewModel viewModel,
            IExpenseService expenseService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult? loginResult)
        {
            if (loginResult == null || loginResult.IsError)
            {
                MessageBox.Show("Будь ласка, увійдіть, щоб переглянути витрати.", "Потрібна автентифікація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var auth0UserId = auth0Service.GetUserId(loginResult);
            var user = await userService.GetUserByAuth0IdAsync(auth0UserId);

            if (user == null)
            {
                MessageBox.Show("Користувача не знайдено в системі.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            viewModel.CurrentUserId = user.Id;
            var userExpenses = await expenseService.GetUserExpensesAsync(user.Id);
            viewModel.Expenses.Clear(); 

            foreach (var expense in userExpenses)
            {
                viewModel.Expenses.Add(expense);
            }

            viewModel.IsExpensesListVisible = viewModel.Expenses.Count > 0;
        }
    }
}