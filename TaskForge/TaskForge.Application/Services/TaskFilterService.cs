using TaskForge.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskForge.Infrastructure.Repositories;

namespace TaskForge.Application.Services
{
    public interface ITaskFilterService
    {
        Task<IEnumerable<TaskEntity>> GetTasksForProjectAsync(int projectId, int? userId = null);
    }

    public class TaskFilterService : ITaskFilterService
    {
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;

        public TaskFilterService(ProjectRepository projectRepository, TaskRepository taskRepository)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
        }

        public async Task<IEnumerable<TaskEntity>> GetTasksForProjectAsync(int projectId, int? userId = null)
        {
            if (userId.HasValue)
            {
                return await _taskRepository.GetTasksByProjectIdAndUserIdAsync(projectId, userId.Value);
            }
            else
            {
                var tasks = await _projectRepository.GetTasksByProjectIdAsync(projectId);
                return tasks;
            }
        }
    }
}