using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.Commands;
using SysApp = System.Windows.Application;

namespace TaskForge.WPF.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly Auth0Service _auth0Service;
        private readonly Duende.IdentityModel.OidcClient.LoginResult _loginResult;
        private UserDto? _currentUser;

        public UserDto? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private BitmapImage _profileAvatar;
        public BitmapImage ProfileAvatar
        {
            get => _profileAvatar;
            set => SetProperty(ref _profileAvatar, value);
        }

        private string _profileInfoText = string.Empty;
        public string ProfileInfoText
        {
            get => _profileInfoText;
            set => SetProperty(ref _profileInfoText, value);
        }

        private Visibility _profileInfoVisibility = Visibility.Collapsed;
        public Visibility ProfileInfoVisibility
        {
            get => _profileInfoVisibility;
            set => SetProperty(ref _profileInfoVisibility, value);
        }

        private bool _isFieldsEnabled = true;
        public bool IsFieldsEnabled
        {
            get => _isFieldsEnabled;
            set => SetProperty(ref _isFieldsEnabled, value);
        }

        private Visibility _saveButtonVisibility = Visibility.Visible;
        public Visibility SaveButtonVisibility
        {
            get => _saveButtonVisibility;
            set => SetProperty(ref _saveButtonVisibility, value);
        }

        public ICommand LoadedCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand LogoutCommand { get; }

        public ProfileViewModel(Duende.IdentityModel.OidcClient.LoginResult loginResult, Auth0Service auth0Service, IUserService userService)
        {
            _loginResult = loginResult ?? throw new ArgumentNullException(nameof(loginResult));
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            _profileAvatar = new BitmapImage(new Uri("pack://application:,,,/TaskForge.WPF;component/Resources/avatar_placeholder.png"));

            LoadedCommand = new Commands.Profile.LoadProfileCommand(this, _userService, _auth0Service, _loginResult);
            SaveCommand = new Commands.Profile.SaveProfileCommand(this, _userService, _auth0Service, _loginResult);
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
            LogoutCommand = new AsyncRelayCommand(OnLogoutAsync);
        }

        public void RequestClose()
        {
            CloseRequested?.Invoke();
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
                RequestClose();

                if (SysApp.Current.MainWindow?.DataContext is MainWindowViewModel mainVM)
                {
                    if (mainVM.LogoutCommand.CanExecute(null))
                    {
                        mainVM.LogoutCommand.Execute(null);
                    }
                }
            }
        }
        public event Action? CloseRequested;
    }
}