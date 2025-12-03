using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WPF.Commands;
using TaskForge.WPF.Commands.PasswordManager;
using TaskForge.WPF.Common;
using SysApp = System.Windows.Application; 

namespace TaskForge.WPF.ViewModels
{
    public class PasswordManagerViewModel : ViewModelBase
    {
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly LoginResult? _currentLoginResult;

        public Func<string>? GetAddPassword { get; set; }
        public Func<string>? GetEditPassword { get; set; }

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

        private ObservableCollection<PasswordDisplayItem> _passwords;
        private string _url;
        private string _login;
        private string _passwordText;
        private string _note;
        private PasswordCategory _selectedCategory;
        private ObservableCollection<PasswordCategory> _categories;

        private bool _isAddModalVisible;
        private bool _isEditModalVisible;
        private PasswordDisplayItem _selectedItem;


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

        public ICommand ProfileCommand { get; }
        public ICommand LogoutCommand { get; }

        public PasswordManagerViewModel(
            IPasswordService passwordService,
            IUserService userService,
            Auth0Service auth0Service,
            LoginResult? loginResult,
            bool openAddModalOnLoad = false)
        {
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _currentLoginResult = loginResult;

            _passwords = new ObservableCollection<PasswordDisplayItem>();
            _categories = new ObservableCollection<PasswordCategory>(Enum.GetValues<PasswordCategory>());


            LoadPasswordsCommand = new LoadPasswordsCommand(this, _passwordService, _userService, _auth0Service, _currentLoginResult);
            SaveAddCommand = new AddPasswordCommand(this, _passwordService, _userService, _auth0Service, _currentLoginResult);
            DeleteCommand = new DeletePasswordCommand(this, _passwordService);

            OpenAddCommand = new RelayCommand(OpenAddModal);
            CancelCommand = new RelayCommand(CancelAdd);
            OpenEditCommand = new RelayCommand(OpenEditModal);
            SaveEditCommand = new AsyncRelayCommand(SaveEditedPasswordAsync);
            CancelEditCommand = new RelayCommand(CancelEdit);
            CopyPasswordCommand = new RelayCommand(CopyPassword);
            ToggleVisibilityCommand = new RelayCommand(ToggleVisibility);


            ProfileCommand = new AsyncRelayCommand(OnProfileAsync);
            LogoutCommand = new AsyncRelayCommand(OnLogoutAsync);


            LoadUserAvatar();


            if (openAddModalOnLoad)
            {
                OpenAddModal(null);
            }
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
                    catch
                    {

                    }
                }
            }
        }

        private async Task OnProfileAsync()
        {
            if (_currentLoginResult == null) return;

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
                var currentWindow = SysApp.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
                currentWindow?.Close();

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

        #region Command Implementations

        private void OpenAddModal(object parameter)
        {
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

        private async Task SaveEditedPasswordAsync(object? parameter)
        {
            if (SelectedItem?.Password == null) return;

            try
            {
                SelectedItem.Password.Url = Url;
                SelectedItem.Password.Login = Login;

                var passwordFromUI = GetEditPassword?.Invoke() ?? "";
                if (!string.IsNullOrEmpty(passwordFromUI))
                {
                    SelectedItem.Password.PasswordEncrypted = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordFromUI));
                }

                SelectedItem.Password.Note = Note;
                SelectedItem.Password.Category = SelectedCategory;

                await _passwordService.UpdatePasswordAsync(SelectedItem.Password);
                IsEditModalVisible = false;

                if (LoadPasswordsCommand.CanExecute(null))
                {
                    await ((AsyncRelayCommand)LoadPasswordsCommand).ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні паролю: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEdit(object parameter)
        {
            IsEditModalVisible = false;
        }

        private void CopyPassword(object parameter)
        {
            if (parameter is string password)
            {
                try
                {
                    Clipboard.SetText(password);
                    MessageBox.Show("Пароль скопійовано в буфер обміну!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при копіюванні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public string DecryptPassword(string encryptedText)
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