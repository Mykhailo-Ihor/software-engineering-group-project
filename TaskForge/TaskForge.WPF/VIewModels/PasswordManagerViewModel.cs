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
    /// <summary>
  /// ViewModel for the Password Manager window
    /// </summary>
    public class PasswordManagerViewModel : ViewModelBase
  {
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
     private readonly LoginResult? _currentLoginResult;

        public Func<string>? GetAddPassword { get; set; }
        public Func<string>? GetEditPassword { get; set; }

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

        // Header properties
    private BitmapImage _userAvatar;
        public BitmapImage UserAvatar
        {
  get => _userAvatar;
            set => SetProperty(ref _userAvatar, value);
        }

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

            // Initialize user avatar
      _userAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));
         LoadUserAvatar();

  // Initialize commands with external command classes
 LoadPasswordsCommand = new LoadPasswordsCommand(
      this, _passwordService, _userService, _auth0Service, _currentLoginResult);

      SaveAddCommand = new AddPasswordCommand(
         this, _passwordService, _userService, _auth0Service, _currentLoginResult);

  DeleteCommand = new DeletePasswordCommand(this, _passwordService);

       // Initialize simple UI commands
   OpenAddCommand = new RelayCommand(OpenAddModal);
 CancelCommand = new RelayCommand(CancelAdd);
    OpenEditCommand = new RelayCommand(OpenEditModal);
   SaveEditCommand = new AsyncRelayCommand(SaveEditedPasswordAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);
    CopyPasswordCommand = new RelayCommand(CopyPassword);
   ToggleVisibilityCommand = new RelayCommand(ToggleVisibility);

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

     #region Command Implementations

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
            if (SelectedItem?.Password == null)
     {
                return;
          }

    try
    {
     SelectedItem.Password.Url = Url;
    SelectedItem.Password.Login = Login;

   var passwordFromUI = GetEditPassword?.Invoke() ?? "";
    SelectedItem.Password.PasswordEncrypted = Convert.ToBase64String(
             System.Text.Encoding.UTF8.GetBytes(passwordFromUI));

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
                MessageBox.Show($"Помилка при збереженні паролю: {ex.Message}", "Помилка",
     MessageBoxButton.OK, MessageBoxImage.Error);
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
      MessageBox.Show($"Помилка при копіюванні паролю: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        /// <summary>
        /// Decrypts the encrypted password text
        /// </summary>
        /// <param name="encryptedText">The Base64 encoded password</param>
        /// <returns>The decrypted password string</returns>
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
