using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.ProjectDetails
{
    public class KickUserCommand : AsyncRelayCommand
    {
        private readonly ProjectDetailsViewModel _viewModel;
        private readonly IUserService _userService;
        private readonly int _projectId;

        public KickUserCommand(
            ProjectDetailsViewModel viewModel,
            IUserService userService,
            int projectId)
            : base(async parameter => await ExecuteKickUserAsync(viewModel, userService, projectId, parameter))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _projectId = projectId;
        }

        private static async Task ExecuteKickUserAsync(
            ProjectDetailsViewModel viewModel,
            IUserService userService,
            int projectId,
            object? parameter)
        {
            if (parameter is not int userIdToKick) return;
            var userToKick = viewModel.UsersToKickList.FirstOrDefault(u => u.Id == userIdToKick);
            string userName = userToKick != null ? $"{userToKick.FirstName} {userToKick.LastName}" : "користувача";

            var result = MessageBox.Show(
                $"Ви впевнені, що хочете вигнати {userName} з проекту? Цю дію не можна скасувати.",
                "Підтвердження вигнання",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await userService.RemoveUserFromProjectAsync(userIdToKick, projectId);

                    MessageBox.Show("Користувача успішно вигнано з проекту.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (userToKick != null)
                    {
                        viewModel.UsersToKickList.Remove(userToKick);
                    }

                    if (!viewModel.UsersToKickList.Any())
                    {
                        viewModel.IsKickUserModalVisible = false;
                    }

                    if (viewModel.LoadedCommand.CanExecute(null))
                    {
                        await ((AsyncRelayCommand)viewModel.LoadedCommand).ExecuteAsync(null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при видаленні користувача: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}