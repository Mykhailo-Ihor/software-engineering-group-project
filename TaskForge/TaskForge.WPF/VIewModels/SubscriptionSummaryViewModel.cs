using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging; // Для BitmapImage
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WPF.Commands;
using SysApp = System.Windows.Application;

namespace TaskForge.WPF.ViewModels
{
    public class SubscriptionSummaryViewModel : ViewModelBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult _currentLoginResult;
        private int _currentUserId;
        private int _editingSubscriptionId;

        // --- Властивості для Header ---
        private BitmapImage _userAvatar;
        public BitmapImage UserAvatar
        {
            get => _userAvatar;
            set => SetProperty(ref _userAvatar, value);
        }

        private bool _isUserInfoVisible = true;
        public bool IsUserInfoVisible
        {
            get => _isUserInfoVisible;
            set => SetProperty(ref _isUserInfoVisible, value);
        }
        // ------------------------------

        private ObservableCollection<SubscriptionRecordDto> _subscriptions;
        public ObservableCollection<SubscriptionRecordDto> Subscriptions
        {
            get => _subscriptions;
            set => SetProperty(ref _subscriptions, value);
        }

        private decimal _totalMonthlyCost;
        public decimal TotalMonthlyCost
        {
            get => _totalMonthlyCost;
            set => SetProperty(ref _totalMonthlyCost, value);
        }

        private decimal _totalYearlyCost;
        public decimal TotalYearlyCost
        {
            get => _totalYearlyCost;
            set => SetProperty(ref _totalYearlyCost, value);
        }

        private bool _isSubscriptionsListVisible;
        public bool IsSubscriptionsListVisible
        {
            get => _isSubscriptionsListVisible;
            set => SetProperty(ref _isSubscriptionsListVisible, value);
        }

        // Add Modal Properties
        private bool _isAddSubscriptionModalVisible;
        public bool IsAddSubscriptionModalVisible
        {
            get => _isAddSubscriptionModalVisible;
            set => SetProperty(ref _isAddSubscriptionModalVisible, value);
        }

        private string _addSubscriptionName;
        public string AddSubscriptionName
        {
            get => _addSubscriptionName;
            set => SetProperty(ref _addSubscriptionName, value);
        }

        private string _addSubscriptionAmount;
        public string AddSubscriptionAmount
        {
            get => _addSubscriptionAmount;
            set => SetProperty(ref _addSubscriptionAmount, value);
        }

        private Currency _addSubscriptionCurrency;
        public Currency AddSubscriptionCurrency
        {
            get => _addSubscriptionCurrency;
            set => SetProperty(ref _addSubscriptionCurrency, value);
        }

        private DateTime _addSubscriptionBillingDate;
        public DateTime AddSubscriptionBillingDate
        {
            get => _addSubscriptionBillingDate;
            set => SetProperty(ref _addSubscriptionBillingDate, value);
        }

        private int _addSubscriptionIntervalValue;
        public int AddSubscriptionIntervalValue
        {
            get => _addSubscriptionIntervalValue;
            set => SetProperty(ref _addSubscriptionIntervalValue, value);
        }

        private IntervalUnit _addSubscriptionIntervalUnit;
        public IntervalUnit AddSubscriptionIntervalUnit
        {
            get => _addSubscriptionIntervalUnit;
            set => SetProperty(ref _addSubscriptionIntervalUnit, value);
        }

        private bool _addSubscriptionNotify;
        public bool AddSubscriptionNotify
        {
            get => _addSubscriptionNotify;
            set => SetProperty(ref _addSubscriptionNotify, value);
        }

        // Edit Modal Properties
        private bool _isEditSubscriptionModalVisible;
        public bool IsEditSubscriptionModalVisible
        {
            get => _isEditSubscriptionModalVisible;
            set => SetProperty(ref _isEditSubscriptionModalVisible, value);
        }

        private string _editSubscriptionName;
        public string EditSubscriptionName
        {
            get => _editSubscriptionName;
            set => SetProperty(ref _editSubscriptionName, value);
        }

        private string _editSubscriptionAmount;
        public string EditSubscriptionAmount
        {
            get => _editSubscriptionAmount;
            set => SetProperty(ref _editSubscriptionAmount, value);
        }

        private Currency _editSubscriptionCurrency;
        public Currency EditSubscriptionCurrency
        {
            get => _editSubscriptionCurrency;
            set => SetProperty(ref _editSubscriptionCurrency, value);
        }

        private DateTime _editSubscriptionBillingDate;
        public DateTime EditSubscriptionBillingDate
        {
            get => _editSubscriptionBillingDate;
            set => SetProperty(ref _editSubscriptionBillingDate, value);
        }

        private int _editSubscriptionIntervalValue;
        public int EditSubscriptionIntervalValue
        {
            get => _editSubscriptionIntervalValue;
            set => SetProperty(ref _editSubscriptionIntervalValue, value);
        }

        private IntervalUnit _editSubscriptionIntervalUnit;
        public IntervalUnit EditSubscriptionIntervalUnit
        {
            get => _editSubscriptionIntervalUnit;
            set => SetProperty(ref _editSubscriptionIntervalUnit, value);
        }

        private bool _editSubscriptionNotify;
        public bool EditSubscriptionNotify
        {
            get => _editSubscriptionNotify;
            set => SetProperty(ref _editSubscriptionNotify, value);
        }

        public IEnumerable<Currency> Currencies => Enum.GetValues(typeof(Currency)).Cast<Currency>();
        public IEnumerable<IntervalUnit> IntervalUnits => Enum.GetValues(typeof(IntervalUnit)).Cast<IntervalUnit>();

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand AddSubscriptionCommand { get; }
        public ICommand SaveAddSubscriptionCommand { get; }
        public ICommand CancelAddSubscriptionCommand { get; }
        public ICommand OpenEditSubscriptionCommand { get; }
        public ICommand SaveEditSubscriptionCommand { get; }
        public ICommand CancelEditSubscriptionCommand { get; }
        public ICommand DeleteSubscriptionCommand { get; }

        // --- Header Commands ---
        public ICommand ProfileCommand { get; }
        public ICommand LogoutCommand { get; }

        public SubscriptionSummaryViewModel(
            ISubscriptionService subscriptionService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult currentLoginResult)
        {
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _currentLoginResult = currentLoginResult ?? throw new ArgumentNullException(nameof(currentLoginResult));

            _subscriptions = new ObservableCollection<SubscriptionRecordDto>();

            LoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
            CloseCommand = new RelayCommand(OnClose);

            AddSubscriptionCommand = new RelayCommand(OnAddSubscription);
            SaveAddSubscriptionCommand = new AsyncRelayCommand(OnSaveAddSubscriptionAsync);
            CancelAddSubscriptionCommand = new RelayCommand(OnCancelAddSubscription);

            OpenEditSubscriptionCommand = new AsyncRelayCommand(OnOpenEditSubscriptionAsync);
            SaveEditSubscriptionCommand = new AsyncRelayCommand(OnSaveEditSubscriptionAsync);
            CancelEditSubscriptionCommand = new RelayCommand(OnCancelEditSubscription);

            DeleteSubscriptionCommand = new AsyncRelayCommand(OnDeleteSubscriptionAsync);

            // --- Header Commands Init ---
            ProfileCommand = new AsyncRelayCommand(OnProfileAsync);
            LogoutCommand = new AsyncRelayCommand(OnLogoutAsync);

            // Завантаження аватара
            LoadUserAvatar();
        }

        #region Header Logic

        private void LoadUserAvatar()
        {
            UserAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));

            if (_currentLoginResult != null && !_currentLoginResult.IsError)
            {
                var avatarUrl = _auth0Service.GetUserAvatarUrl(_currentLoginResult);
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    try
                    {
                        UserAvatar = new BitmapImage(new Uri(avatarUrl));
                    }
                    catch { }
                }
            }
        }

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
            var result = MessageBox.Show(
                "Ви впевнені, що хочете вийти з облікового запису?",
                "Вихід",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CloseWindow();
                if (SysApp.Current.MainWindow?.DataContext is MainWindowViewModel mainVM)
                {
                    if (mainVM.LogoutCommand.CanExecute(null))
                    {
                        mainVM.LogoutCommand.Execute(null);
                    }
                }
            }
        }

        #endregion

        #region Currency Conversion & Logic

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

        private void RecalculateTotals()
        {
            if (Subscriptions == null || !Subscriptions.Any())
            {
                TotalMonthlyCost = 0;
                TotalYearlyCost = 0;
                return;
            }

            decimal totalMonthly = 0;
            decimal totalYearly = 0;

            foreach (var sub in Subscriptions)
            {
                decimal amountInUah = ConvertToUah(sub.Amount, sub.Currency);
                int intervalValue = sub.IntervalValue > 0 ? sub.IntervalValue : 1;
                decimal monthlyAmount;
                decimal yearlyAmount;

                switch (sub.IntervalUnit?.ToLower())
                {
                    case "day":
                        monthlyAmount = (amountInUah / intervalValue) * 30;
                        yearlyAmount = (amountInUah / intervalValue) * 365;
                        break;
                    case "week":
                        monthlyAmount = (amountInUah / intervalValue) * 4.33m;
                        yearlyAmount = (amountInUah / intervalValue) * 52;
                        break;
                    case "month":
                        monthlyAmount = amountInUah / intervalValue;
                        yearlyAmount = (amountInUah / intervalValue) * 12;
                        break;
                    case "year":
                        yearlyAmount = amountInUah / intervalValue;
                        monthlyAmount = yearlyAmount / 12;
                        break;
                    default:
                        monthlyAmount = amountInUah;
                        yearlyAmount = amountInUah * 12;
                        break;
                }

                totalMonthly += monthlyAmount;
                totalYearly += yearlyAmount;
            }

            TotalMonthlyCost = Math.Round(totalMonthly, 2);
            TotalYearlyCost = Math.Round(totalYearly, 2);
        }

        private async Task OnLoadedAsync()
        {
            await LoadUserSubscriptionsAsync();
        }

        private async Task LoadUserSubscriptionsAsync()
        {
            try
            {
                if (_currentLoginResult == null || _currentLoginResult.IsError)
                {
                    MessageBox.Show("Помилка автентифікації.", "Помилка");
                    CloseWindow();
                    return;
                }

                var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
                var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
                if (user == null)
                {
                    MessageBox.Show("Не вдалося знайти ваші дані в системі.", "Помилка");
                    CloseWindow();
                    return;
                }

                _currentUserId = user.Id;

                var userSubscriptions = await _subscriptionService.GetUserSubscriptionsAsync(user.Id);
                Subscriptions = new ObservableCollection<SubscriptionRecordDto>(userSubscriptions);

                IsSubscriptionsListVisible = Subscriptions.Any();
                RecalculateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження підписок: {ex.Message}", "Помилка");
            }
        }

        private void CloseWindow()
        {
            SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        private void OnClose()
        {
            CloseWindow();
        }

        #endregion

        #region Add/Edit Logic

        private void OnAddSubscription()
        {
            ClearAddSubscriptionFields();
            IsAddSubscriptionModalVisible = true;
        }

        private void ClearAddSubscriptionFields()
        {
            AddSubscriptionName = string.Empty;
            AddSubscriptionAmount = string.Empty;
            AddSubscriptionCurrency = Currencies.FirstOrDefault();
            AddSubscriptionBillingDate = DateTime.Today.AddDays(1);
            AddSubscriptionIntervalValue = 1;
            AddSubscriptionIntervalUnit = IntervalUnits.FirstOrDefault(u => u == IntervalUnit.Month);
            AddSubscriptionNotify = true;
        }

        private void OnCancelAddSubscription()
        {
            IsAddSubscriptionModalVisible = false;
        }

        private async Task OnSaveAddSubscriptionAsync()
        {
            var name = AddSubscriptionName?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Будь ласка, введіть назву підписки.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(AddSubscriptionAmount, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну суму (число більше 0).", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AddSubscriptionIntervalValue <= 0)
            {
                MessageBox.Show("Інтервал повторення має бути більшим за 0.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _subscriptionService.CreateSubscriptionAsync(
                    name,
                    amount,
                    AddSubscriptionCurrency,
                    AddSubscriptionBillingDate,
                    AddSubscriptionNotify,
                    AddSubscriptionIntervalValue,
                    AddSubscriptionIntervalUnit,
                    _currentUserId
                );

                MessageBox.Show("Підписку успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                IsAddSubscriptionModalVisible = false;

                await LoadUserSubscriptionsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні підписки: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnOpenEditSubscriptionAsync(object? parameter)
        {
            if (parameter is not int subscriptionId) return;

            _editingSubscriptionId = subscriptionId;

            try
            {
                var subscription = await _subscriptionService.GetSubscriptionByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    MessageBox.Show("Підписку не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                EditSubscriptionName = subscription.Name;
                EditSubscriptionAmount = subscription.Amount.ToString("F2");
                EditSubscriptionCurrency = subscription.Currency;
                EditSubscriptionBillingDate = subscription.BillingDate;
                EditSubscriptionIntervalValue = subscription.IntervalValue;
                EditSubscriptionIntervalUnit = subscription.IntervalUnit;
                EditSubscriptionNotify = subscription.Notify;

                IsEditSubscriptionModalVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні підписки: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelEditSubscription()
        {
            IsEditSubscriptionModalVisible = false;
            ClearEditSubscriptionFields();
        }

        private void ClearEditSubscriptionFields()
        {
            _editingSubscriptionId = 0;
            EditSubscriptionName = string.Empty;
            EditSubscriptionAmount = string.Empty;
            EditSubscriptionCurrency = Currencies.FirstOrDefault();
            EditSubscriptionBillingDate = DateTime.Today;
            EditSubscriptionIntervalValue = 1;
            EditSubscriptionIntervalUnit = IntervalUnits.FirstOrDefault();
            EditSubscriptionNotify = false;
        }

        private async Task OnSaveEditSubscriptionAsync()
        {
            var name = EditSubscriptionName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Будь ласка, введіть назву підписки.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(EditSubscriptionAmount, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну суму (число більше 0).", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditSubscriptionIntervalValue <= 0)
            {
                MessageBox.Show("Інтервал повторення має бути більшим за 0.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var subscription = await _subscriptionService.GetSubscriptionByIdAsync(_editingSubscriptionId);
                if (subscription == null)
                {
                    MessageBox.Show("Підписку не знайдено. Можливо, її було видалено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    IsEditSubscriptionModalVisible = false;
                    return;
                }
                subscription.Name = name;
                subscription.Amount = amount;
                subscription.Currency = EditSubscriptionCurrency;
                subscription.BillingDate = EditSubscriptionBillingDate;
                subscription.Notify = EditSubscriptionNotify;
                subscription.IntervalValue = EditSubscriptionIntervalValue;
                subscription.IntervalUnit = EditSubscriptionIntervalUnit;

                await _subscriptionService.UpdateSubscriptionAsync(subscription);

                MessageBox.Show("Підписку успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditSubscriptionModalVisible = false;
                ClearEditSubscriptionFields();

                await LoadUserSubscriptionsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні підписки: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnDeleteSubscriptionAsync(object? parameter)
        {
            if (parameter is not int subscriptionId) return;

            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити цю підписку?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _subscriptionService.DeleteSubscriptionAsync(subscriptionId);
                    if (success)
                    {
                        MessageBox.Show("Підписку успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUserSubscriptionsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Не вдалося знайти підписку для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка під час видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}