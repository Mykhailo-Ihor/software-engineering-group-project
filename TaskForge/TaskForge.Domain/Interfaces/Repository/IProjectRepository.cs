using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

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
}