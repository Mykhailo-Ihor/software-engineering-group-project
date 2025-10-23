using TaskForge.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskForge.Domain.Interfaces;
using TaskForge.Infrastructure.Repositories;

namespace TaskForge.Application.Services
{
    public interface ITaskFilterService
    {
        Task<IEnumerable<TaskEntity>> GetTasksForProjectAsync(int projectId, int? userId = null);
    }

    public class TaskFilterService : ITaskFilterService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskFilterService(IProjectRepository projectRepository, ITaskRepository taskRepository)
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