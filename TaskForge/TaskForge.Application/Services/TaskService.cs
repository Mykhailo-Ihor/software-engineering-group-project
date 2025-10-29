using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Interfaces;
using TaskForge.Application.Interfaces;

namespace TaskForge.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }
        public Task<TaskEntity> CreateTaskAsync(string title, string description, DateTime dueDate, int projectId)
            => _taskRepository.CreateTaskAsync(title, description, dueDate, projectId);
        public Task AssignUserToTaskAsync(int taskId, int userId)
            => _taskRepository.AssignUserToTaskAsync(taskId, userId);
        public Task UnassignUserFromTaskAsync(int taskId, int userId)
            => _taskRepository.UnassignUserFromTaskAsync(taskId, userId);
        public Task<bool> DeleteTaskAsync(int taskId)
            => _taskRepository.DeleteTaskAsync(taskId);
        public Task<TaskEntity> UpdateTaskAsync(TaskEntity task)
            => _taskRepository.UpdateTaskAsync(task);
        public Task<TaskEntity> GetTaskByIdAsync(int taskId)
            => _taskRepository.GetTaskByIdAsync(taskId);
    }
}
