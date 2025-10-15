using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
public interface IProjectService
{
    //Task<Project> CreateProjectAsync(ProjectDto createProjectDto);

    /// <summary>
    /// Gets all projects for a user, formatted as DTOs.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A collection of project DTOs.</returns>
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int userId);
}