using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WPF.Commands;
using TaskForge.WPF.Common;

namespace TaskForge.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for the Password Manager window
    /// </summary>
    public class PasswordManagerViewModel : ViewModelBase
    {
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult? _currentLoginResult;

        // Data binding properties
        private ObservableCollection<PasswordDisplayItem> _passwords;
        private string _url;
        private string _login;
        private string _passwordText;
        private string _note;
        private PasswordCategory _selectedCategory;
        private ObservableCollection<PasswordCategory> _categories;

        // UI state properties
        private bool _isAddModalVisible;
        private bool _isEditModalVisible;
        private PasswordDisplayItem _selectedItem;

        public PasswordManagerViewModel(
            IPasswordService passwordService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult? loginResult)
        {
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _currentLoginResult = loginResult;

            // Initialize collections
            _passwords = new ObservableCollection<PasswordDisplayItem>();
            _categories = new ObservableCollection<PasswordCategory>(Enum.GetValues<PasswordCategory>());

            // Initialize commands
            LoadPasswordsCommand = new AsyncRelayCommand(LoadPasswordsAsync);
            OpenAddCommand = new RelayCommand(OpenAddModal);
            CancelCommand = new RelayCommand(CancelAdd);
            SaveAddCommand = new AsyncRelayCommand(SaveNewPasswordAsync);
            OpenEditCommand = new RelayCommand(OpenEditModal);
            SaveEditCommand = new AsyncRelayCommand(SaveEditedPasswordAsync);
            CancelEditCommand = new RelayCommand(CancelEdit);
            DeleteCommand = new AsyncRelayCommand(DeletePasswordAsync);
            CopyPasswordCommand = new RelayCommand(CopyPassword);
            ToggleVisibilityCommand = new RelayCommand(ToggleVisibility);
        }

        #region Properties

        public ObservableCollection<PasswordDisplayItem> Passwords
        {
            get => _passwords;
            set => SetProperty(ref _passwords, value);
        }

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string PasswordText
        {
            get => _passwordText;
            set => SetProperty(ref _passwordText, value);
        }

        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        public PasswordCategory SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ObservableCollection<PasswordCategory> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public bool IsAddModalVisible
        {
            get => _isAddModalVisible;
            set => SetProperty(ref _isAddModalVisible, value);
        }

        public bool IsEditModalVisible
        {
            get => _isEditModalVisible;
            set => SetProperty(ref _isEditModalVisible, value);
        }

        public PasswordDisplayItem SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        #endregion

        #region Commands

        public ICommand LoadPasswordsCommand { get; }

        public ICommand OpenAddCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand SaveAddCommand { get; }

        public ICommand OpenEditCommand { get; }

        public ICommand SaveEditCommand { get; }

        public ICommand CancelEditCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand CopyPasswordCommand { get; }

        public ICommand ToggleVisibilityCommand { get; }

        #endregion

        #region Command Implementations

        public async Task LoadPasswordsAsync()
        {
            if (_currentLoginResult == null || _currentLoginResult.IsError)
            {
                MessageBox.Show("Будь ласка, увійдіть, щоб переглянути паролі.", "Потрібна автентифікація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
            var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);

            if (user == null)
            {
                MessageBox.Show("Користувача не знайдено в системі.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var passwords = await _passwordService.GetPasswordsByUserIdAsync(user.Id);

            Passwords.Clear();
            foreach (var password in passwords)
            {
                Passwords.Add(new PasswordDisplayItem
                {
                    Password = new Password
                    {
                        Id = password.Id,
                        Url = password.Url,
                        Login = password.Login,
                        PasswordEncrypted = DecryptPassword(password.PasswordEncrypted),
                        Note = password.Note,
                        Category = password.Category,
                        UserId = password.UserId
                    },
                    IsRevealed = false
                });
            }
        }

        private void OpenAddModal(object parameter)
        {
            // Clear all fields
            Url = string.Empty;
            Login = string.Empty;
            PasswordText = string.Empty;
            Note = string.Empty;
            SelectedCategory = PasswordCategory.Other;

            IsAddModalVisible = true;
        }

        private void CancelAdd(object parameter)
        {
            IsAddModalVisible = false;
        }

        private async Task SaveNewPasswordAsync(object parameter)
        {
            if (_currentLoginResult == null || _currentLoginResult.IsError)
            {
                MessageBox.Show("Будь ласка, увійдіть, щоб зберегти паролі.", "Потрібна автентифікація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var auth0UserId = _auth0Service.GetUserId(_currentLoginResult);
            var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);

            if (user == null)
            {
                MessageBox.Show("Користувача не знайдено в системі.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var password = new Password
            {
                Url = Url,
                Login = Login,
                PasswordEncrypted = PasswordText,
                Note = Note,
                Category = SelectedCategory,
                UserId = user.Id
            };

            await _passwordService.AddPasswordAsync(password);
            IsAddModalVisible = false;
            await LoadPasswordsAsync();
        }

        private void OpenEditModal(object parameter)
        {
            if (parameter is PasswordDisplayItem item && item.Password != null)
            {
                SelectedItem = item;
                Url = item.Password.Url;
                Login = item.Password.Login;
                PasswordText = item.Password.PasswordEncrypted;
                Note = item.Password.Note;
                SelectedCategory = item.Password.Category;

                IsEditModalVisible = true;
            }
        }

        private async Task SaveEditedPasswordAsync(object parameter)
        {
            if (SelectedItem?.Password == null)
            {
                return;
            }

            SelectedItem.Password.Url = Url;
            SelectedItem.Password.Login = Login;
            SelectedItem.Password.PasswordEncrypted = PasswordText;
            SelectedItem.Password.Note = Note;
            SelectedItem.Password.Category = SelectedCategory;

            await _passwordService.UpdatePasswordAsync(SelectedItem.Password);
            IsEditModalVisible = false;
            await LoadPasswordsAsync();
        }

        private void CancelEdit(object parameter)
        {
            IsEditModalVisible = false;
        }

        private async Task DeletePasswordAsync(object parameter)
        {
            if (parameter is int passwordId)
            {
                await _passwordService.DeletePasswordAsync(passwordId);
                await LoadPasswordsAsync();
            }
            else if (parameter is PasswordDisplayItem item && item.Password != null)
            {
                await _passwordService.DeletePasswordAsync(item.Password.Id);
                await LoadPasswordsAsync();
            }
        }

        private void CopyPassword(object parameter)
        {
            if (parameter is string password)
            {
                try
                {
                    Clipboard.SetText(password);
                    MessageBox.Show("Пароль скопійовано в буфер обміну!", "Пароль скопійовано", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося скопіювати пароль: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleVisibility(object parameter)
        {
            if (parameter is PasswordDisplayItem item)
            {
                item.IsRevealed = !item.IsRevealed;
            }
        }

        #endregion

        #region Helper Methods

        private string DecryptPassword(string encryptedText)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                return System.Text.Encoding.UTF8.GetString(encryptedBytes);
            }
            catch (FormatException)
            {
                return encryptedText;
            }
        }

        #endregion
    }
}
