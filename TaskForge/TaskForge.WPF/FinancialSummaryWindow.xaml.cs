using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;

namespace TaskForge.WPF
{
    public partial class FinancialSummaryWindow : Window
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult _currentLoginResult;

        public FinancialSummaryWindow(IExpenseService expenseService, IUserService userService, Auth0Service auth0Service, LoginResult currentLoginResult)
        {
            InitializeComponent();
            _expenseService = expenseService;
            _userService = userService;
            _auth0Service = auth0Service;
            _currentLoginResult = currentLoginResult;

            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserExpenses();
        }

        private async Task LoadUserExpenses()
        {
            try
            {
                if (_currentLoginResult == null || _currentLoginResult.IsError)
                {
                    MessageBox.Show("Помилка автентифікації.", "Помилка");
                    this.Close();
                    return;
                }

                var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
                var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user == null)
                {
                    MessageBox.Show("Не вдалося знайти ваші дані в системі.", "Помилка");
                    this.Close();
                    return;
                }

                var userExpenses = await _expenseService.GetUserExpensesAsync(user.Id);
                ExpensesListView.ItemsSource = userExpenses;

                if (!userExpenses.Any())
                {
                    // Можна показати TextBlock замість списку, якщо він порожній
                    ExpensesListView.Visibility = Visibility.Collapsed;
                    // (Додатково) сюди можна додати TextBlock з повідомленням "Витрат не знайдено"
                }
                else
                {
                    ExpensesListView.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження витрат: {ex.Message}", "Помилка");
            }
        }

        private async void DeleteExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null) return;

            var expenseId = (int)button.Tag;

            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити цей запис про витрату?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _expenseService.DeleteExpenseAsync(expenseId);
                    if (success)
                    {
                        MessageBox.Show("Витрату успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                        // Оновлюємо список
                        await LoadUserExpenses();
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}