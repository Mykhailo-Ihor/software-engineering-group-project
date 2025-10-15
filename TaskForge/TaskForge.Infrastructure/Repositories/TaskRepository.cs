using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Infrastructure.Repositories
{
    public class TaskRepository
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

        // місце під асайн і анасайн тасок
    }
}