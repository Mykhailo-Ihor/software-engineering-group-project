using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Duende.IdentityModel.OidcClient;
using LiveCharts;
using LiveCharts.Wpf;
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
        private bool _isAddIncome;
        private bool _isAddCategoryVisible;
   private bool _isEditIncome;
        private bool _isEditCategoryVisible;
   private TransactionType _editTransactionType;

        private decimal _currentBalance;
        private decimal _spentThisMonth;
        private decimal _earnedThisMonth;
 private SeriesCollection _pieChartSeries;

      // Header properties
        private BitmapImage _userAvatar;
      public BitmapImage UserAvatar
        {
            get => _userAvatar;
    set => SetProperty(ref _userAvatar, value);
        }

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

         IsAddIncome = false;

  _expenses = new ObservableCollection<ExpenceRecordDto>();
     _pieChartSeries = new SeriesCollection();
   ClearAddExpenseFields();
 ClearEditExpenseFields();

       // Initialize user avatar
   _userAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));
        LoadUserAvatar();

        LoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
 SaveAddExpenseCommand = new AddExpenseCommand(this, _expenseService);
       DeleteExpenseCommand = new DeleteExpenseCommand(this, _expenseService);

            AddExpenseCommand = new RelayCommand(OnAddExpense);
 CancelAddExpenseCommand = new RelayCommand(OnCancelAddExpense);
   EditExpenseCommand = new AsyncRelayCommand(OnEditExpenseAsync);
  SaveEditExpenseCommand = new AsyncRelayCommand(OnSaveEditExpenseAsync);
            CancelEditExpenseCommand = new RelayCommand(OnCancelEditExpense);
        CloseCommand = new RelayCommand(OnClose);

     // Header commands
    ProfileCommand = new AsyncRelayCommand(OnProfileAsync);
  LogoutCommand = new AsyncRelayCommand(OnLogoutAsync);
}

     private void LoadUserAvatar()
        {
   if (_currentLoginResult != null && !_currentLoginResult.IsError)
        {
   var avatarUrl = _auth0Service.GetUserAvatarUrl(_currentLoginResult);
    if (!string.IsNullOrEmpty(avatarUrl))
{
      UserAvatar = new BitmapImage(new Uri(avatarUrl));
  }
   }
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

        public bool IsAddIncome
        {
            get => _isAddIncome;
  set
     {
     if (SetProperty(ref _isAddIncome, value))
       {
     IsAddCategoryVisible = !value;
      OnPropertyChanged(nameof(IsAddExpense));
      }
 }
        }

     public bool IsAddExpense
        {
     get => !_isAddIncome;
         set => IsAddIncome = !value;
        }

     public bool IsAddCategoryVisible
        {
        get => _isAddCategoryVisible;
          set => SetProperty(ref _isAddCategoryVisible, value);
    }

        public bool IsEditIncome
    {
      get => _isEditIncome;
     set
       {
     if (SetProperty(ref _isEditIncome, value))
            {
     IsEditCategoryVisible = !value;
       OnPropertyChanged(nameof(IsEditExpense));
  }
     }
        }

        public bool IsEditExpense
   {
get => !_isEditIncome;
   set => IsEditIncome = !value;
    }

        public bool IsEditCategoryVisible
        {
            get => _isEditCategoryVisible;
   set => SetProperty(ref _isEditCategoryVisible, value);
    }

 public decimal CurrentBalance
     {
      get => _currentBalance;
  set => SetProperty(ref _currentBalance, value);
        }

        public decimal SpentThisMonth
  {
    get => _spentThisMonth;
            set => SetProperty(ref _spentThisMonth, value);
        }

 public decimal EarnedThisMonth
      {
     get => _earnedThisMonth;
         set => SetProperty(ref _earnedThisMonth, value);
        }

   public SeriesCollection PieChartSeries
 {
      get => _pieChartSeries;
      set => SetProperty(ref _pieChartSeries, value);
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
        public ICommand ProfileCommand { get; }
  public ICommand LogoutCommand { get; }

        #endregion

        #region Header Command Implementations

        private async Task OnProfileAsync()
    {
        var profileWindow = new ProfileWindow(_currentLoginResult, _auth0Service, _userService);
   var currentWindow = SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
   if (currentWindow != null)
{
  profileWindow.Owner = currentWindow;
 }
profileWindow.ShowDialog();
  LoadUserAvatar();
   }

     private async Task OnLogoutAsync()
        {
      try
 {
     await _auth0Service.LogoutAsync();
     CloseWindow();
       }
            catch (Exception ex)
   {
MessageBox.Show($"Помилка при виході: {ex.Message}", "Помилка виходу",
  MessageBoxButton.OK, MessageBoxImage.Error);
     }
        }

     private void CloseWindow()
{
    SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this)?.Close();
     }

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
 IsAddIncome = false;
   IsAddCategoryVisible = true;
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
    IsEditIncome = expense.Type == TransactionType.Income;

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
   expense.Type = IsEditIncome ? TransactionType.Income : TransactionType.Expense;

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

     private async Task OnLoadedAsync(object? parameter)
        {
  if (_currentLoginResult == null || _currentLoginResult.IsError)
            {
            return;
      }

 var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
   var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);

     if (user == null) return;

    CurrentUserId = user.Id;
  var userExpenses = await _expenseService.GetUserExpensesAsync(user.Id);

   Expenses.Clear();
   foreach (var expense in userExpenses)
            {
 Expenses.Add(expense);
  }

    IsExpensesListVisible = Expenses.Count > 0;

RecalculateStats();
   }

        #endregion

     #region HelperFunctions

        private decimal ConvertToUah(decimal amount, string currencyStr)
        {
     if (!Enum.TryParse(currencyStr, out Currency currency))
  {
        return amount;
        }

   return currency switch
      {
     Currency.UAH => amount,
        Currency.USD => amount * 42.3342m,
         Currency.EUR => amount * 49.1839m,
     Currency.GBP => amount * 55.9150m,
         Currency.JPY => amount * 2.7136m,
    Currency.CAD => amount * 30.2301m,
  Currency.AUD => amount * 27.7458m,
      _ => amount
  };
   }
        public void RecalculateStats()
        {
     var now = DateTime.Now;
            var totalIncomeUah = Expenses
   .Where(e => e.Type == TransactionType.Income.ToString())
  .Sum(e => ConvertToUah(e.Amount, e.Currency));

        var totalExpenseUah = Expenses
      .Where(e => e.Type == TransactionType.Expense.ToString())
         .Sum(e => ConvertToUah(e.Amount, e.Currency));

       CurrentBalance = totalIncomeUah - totalExpenseUah;
 SpentThisMonth = Expenses
        .Where(e => e.Type == TransactionType.Expense.ToString() && e.Date.Month == now.Month && e.Date.Year == now.Year)
         .Sum(e => ConvertToUah(e.Amount, e.Currency));

    EarnedThisMonth = Expenses
 .Where(e => e.Type == TransactionType.Income.ToString() && e.Date.Month == now.Month && e.Date.Year == now.Year)
       .Sum(e => ConvertToUah(e.Amount, e.Currency));

   UpdatePieChart();
        }

        private void UpdatePieChart()
        {
   var expenseCategories = Expenses
    .Where(e => e.Type == TransactionType.Expense.ToString())
         .GroupBy(e => e.Category)
                .Select(g => new
    {
       Category = g.Key,
           AmountUah = g.Sum(e => ConvertToUah(e.Amount, e.Currency))
   })
  .ToList();

            var newSeries = new SeriesCollection();

   var colors = new List<System.Windows.Media.Brush>
  {
     (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#7E57C2"),
     (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#42A5F5"),
         (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#26C6DA"),
   (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#AB47BC"),
       (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#5C6BC0"),
   (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#29B6F6"),
     (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#EC407A"),
       (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#78909C")
    };

        int colorIndex = 0;

    foreach (var item in expenseCategories)
       {
         var color = colors[colorIndex % colors.Count];

       newSeries.Add(new PieSeries
  {
           Title = item.Category,
          Values = new ChartValues<decimal> { item.AmountUah },
 DataLabels = true,
             LabelPoint = chartPoint => "",
       Fill = color,
  Stroke = System.Windows.Media.Brushes.Transparent,
 StrokeThickness = 0
             });

      colorIndex++;
            }

 PieChartSeries = newSeries;
        }

        #endregion
    }
}
