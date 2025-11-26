using System;
using System.Threading.Tasks;
using System.Windows;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.FinancialSummary
{
    public class DeleteExpenseCommand : AsyncRelayCommand
    {
        private readonly FinancialSummaryViewModel _viewModel;
        private readonly IExpenseService _expenseService;

        public DeleteExpenseCommand(
            FinancialSummaryViewModel viewModel,
            IExpenseService expenseService)
            : base(async parameter => await ExecuteDeleteExpenseAsync(viewModel, expenseService, parameter))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
        }

        private static async Task ExecuteDeleteExpenseAsync(
            FinancialSummaryViewModel viewModel,
            IExpenseService expenseService,
            object? parameter)
        {
            if (parameter is not int expenseId) return;

            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити цей запис про витрату?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return; 

            try
            {
                var success = await expenseService.DeleteExpenseAsync(expenseId);

                if (success)
                {
                    MessageBox.Show("Витрату успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (viewModel.LoadedCommand.CanExecute(null))
                    {
                        await ((AsyncRelayCommand)viewModel.LoadedCommand).ExecuteAsync(null);
                    }
                } 
                else
                {
                    MessageBox.Show("Не вдалося знайти витрату для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}