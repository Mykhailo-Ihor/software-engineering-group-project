using System;
using System.Threading.Tasks;
using System.Windows;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.Common;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.PasswordManager
{
    /// <summary>
    /// Command to delete a password entry with confirmation
    /// </summary>
    public class DeletePasswordCommand : AsyncRelayCommand
    {
        private readonly PasswordManagerViewModel _viewModel;
        private readonly IPasswordService _passwordService;

        public DeletePasswordCommand(
     PasswordManagerViewModel viewModel,
     IPasswordService passwordService)
            : base(async parameter => await ExecuteDeletePasswordAsync(viewModel, passwordService, parameter))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        private static async Task ExecuteDeletePasswordAsync(
        PasswordManagerViewModel viewModel,
        IPasswordService passwordService,
        object? parameter)
        {
            // Determine the password ID from the parameter
            int? passwordId = null;

            if (parameter is int id)
            {
                passwordId = id;
            }
            else if (parameter is PasswordDisplayItem item && item.Password != null)
            {
                passwordId = item.Password.Id;
            }

            // If no valid ID was found, exit
            if (passwordId == null)
            {
                return;
            }

            // Show confirmation dialog before deleting
            var result = MessageBox.Show(
           "Ви впевнені, що хочете видалити цей пароль?",
           "Підтвердження видалення",
         MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // If user cancelled, exit
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Delete the password
                await passwordService.DeletePasswordAsync(passwordId.Value);

                // Show success message
                MessageBox.Show(
                      "Пароль успішно видалено.",
                    "Успіх",
            MessageBoxButton.OK,
                     MessageBoxImage.Information);

                // Reload passwords by executing LoadPasswordsCommand
                if (viewModel.LoadPasswordsCommand.CanExecute(null))
                {
                    await ((AsyncRelayCommand)viewModel.LoadPasswordsCommand).ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                // Show error message if deletion fails
                MessageBox.Show(
              $"Помилка при видаленні пароля: {ex.Message}",
          "Помилка",
                      MessageBoxButton.OK,
           MessageBoxImage.Error);
            }
        }
    }
}
