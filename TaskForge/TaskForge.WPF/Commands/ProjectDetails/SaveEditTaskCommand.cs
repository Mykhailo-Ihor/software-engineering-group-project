using System;
using System.Threading.Tasks;
using System.Windows;
using TaskForge.Application.Interfaces;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF.Commands.ProjectDetails
{
    public class SaveEditTaskCommand : AsyncRelayCommand
    {
        private readonly ProjectDetailsViewModel _viewModel;
        private readonly ITaskService _taskService;

        public SaveEditTaskCommand(ProjectDetailsViewModel viewModel, ITaskService taskService)
            : base(async _ => await ExecuteSaveEditTaskAsync(viewModel, taskService))
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        }

        private static async Task ExecuteSaveEditTaskAsync(ProjectDetailsViewModel viewModel, ITaskService taskService)
        {
            if (string.IsNullOrWhiteSpace(viewModel.EditTaskTitle))
            {
                MessageBox.Show("Введіть назву завдання.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var task = await taskService.GetTaskByIdAsync(viewModel.EditingTaskId);
                if (task == null)
                {
                    MessageBox.Show("Завдання не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    viewModel.IsEditTaskModalVisible = false;
                    return;
                }

                task.Title = viewModel.EditTaskTitle.Trim();
                task.Description = viewModel.EditTaskDescription?.Trim() ?? string.Empty;
                task.DueDate = viewModel.EditTaskDueDate;

                await taskService.UpdateTaskAsync(task);

                viewModel.IsEditTaskModalVisible = false;
                viewModel.EditTaskTitle = string.Empty;
                viewModel.EditTaskDescription = string.Empty;
                viewModel.EditTaskDueDate = DateTime.Now.AddDays(1);
                viewModel.EditingTaskId = 0;

                if (viewModel.LoadedCommand.CanExecute(null))
                {
                    await ((AsyncRelayCommand)viewModel.LoadedCommand).ExecuteAsync(null);
                }

                MessageBox.Show("Завдання успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні завдання: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}