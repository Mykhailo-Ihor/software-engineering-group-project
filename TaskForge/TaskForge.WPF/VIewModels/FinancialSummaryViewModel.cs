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
using TaskForge.WPF.Commands.FinancialSummary; 
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
        private ObservableCollection<ExpenceRecordDto> _expenses;
        private bool _isExpensesListVisible;
        private bool _isAddExpenseModalVisible;
        private string _addExpenseAmount;
        private Currency _addExpenseCurrency;
        private ExpenceCategory _addExpenseCategory;
        private DateTime _addExpenseDate;
        private string _addExpenseDescription;
        private bool _isEditExpenseModalVisible;
        private string _editExpenseAmount;
        private Currency _editExpenseCurrency;
        private ExpenceCategory _editExpenseCategory;
        private DateTime _editExpenseDate;
        private string _editExpenseDescription;

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
            ClearAddExpenseFields();
            ClearEditExpenseFields();

            LoadedCommand = new LoadExpensesCommand(this, _expenseService, _userService, _auth0Service, _currentLoginResult);
            SaveAddExpenseCommand = new AddExpenseCommand(this, _expenseService);
            DeleteExpenseCommand = new DeleteExpenseCommand(this, _expenseService);

            AddExpenseCommand = new RelayCommand(OnAddExpense);
            CancelAddExpenseCommand = new RelayCommand(OnCancelAddExpense);
            EditExpenseCommand = new AsyncRelayCommand(OnEditExpenseAsync);
            SaveEditExpenseCommand = new AsyncRelayCommand(OnSaveEditExpenseAsync);
            CancelEditExpenseCommand = new RelayCommand(OnCancelEditExpense);
            CloseCommand = new RelayCommand(OnClose);
        }

        #region Properties

        public int CurrentUserId
        {
            get => _currentUserId;
            set => SetProperty(ref _currentUserId, value);
        }

        public ObservableCollection<ExpenceRecordDto> Expenses
        {
            get => _expenses;
            set => SetProperty(ref _expenses, value);
        }

        public bool IsExpensesListVisible
        {
            get => _isExpensesListVisible;
            set => SetProperty(ref _isExpensesListVisible, value);
        }

        public bool IsAddExpenseModalVisible
        {
            get => _isAddExpenseModalVisible;
            set => SetProperty(ref _isAddExpenseModalVisible, value);
        }

        public string AddExpenseAmount
        {
            get => _addExpenseAmount;
            set => SetProperty(ref _addExpenseAmount, value);
        }

        public Currency AddExpenseCurrency
        {
            get => _addExpenseCurrency;
            set => SetProperty(ref _addExpenseCurrency, value);
        }

        public ExpenceCategory AddExpenseCategory
        {
            get => _addExpenseCategory;
            set => SetProperty(ref _addExpenseCategory, value);
        }

        public DateTime AddExpenseDate
        {
            get => _addExpenseDate;
            set => SetProperty(ref _addExpenseDate, value);
        }

        public string AddExpenseDescription
        {
            get => _addExpenseDescription;
            set => SetProperty(ref _addExpenseDescription, value);
        }

        public bool IsEditExpenseModalVisible
        {
            get => _isEditExpenseModalVisible;
            set => SetProperty(ref _isEditExpenseModalVisible, value);
        }

        public string EditExpenseAmount
        {
            get => _editExpenseAmount;
            set => SetProperty(ref _editExpenseAmount, value);
        }

        public Currency EditExpenseCurrency
        {
            get => _editExpenseCurrency;
            set => SetProperty(ref _editExpenseCurrency, value);
        }

        public ExpenceCategory EditExpenseCategory
        {
            get => _editExpenseCategory;
            set => SetProperty(ref _editExpenseCategory, value);
        }

        public DateTime EditExpenseDate
        {
            get => _editExpenseDate;
            set => SetProperty(ref _editExpenseDate, value);
        }

        public string EditExpenseDescription
        {
            get => _editExpenseDescription;
            set => SetProperty(ref _editExpenseDescription, value);
        }
        #endregion

        #region ComboBoxInit

        public IEnumerable<Currency> Currencies => Enum.GetValues(typeof(Currency)).Cast<Currency>();
        public IEnumerable<ExpenceCategory> Categories => Enum.GetValues(typeof(ExpenceCategory)).Cast<ExpenceCategory>();

        #endregion

        #region Commands

        public ICommand LoadedCommand { get; }
        public ICommand AddExpenseCommand { get; } 
        public ICommand SaveAddExpenseCommand { get; } 
        public ICommand CancelAddExpenseCommand { get; }
        public ICommand EditExpenseCommand { get; }
        public ICommand SaveEditExpenseCommand { get; }
        public ICommand CancelEditExpenseCommand { get; }
        public ICommand DeleteExpenseCommand { get; } 
        public ICommand CloseCommand { get; }

        #endregion

        #region CommandsImplementation

        private void OnAddExpense()
        {
            ClearAddExpenseFields();
            IsAddExpenseModalVisible = true;
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

                if (LoadedCommand.CanExecute(null))
                {
                    await ((AsyncRelayCommand)LoadedCommand).ExecuteAsync(null);
                }
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

        private void OnClose()
        {
            SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        #endregion
    }
}
