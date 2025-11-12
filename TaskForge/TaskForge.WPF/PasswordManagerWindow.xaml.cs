using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Duende.IdentityModel.OidcClient;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
namespace TaskForge.WPF
{
    public partial class PasswordManagerWindow : Window
    {
        private readonly IPasswordService passwordService;
        private readonly Auth0Service auth0Service;
        private readonly IUserService userService;
        private readonly LoginResult? currentLoginResult;
        private List<Password> passwords = new List<Password>();

        public PasswordManagerWindow(IPasswordService passwordService, Auth0Service auth0Service, IUserService userService, LoginResult? loginResult)
        {
            this.InitializeComponent();
            this.passwordService = passwordService;
            this.auth0Service = auth0Service;
            this.userService = userService;
            this.currentLoginResult = loginResult;
            this.LoadPasswords();

            this.AddCategoryComboBox.ItemsSource = Enum.GetValues(typeof(PasswordCategory));
            this.EditCategoryComboBox.ItemsSource = Enum.GetValues(typeof(PasswordCategory));

            this.AddCategoryComboBox.SelectedIndex = 0;
            this.EditCategoryComboBox.SelectedIndex = 0;
        }

        private string DecryptPassword(string encryptedText)
        {
            try
            {
                var encryptedBytes = System.Convert.FromBase64String(encryptedText);
                return System.Text.Encoding.UTF8.GetString(encryptedBytes);
            }
            catch (FormatException)
            {
                return encryptedText;
            }
        }

        private async void LoadPasswords()
        {
            if (this.currentLoginResult == null || this.currentLoginResult.IsError)
            {
                MessageBox.Show("Будь ласка, увійдіть, щоб переглянути паролі.", "Потрібна автентифікація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var auth0UserId = this.auth0Service.GetUserId(this.currentLoginResult);
            var user = await this.userService.GetUserByAuth0IdAsync(auth0UserId);

            if (user == null)
            {
                MessageBox.Show("Користувача не знайдено в системі.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.passwords = await this.passwordService.GetPasswordsByUserIdAsync(user.Id);

            var decryptedPasswords = new List<Password>();
            foreach (var password in this.passwords)
            {
                decryptedPasswords.Add(new Password
                {
                    Id = password.Id,
                    Url = password.Url,
                    Login = password.Login,
                    PasswordEncrypted = this.DecryptPassword(password.PasswordEncrypted),
                    Note = password.Note,
                    Category = password.Category,
                    UserId = password.UserId
                });
            }

            this.PasswordListView.ItemsSource = decryptedPasswords;
        }

        private void AddPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            this.AddUrlTextBox.Text = string.Empty;
            this.AddLoginTextBox.Text = string.Empty;
            this.AddPasswordEncryptedBox.Password = string.Empty;
            this.AddNoteTextBox.Text = string.Empty;
            this.AddCategoryComboBox.SelectedIndex = -1;

            this.AddPasswordModalOverlay.Visibility = Visibility.Visible;
        }

        private void EditPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.PasswordListView.SelectedItem is Password selectedPassword)
            {
                this.EditUrlTextBox.Text = selectedPassword.Url;
                this.EditLoginTextBox.Text = selectedPassword.Login;
                this.EditPasswordEncryptedBox.Password = selectedPassword.PasswordEncrypted;
                this.EditNoteTextBox.Text = selectedPassword.Note;
                this.EditCategoryComboBox.SelectedValue = selectedPassword.Category;

                this.EditPasswordModalOverlay.Visibility = Visibility.Visible;
            }
        }

        private async void DeletePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.PasswordListView.SelectedItem is Password selectedPassword)
            {
                await this.passwordService.DeletePasswordAsync(selectedPassword.Id);
                this.LoadPasswords();
            }
        }

        private async void AddPasswordModalSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentLoginResult == null || this.currentLoginResult.IsError)
            {
                MessageBox.Show("Будь ласка, увійдіть, щоб зберегти паролі.", "Потрібна автентифікація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var auth0UserId = this.auth0Service.GetUserId(this.currentLoginResult);
            var user = await this.userService.GetUserByAuth0IdAsync(auth0UserId);

            if (user == null)
            {
                MessageBox.Show("Користувача не знайдено в системі.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var password = new Password
            {
                Url = this.AddUrlTextBox.Text,
                Login = this.AddLoginTextBox.Text,
                PasswordEncrypted = this.AddPasswordEncryptedBox.Password,
                Note = this.AddNoteTextBox.Text,
                Category = this.AddCategoryComboBox.SelectedValue is PasswordCategory addCategory ? addCategory : PasswordCategory.Other,
                UserId = user.Id,
            };

            await this.passwordService.AddPasswordAsync(password);
            this.AddPasswordModalOverlay.Visibility = Visibility.Collapsed;
            this.LoadPasswords();
        }

        private void AddPasswordModalCancel_Click(object sender, RoutedEventArgs e)
        {
            this.AddPasswordModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void EditPasswordModalCancel_Click(object sender, RoutedEventArgs e)
        {
            this.EditPasswordModalOverlay.Visibility = Visibility.Collapsed;
        }

        private async void EditPasswordModalSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.PasswordListView.SelectedItem is Password selectedPassword)
            {
                selectedPassword.Url = this.EditUrlTextBox.Text;
                selectedPassword.Login = this.EditLoginTextBox.Text;
                selectedPassword.PasswordEncrypted = this.EditPasswordEncryptedBox.Password;
                selectedPassword.Note = this.EditNoteTextBox.Text;
                selectedPassword.Category = this.EditCategoryComboBox.SelectedValue as PasswordCategory? ?? PasswordCategory.Other;

                await this.passwordService.UpdatePasswordAsync(selectedPassword);
                this.EditPasswordModalOverlay.Visibility = Visibility.Collapsed;
                this.LoadPasswords();
            }
        }

        private void CopyPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string password)
            {
                try
                {
                    System.Windows.Clipboard.SetText(password);
                    MessageBox.Show("Пароль скопійовано в буфер обміну!", "Пароль скопійовано", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Не вдалося скопіювати пароль: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}