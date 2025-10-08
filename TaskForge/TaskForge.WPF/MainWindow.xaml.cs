using Duende.IdentityModel.OidcClient;
using Serilog;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LoginResult _loginResult;
        public MainWindow()
        {
            InitializeComponent();
            Log.Information("Додаток запущено");
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _loginResult = await App.Auth0Client.LoginAsync();

            if (_loginResult.IsError)
            {
                MessageBox.Show($"Error: {_loginResult.Error}");
                return;
            }

            // Успішний логін - тепер доступні токени
            MessageBox.Show("Login successful!");
            // Оновіть UI, наприклад, сховайте кнопку логіну
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loginResult == null || _loginResult.IsError)
            {
                MessageBox.Show("Not logged in.");
                return;
            }

            await App.Auth0Client.LogoutAsync();

            _loginResult = null;
            UserProfileText.Text = "User Profile";
            MessageBox.Show("Logout successful!");
        }

        private void ShowProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loginResult == null || _loginResult.IsError)
            {
                MessageBox.Show("Please login first.");
                return;
            }

            // Отримати дані користувача з claims
            var user = _loginResult.User;
            var profile = $"Name: {user.FindFirst(c => c.Type == "name")?.Value}\n" +
                          $"Email: {user.FindFirst(c => c.Type == "email")?.Value}\n" +
                          $"Access Token: {_loginResult.AccessToken.Substring(0, 10)}...";  // Не показуйте повний токен

            UserProfileText.Text = profile;
        }
    }
}