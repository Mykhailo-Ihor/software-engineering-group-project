using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using System.Collections.Generic; 
using System.Threading.Tasks;

namespace TaskForge.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project> CreateProjectForUserAsync(string name, string status, string description, int userId, Role role);

    /// <summary>
    /// Gets all projects associated with a specific user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A collection of projects.</returns>
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(int userId);

    Task<List<Project>> GetProjectsForUserAsync(int userId);
    Task<List<TaskEntity>> GetTasksByProjectIdAsync(int projectId);
    Task<ProjectUser> GetProjectUserAsync(int userId, int projectId);
    Task<List<ProjectUser>> GetProjectUsersAsync(int projectId);
    Task<Project> GetProjectByIdAsync(int projectId);
    Task<Project> UpdateProjectAsync(Project project);
    Task DeleteProjectAsync(int projectId);
}