using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WPF.Commands;
using SysApp = System.Windows.Application;

namespace TaskForge.WPF.ViewModels
{
    public class FinancialSummaryViewModel : ViewModelBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult _currentLoginResult;
        private int _currentUserId;
        private int _editingExpenseId;

        // Expense List
        private ObservableCollection<ExpenceRecordDto> _expenses;
        public ObservableCollection<ExpenceRecordDto> Expenses
        {
            get => _expenses;
            set => SetProperty(ref _expenses, value);
        }

        private bool _isExpensesListVisible;
        public bool IsExpensesListVisible
        {
            get => _isExpensesListVisible;
            set => SetProperty(ref _isExpensesListVisible, value);
        }

        // Add Expense Modal
        private bool _isAddExpenseModalVisible;
        public bool IsAddExpenseModalVisible
        {
            get => _isAddExpenseModalVisible;
            set => SetProperty(ref _isAddExpenseModalVisible, value);
        }

        private string _addExpenseAmount;
        public string AddExpenseAmount
        {
            get => _addExpenseAmount;
            set => SetProperty(ref _addExpenseAmount, value);
        }

        private Currency _addExpenseCurrency;
        public Currency AddExpenseCurrency
        {
            get => _addExpenseCurrency;
            set => SetProperty(ref _addExpenseCurrency, value);
        }

        private ExpenceCategory _addExpenseCategory;
        public ExpenceCategory AddExpenseCategory
        {
            get => _addExpenseCategory;
            set => SetProperty(ref _addExpenseCategory, value);
        }

        private DateTime _addExpenseDate;
        public DateTime AddExpenseDate
        {
            get => _addExpenseDate;
            set => SetProperty(ref _addExpenseDate, value);
        }

        private string _addExpenseDescription;
        public string AddExpenseDescription
        {
            get => _addExpenseDescription;
            set => SetProperty(ref _addExpenseDescription, value);
        }

        // Edit Expense Modal
        private bool _isEditExpenseModalVisible;
        public bool IsEditExpenseModalVisible
        {
            get => _isEditExpenseModalVisible;
            set => SetProperty(ref _isEditExpenseModalVisible, value);
        }

        private string _editExpenseAmount;
        public string EditExpenseAmount
        {
            get => _editExpenseAmount;
            set => SetProperty(ref _editExpenseAmount, value);
        }

        private Currency _editExpenseCurrency;
        public Currency EditExpenseCurrency
        {
            get => _editExpenseCurrency;
            set => SetProperty(ref _editExpenseCurrency, value);
        }

        private ExpenceCategory _editExpenseCategory;
        public ExpenceCategory EditExpenseCategory
        {
            get => _editExpenseCategory;
            set => SetProperty(ref _editExpenseCategory, value);
        }

        private DateTime _editExpenseDate;
        public DateTime EditExpenseDate
        {
            get => _editExpenseDate;
            set => SetProperty(ref _editExpenseDate, value);
        }

        private string _editExpenseDescription;
        public string EditExpenseDescription
        {
            get => _editExpenseDescription;
            set => SetProperty(ref _editExpenseDescription, value);
        }

        // ComboBox Sources
        public IEnumerable<Currency> Currencies => Enum.GetValues(typeof(Currency)).Cast<Currency>();
        public IEnumerable<ExpenceCategory> Categories => Enum.GetValues(typeof(ExpenceCategory)).Cast<ExpenceCategory>();

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddExpenseCommand { get; }
        public ICommand SaveAddExpenseCommand { get; }
        public ICommand CancelAddExpenseCommand { get; }
        public ICommand EditExpenseCommand { get; }
        public ICommand SaveEditExpenseCommand { get; }
        public ICommand CancelEditExpenseCommand { get; }
        public ICommand DeleteExpenseCommand { get; }
        public ICommand CloseCommand { get; }

        public FinancialSummaryViewModel(
            IExpenseService expenseService,
               IUserService userService,
              Auth0Service auth0Service,
               LoginResult currentLoginResult)
        {
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _currentLoginResult = currentLoginResult ?? throw new ArgumentNullException(nameof(currentLoginResult));

            _expenses = new ObservableCollection<ExpenceRecordDto>();
            _addExpenseAmount = string.Empty;
            _addExpenseDescription = string.Empty;
            _addExpenseDate = DateTime.Today;
            _editExpenseAmount = string.Empty;
            _editExpenseDescription = string.Empty;
            _editExpenseDate = DateTime.Today;

            // Initialize Commands
            LoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
            AddExpenseCommand = new RelayCommand(OnAddExpense);
            SaveAddExpenseCommand = new AsyncRelayCommand(OnSaveAddExpenseAsync);
            CancelAddExpenseCommand = new RelayCommand(OnCancelAddExpense);
            EditExpenseCommand = new AsyncRelayCommand(OnEditExpenseAsync);
            SaveEditExpenseCommand = new AsyncRelayCommand(OnSaveEditExpenseAsync);
            CancelEditExpenseCommand = new RelayCommand(OnCancelEditExpense);
            DeleteExpenseCommand = new AsyncRelayCommand(OnDeleteExpenseAsync);
            CloseCommand = new RelayCommand(OnClose);
        }

        private async Task OnLoadedAsync()
        {
            await LoadUserExpensesAsync();
        }

        private async Task LoadUserExpensesAsync()
        {
            try
            {
                if (_currentLoginResult == null || _currentLoginResult.IsError)
                {
                    MessageBox.Show("Помилка автентифікації.", "Помилка");
                    SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this)?.Close();
                    return;
                }

                var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
                var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user == null)
                {
                    MessageBox.Show("Не вдалося знайти ваші дані в системі.", "Помилка");
                    SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this)?.Close();
                    return;
                }

                _currentUserId = user.Id;

                var userExpenses = await _expenseService.GetUserExpensesAsync(user.Id);
                Expenses = new ObservableCollection<ExpenceRecordDto>(userExpenses);

                IsExpensesListVisible = Expenses.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження витрат: {ex.Message}", "Помилка");
            }
        }

        private void OnAddExpense()
        {
            ClearAddExpenseFields();
            IsAddExpenseModalVisible = true;
        }

        private async Task OnSaveAddExpenseAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddExpenseAmount))
                {
                    MessageBox.Show("Будь ласка, введіть суму витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(AddExpenseAmount, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Будь ласка, введіть коректну суму (число більше 0).", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var description = AddExpenseDescription?.Trim() ?? string.Empty;

                await _expenseService.CreateExpenseAsync(
               amount,
             AddExpenseCurrency,
          AddExpenseCategory,
            AddExpenseDate,
             description,
              _currentUserId);

                MessageBox.Show("Витрату успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                IsAddExpenseModalVisible = false;

                await LoadUserExpensesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні витрати: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelAddExpense()
        {
            IsAddExpenseModalVisible = false;
        }

        private void ClearAddExpenseFields()
        {
            AddExpenseAmount = string.Empty;
            AddExpenseCurrency = Currencies.FirstOrDefault();
            AddExpenseCategory = Categories.FirstOrDefault();
            AddExpenseDate = DateTime.Today;
            AddExpenseDescription = string.Empty;
        }

        private async Task OnEditExpenseAsync(object? parameter)
        {
            if (parameter is not int expenseId)
                return;

            _editingExpenseId = expenseId;

            try
            {
                var expense = await _expenseService.GetExpenseByIdAsync(expenseId);
                if (expense == null)
                {
                    MessageBox.Show("Витрату не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                EditExpenseAmount = expense.Amount.ToString("F2");
                EditExpenseCurrency = expense.Currency;
                EditExpenseCategory = expense.Category;
                EditExpenseDate = expense.Date;
                EditExpenseDescription = expense.Description;

                IsEditExpenseModalVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні витрати: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnSaveEditExpenseAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditExpenseAmount))
                {
                    MessageBox.Show("Будь ласка, введіть суму витрати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(EditExpenseAmount, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Будь ласка, введіть коректну суму (число більше 0).", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var expense = await _expenseService.GetExpenseByIdAsync(_editingExpenseId);
                if (expense == null)
                {
                    MessageBox.Show("Витрату не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    IsEditExpenseModalVisible = false;
                    return;
                }

                expense.Amount = amount;
                expense.Currency = EditExpenseCurrency;
                expense.Category = EditExpenseCategory;
                expense.Date = EditExpenseDate;
                expense.Description = EditExpenseDescription?.Trim() ?? string.Empty;

                await _expenseService.UpdateExpenseAsync(expense);

                MessageBox.Show("Витрату успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditExpenseModalVisible = false;
                ClearEditExpenseFields();

                await LoadUserExpensesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні витрати: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelEditExpense()
        {
            IsEditExpenseModalVisible = false;
            ClearEditExpenseFields();
        }

        private void ClearEditExpenseFields()
        {
            EditExpenseAmount = string.Empty;
            EditExpenseCurrency = Currencies.FirstOrDefault();
            EditExpenseCategory = Categories.FirstOrDefault();
            EditExpenseDate = DateTime.Today;
            EditExpenseDescription = string.Empty;
            _editingExpenseId = 0;
        }

        private async Task OnDeleteExpenseAsync(object? parameter)
        {
            if (parameter is not int expenseId)
                return;

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
                        await LoadUserExpensesAsync();
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

        private void OnClose()
        {
            SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
