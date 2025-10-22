using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using TaskForge.Domain.Interfaces;

namespace TaskForge.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskForgeDbContext _context;

        public TaskRepository(TaskForgeDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Створює нове завдання в проекті. Призначення користувачів виконується окремо.
        /// </summary>
        /// <param name="title">Назва завдання.</param>
        /// <param name="description">Опис завдання.</param>
        /// <param name="dueDate">Термін виконання.</param>
        /// <param name="projectId">ID проекту, до якого належить завдання.</param>
        /// <returns>Створене завдання.</returns>
        public async Task<TaskEntity> CreateTaskAsync(string title, string description, DateTime dueDate, int projectId)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
            {
                throw new InvalidOperationException($"Проект з ID {projectId} не знайдено.");
            }

            var task = new TaskEntity
            {
                Title = title,
                Description = description,
                DueDate = dueDate,
                IsCompleted = false,
                ProjectId = projectId
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return task;
        }

        /// <summary>
        /// Призначає користувача на виконання завдання.
        /// </summary>
        /// <param name="taskId">ID завдання.</param>
        /// <param name="userId">ID користувача.</param>
        public async Task AssignUserToTaskAsync(int taskId, int userId)
        {
            // Перевіряємо, чи існують завдання та користувач
            var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
            if (!taskExists)
            {
                throw new InvalidOperationException($"Завдання з ID {taskId} не знайдено.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new InvalidOperationException($"Користувача з ID {userId} не знайдено.");
            }

            // Перевіряємо, чи користувач вже не призначений на це завдання
            var isAlreadyAssigned = await _context.TaskUsers
                .AnyAsync(tu => tu.TaskId == taskId && tu.UserId == userId);

            if (isAlreadyAssigned)
            {
                // Можна або нічого не робити, або кидати виняток
                return; // Користувач вже призначений
            }

            var taskUser = new TaskUser
            {
                TaskId = taskId,
                UserId = userId
            };

            await _context.TaskUsers.AddAsync(taskUser);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Знімає користувача із завдання.
        /// </summary>
        /// <param name="taskId">ID завдання.</param>
        /// <param name="userId">ID користувача.</param>
        public async Task UnassignUserFromTaskAsync(int taskId, int userId)
        {
            var assignment = await _context.TaskUsers
                .FirstOrDefaultAsync(tu => tu.TaskId == taskId && tu.UserId == userId);

            if (assignment != null)
            {
                _context.TaskUsers.Remove(assignment);
                await _context.SaveChangesAsync();
            }
            // Якщо призначення не знайдено, нічого не робимо.
        }
        /// <summary>
        /// Видаляє завдання за його ID.
        /// </summary>
        /// <param name="taskId">ID завдання, яке потрібно видалити.</param>
        /// <returns>Повертає true, якщо завдання було знайдено та видалено; інакше false.</returns>
        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null)
            {
                // Завдання не знайдено
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}