using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IProjectService
{
    /// <summary>
    /// Gets all projects for a user, formatted as DTOs.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A collection of project DTOs.</returns>
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int userId);

    /// <summary>
    /// Gets all tasks for a project by project ID.
    /// </summary>
    /// <param name="projectId">The project's ID.</param>
    /// <returns>A list of tasks for the project.</returns>
    Task<List<TaskEntity>> GetTasksByProjectIdAsync(int projectId);

    /// <summary>
    /// Gets all projects for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A list of projects for the user.</returns>
    Task<List<Project>> GetProjectsForUserAsync(int userId);

    /// <summary>
    /// Creates a new project for a user.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="status">The status of the project.</param>
    /// <param name="description">The description of the project.</param>
    /// <param name="userId">The user's ID.</param>
    /// <param name="role">The role of the user in the project.</param>
    /// <returns>The created project.</returns>
    Task<Project> CreateProjectForUserAsync(string name, string status, string description, int userId, Role role);
    Task<ProjectUser> GetProjectUserAsync(int userId, int projectId);
}