using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Interfaces;

namespace TaskForge.Tests.Application.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _taskService = new TaskService(_mockTaskRepository.Object);
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldUpdateAndReturnTask_WhenTaskExists()
        {
            // Arrange
            var existingTask = new TaskEntity
            {
                Id = 1,
                Title = "Original Title",
                Description = "Original Description",
                DueDate = DateTime.UtcNow.AddDays(5)
            };

            // Модифікуємо таск
            existingTask.Title = "Updated Title";
            existingTask.Description = "Updated Description";

            _mockTaskRepository
                .Setup(repo => repo.UpdateTaskAsync(existingTask))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _taskService.UpdateTaskAsync(existingTask);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal("Updated Description", result.Description);
            _mockTaskRepository.Verify(repo => repo.UpdateTaskAsync(existingTask), Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnTask_WhenTaskExists()
        {
            // Arrange
            var taskId = 1;
            var expectedTask = new TaskEntity { Id = taskId, Title = "Test Task" };

            _mockTaskRepository
                .Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(expectedTask);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
            _mockTaskRepository.Verify(repo => repo.GetTaskByIdAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = 99; // Неіснуючий ID
            _mockTaskRepository
                .Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync((TaskEntity)null);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.Null(result);
            _mockTaskRepository.Verify(repo => repo.GetTaskByIdAsync(taskId), Times.Once);
        }
    }
}