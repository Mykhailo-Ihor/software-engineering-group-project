using System;
using System.Threading.Tasks;
using System.Windows;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.ProjectDetails
{
    public class SaveTaskCommand : AsyncRelayCommand
    {
        private readonly ProjectDetailsViewModel _viewModel;
        private readonly ITaskService _taskService;

        public SaveTaskCommand(ProjectDetailsViewModel viewModel, ITaskService taskService)
            : base(async _ => await ExecuteSaveTaskAsync(viewModel, taskService))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        }

        private static async Task ExecuteSaveTaskAsync(ProjectDetailsViewModel viewModel, ITaskService taskService)
        {
            if (viewModel.SelectedProjectId == 0)
            {
                MessageBox.Show("Будь ласка, виберіть проект.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(viewModel.TaskTitle))
            {
                MessageBox.Show("Будь ласка, введіть назву завдання.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newTask = await taskService.CreateTaskAsync(
                    viewModel.TaskTitle,
                    viewModel.TaskDescription,
                    viewModel.TaskDueDate,
                    viewModel.SelectedProjectId
                );

                foreach (var userId in viewModel.SelectedAssigneeIds)
                {
                    await taskService.AssignUserToTaskAsync(newTask.Id, userId);
                }

                MessageBox.Show($"Завдання '{newTask.Title}' успішно створено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                viewModel.IsTaskModalVisible = false;

                if (viewModel.LoadedCommand.CanExecute(null))
                {
                    await ((AsyncRelayCommand)viewModel.LoadedCommand).ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення завдання: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}