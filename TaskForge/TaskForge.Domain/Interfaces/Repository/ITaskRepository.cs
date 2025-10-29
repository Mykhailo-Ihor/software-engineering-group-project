using System.Threading.Tasks;
using TaskForge.Domain.Entities;

namespace TaskForge.Domain.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskEntity> CreateTaskAsync(string title, string description, DateTime dueDate, int projectId);
        Task AssignUserToTaskAsync(int taskId, int userId);
        Task UnassignUserFromTaskAsync(int taskId, int userId);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<IEnumerable<TaskEntity>> GetTasksByProjectIdAndUserIdAsync(int projectId, int userId);
        Task<TaskEntity> UpdateTaskAsync(TaskEntity task);
        Task<TaskEntity> GetTaskByIdAsync(int taskId);
    }
}
