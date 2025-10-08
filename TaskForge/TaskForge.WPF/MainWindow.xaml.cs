using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
         private readonly Auth0Service _auth0Service;
         private LoginResult _currentLoginResult;

         public MainWindow()
         {
             InitializeComponent();
             _auth0Service = new Auth0Service();
         }

         private async void LoginButton_Click(object sender, RoutedEventArgs e)
         {
             try
             {
                 // Вимкнути кнопку під час логіну
                 LoginButton.IsEnabled = false;
                 StatusText.Text = "Відкриття браузера для входу...";

                 // Виконати логін
                 _currentLoginResult = await _auth0Service.LoginAsync();

                 if (_currentLoginResult.IsError)
                 {
                     StatusText.Text = $"Помилка: {_currentLoginResult.Error}";
                     LoginButton.IsEnabled = true;
                     return;
                 }

                 // Відобразити інформацію про користувача
                 ShowUserInfo(_currentLoginResult);

                 StatusText.Text = "Успішний вхід!";
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Помилка при вході: {ex.Message}", "Помилка",
                     MessageBoxButton.OK, MessageBoxImage.Error);
                 LoginButton.IsEnabled = true;
                 StatusText.Text = "";
             }
         }

         private async void LogoutButton_Click(object sender, RoutedEventArgs e)
         {
             try
             {
                 LogoutButton.IsEnabled = false;
                 StatusText.Text = "Вихід...";

                 await _auth0Service.LogoutAsync();

                 // Очистити інформацію про користувача
                 _currentLoginResult = null;

                 StatusText.Text = "Ви вийшли з системи";
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Помилка при виході: {ex.Message}", "Помилка",
                     MessageBoxButton.OK, MessageBoxImage.Error);
                 LogoutButton.IsEnabled = true;
             }
         }

         private void ShowUserInfo(LoginResult loginResult)
         {
             // Отримати дані користувача
             var userName = _auth0Service.GetUserName(loginResult) ?? "Невідомо";
             var userEmail = _auth0Service.GetUserEmail(loginResult) ?? "Невідомо";
             var userId = _auth0Service.GetUserId(loginResult) ?? "Невідомо";

             // Відобразити дані
             UserNameText.Text = userName;
             UserEmailText.Text = userEmail;
             UserIdText.Text = userId;

             // Перемкнути панелі
             LoginPanel.Visibility = Visibility.Collapsed;
             UserInfoPanel.Visibility = Visibility.Visible;
             LoginButton.Visibility = Visibility.Collapsed;
             LogoutButton.Visibility = Visibility.Visible;

             // Якщо потрібно використовувати Access Token для API запитів:
             // var accessToken = _auth0Service.GetAccessToken(loginResult);
             // Console.WriteLine($"Access Token: {accessToken}");
         }
        }
    }