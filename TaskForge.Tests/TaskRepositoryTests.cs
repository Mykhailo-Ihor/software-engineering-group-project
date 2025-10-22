using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TaskForge.Infrastructure.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using System.Linq;

namespace TaskForge.Tests
{
    public class TaskRepositoryTests
    {
        private TaskForgeDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TaskForgeDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new TaskForgeDbContext(options);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldCreateTask_WhenProjectExists()
        {
            // Arrange
            var dbName = nameof(CreateTaskAsync_ShouldCreateTask_WhenProjectExists);
            using var context = CreateInMemoryDbContext(dbName);
            var project = new Project { Name = "Test Project", Status = "Active", Description = "Desc" };
            context.Projects.Add(project);
            context.SaveChanges();
            var repo = new TaskRepository(context);

            // Act
            var dueDate = DateTime.Now.AddDays(1);
            var task = await repo.CreateTaskAsync("Task 1", "Task Desc", dueDate, project.Id);

            // Assert
            Assert.NotNull(task);
            Assert.Equal("Task 1", task.Title);
            Assert.Equal(project.Id, task.ProjectId);
            Assert.Equal(dueDate, task.DueDate);
            Assert.False(task.IsCompleted);
            Assert.True(context.Tasks.Any(t => t.Id == task.Id));
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldThrow_WhenProjectDoesNotExist()
        {
            // Arrange
            var dbName = nameof(CreateTaskAsync_ShouldThrow_WhenProjectDoesNotExist);
            using var context = CreateInMemoryDbContext(dbName);
            var repo = new TaskRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await repo.CreateTaskAsync("Task 1", "Task Desc", DateTime.Now.AddDays(1), 999);
            });
        }
        [Fact]
        public async Task DeleteTaskAsync_ShouldDeleteTask_WhenTaskExists()
        {
            // Arrange
            var dbName = nameof(DeleteTaskAsync_ShouldDeleteTask_WhenTaskExists);
            using var context = CreateInMemoryDbContext(dbName);

            var project = new Project { Id = 1, Name = "Test Project", Status = "Active", Description = "Desc" };
            var taskToDelete = new TaskEntity
            {
                Id = 1,
                Title = "Task to Delete",
                Description = "This task should be deleted",
                DueDate = DateTime.Now,
                ProjectId = 1
            };

            context.Projects.Add(project);
            context.Tasks.Add(taskToDelete);
            await context.SaveChangesAsync();

            var repo = new TaskRepository(context);

            // Act
            var result = await repo.DeleteTaskAsync(taskToDelete.Id);

            // Assert
            Assert.True(result);
            Assert.False(await context.Tasks.AnyAsync(t => t.Id == taskToDelete.Id));
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            var dbName = nameof(DeleteTaskAsync_ShouldReturnFalse_WhenTaskDoesNotExist);
            using var context = CreateInMemoryDbContext(dbName);
            var repo = new TaskRepository(context);

            // Act
            var result = await repo.DeleteTaskAsync(999); // Неіснуючий ID

            // Assert
            Assert.False(result);
        }
    }
}
