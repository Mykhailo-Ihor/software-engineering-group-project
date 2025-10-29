using System.Threading.Tasks;
using System.Collections.Generic;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskEntity> CreateTaskAsync(string title, string description, DateTime dueDate, int projectId);
        Task AssignUserToTaskAsync(int taskId, int userId);
        Task UnassignUserFromTaskAsync(int taskId, int userId);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<TaskEntity> UpdateTaskAsync(TaskEntity task);
        Task<TaskEntity> GetTaskByIdAsync(int taskId);
    }
}
