using System;
using System.Threading.Tasks;
using System.Windows;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.FinancialSummary
{
    public class AddExpenseCommand : AsyncRelayCommand
    {
        private readonly FinancialSummaryViewModel _viewModel;
        private readonly IExpenseService _expenseService;

        public AddExpenseCommand(
            FinancialSummaryViewModel viewModel,
            IExpenseService expenseService)
            : base(async _ => await ExecuteAddExpenseAsync(viewModel, expenseService))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
        }

        private static async Task ExecuteAddExpenseAsync(
            FinancialSummaryViewModel viewModel,
            IExpenseService expenseService)
        {
            if (string.IsNullOrWhiteSpace(viewModel.AddExpenseAmount))
            {
                MessageBox.Show("Будь ласка, введіть суму витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(viewModel.AddExpenseAmount, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну суму (число більше 0).", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var description = viewModel.AddExpenseDescription?.Trim() ?? string.Empty;

                await expenseService.CreateExpenseAsync(
                    amount,
                    viewModel.AddExpenseCurrency,
                    viewModel.AddExpenseCategory,
                    viewModel.AddExpenseDate,
                    description,
                    viewModel.CurrentUserId);

                MessageBox.Show("Витрату успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                viewModel.IsAddExpenseModalVisible = false;

                if (viewModel.LoadedCommand.CanExecute(null))
                {
                    await ((AsyncRelayCommand)viewModel.LoadedCommand).ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні витрати: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}