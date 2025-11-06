using Duende.IdentityModel.OidcClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Enums;

namespace TaskForge.WPF
{
    public partial class FinancialSummaryWindow : Window
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult _currentLoginResult;
        private int _editingExpenseId;
        private int _currentUserId;

        public FinancialSummaryWindow(IExpenseService expenseService, IUserService userService, Auth0Service auth0Service, LoginResult currentLoginResult)
        {
            InitializeComponent();
            _expenseService = expenseService;
            _userService = userService;
            _auth0Service = auth0Service;
            _currentLoginResult = currentLoginResult;
            InitializeComboBoxes();

            Loaded += Window_Loaded;
        }

        private void InitializeComboBoxes()
        {
            AddExpenseCurrencyBox.ItemsSource = Enum.GetValues(typeof(Currency));
            AddExpenseCategoryBox.ItemsSource = Enum.GetValues(typeof(ExpenceCategory));

            EditExpenseCurrencyBox.ItemsSource = Enum.GetValues(typeof(Currency));
            EditExpenseCategoryBox.ItemsSource = Enum.GetValues(typeof(ExpenceCategory));
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

                _currentUserId = user.Id;

                var userExpenses = await _expenseService.GetUserExpensesAsync(user.Id);
                ExpensesListView.ItemsSource = userExpenses;

                if (!userExpenses.Any())
                {
                    ExpensesListView.Visibility = Visibility.Collapsed;
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

        // ============ ADD EXPENSE ============
        private void AddExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAddExpenseFields();
            AddExpenseModalOverlay.Visibility = Visibility.Visible;
        }

        private async void AddExpenseModalOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddExpenseAmountBox.Text))
                {
                    MessageBox.Show("Будь ласка, введіть суму витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(AddExpenseAmountBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Будь ласка, введіть коректну суму (число більше 0).", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (AddExpenseCurrencyBox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, виберіть валюту.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (AddExpenseCategoryBox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, виберіть категорію.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!AddExpenseDateBox.SelectedDate.HasValue)
                {
                    MessageBox.Show("Будь ласка, виберіть дату витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var currency = (Currency)AddExpenseCurrencyBox.SelectedItem;
                var category = (ExpenceCategory)AddExpenseCategoryBox.SelectedItem;
                var date = AddExpenseDateBox.SelectedDate.Value;
                var description = AddExpenseDescriptionBox.Text.Trim();

                await _expenseService.CreateExpenseAsync(amount, currency, category, date, description, _currentUserId);

                MessageBox.Show("Витрату успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                AddExpenseModalOverlay.Visibility = Visibility.Collapsed;

                await LoadUserExpenses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні витрати: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddExpenseModalCancel_Click(object sender, RoutedEventArgs e)
        {
            AddExpenseModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ClearAddExpenseFields()
        {
            AddExpenseAmountBox.Text = string.Empty;
            AddExpenseCurrencyBox.SelectedIndex = 0;
            AddExpenseCategoryBox.SelectedIndex = 0;
            AddExpenseDateBox.SelectedDate = DateTime.Today;
            AddExpenseDescriptionBox.Text = string.Empty;
        }

        // ============ EDIT EXPENSE ============
        private async void EditExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null) return;

            var expenseId = (int)button.Tag;
            _editingExpenseId = expenseId;

            try
            {
                var expense = await _expenseService.GetExpenseByIdAsync(expenseId);
                if (expense == null)
                {
                    MessageBox.Show("Витрату не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                EditExpenseAmountBox.Text = expense.Amount.ToString("F2");
                EditExpenseCurrencyBox.SelectedItem = expense.Currency;
                EditExpenseCategoryBox.SelectedItem = expense.Category;
                EditExpenseDateBox.SelectedDate = expense.Date;
                EditExpenseDescriptionBox.Text = expense.Description;

                EditExpenseModalOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні витрати: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditExpenseModalOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditExpenseAmountBox.Text))
                {
                    MessageBox.Show("Будь ласка, введіть суму витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(EditExpenseAmountBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Будь ласка, введіть коректну суму (число більше 0).", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (EditExpenseCurrencyBox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, виберіть валюту.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (EditExpenseCategoryBox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, виберіть категорію.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!EditExpenseDateBox.SelectedDate.HasValue)
                {
                    MessageBox.Show("Будь ласка, виберіть дату витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var expense = await _expenseService.GetExpenseByIdAsync(_editingExpenseId);
                if (expense == null)
                {
                    MessageBox.Show("Витрату не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    EditExpenseModalOverlay.Visibility = Visibility.Collapsed;
                    return;
                }

                expense.Amount = amount;
                expense.Currency = (Currency)EditExpenseCurrencyBox.SelectedItem;
                expense.Category = (ExpenceCategory)EditExpenseCategoryBox.SelectedItem;
                expense.Date = EditExpenseDateBox.SelectedDate.Value;
                expense.Description = EditExpenseDescriptionBox.Text.Trim();

                await _expenseService.UpdateExpenseAsync(expense);

                MessageBox.Show("Витрату успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                EditExpenseModalOverlay.Visibility = Visibility.Collapsed;
                ClearEditExpenseFields();

                await LoadUserExpenses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні витрати: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditExpenseModalCancel_Click(object sender, RoutedEventArgs e)
        {
            EditExpenseModalOverlay.Visibility = Visibility.Collapsed;
            ClearEditExpenseFields();
        }

        private void ClearEditExpenseFields()
        {
            EditExpenseAmountBox.Text = string.Empty;
            EditExpenseCurrencyBox.SelectedItem = null;
            EditExpenseCategoryBox.SelectedItem = null;
            EditExpenseDateBox.SelectedDate = null;
            EditExpenseDescriptionBox.Text = string.Empty;
            _editingExpenseId = 0;
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