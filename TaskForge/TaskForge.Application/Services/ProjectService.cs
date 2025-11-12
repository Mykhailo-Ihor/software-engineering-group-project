using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;

namespace TaskForge.Application.Services;
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    /// <summary>
    /// Initializes a new instance of the ProjectService class.
    /// </summary>
    /// <param name="projectRepository">The project repository dependency, injected by the DI container.</param>
    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int userId)
    {
        var projects = await _projectRepository.GetProjectsByUserIdAsync(userId);

        // Як буде час замінити на автомапер
        return projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Status = p.Status,
            UserRoleInProject = p.ProjectUsers
                                 .FirstOrDefault(pu => pu.UserId == userId)?
                                 .Role.ToString() ?? "Unknown"
        });
    }

    public Task<List<TaskEntity>> GetTasksByProjectIdAsync(int projectId)
        => _projectRepository.GetTasksByProjectIdAsync(projectId);

    public Task<List<Project>> GetProjectsForUserAsync(int userId)
        => _projectRepository.GetProjectsForUserAsync(userId);

    public Task<Project> CreateProjectForUserAsync(string name, string status, string description, int userId, Role role)
        => _projectRepository.CreateProjectForUserAsync(name, status, description, userId, role);
    public async Task<ProjectUser> GetProjectUserAsync(int userId, int projectId)
    {
        return await _projectRepository.GetProjectUserAsync(userId, projectId);
    }
    public async Task<Project> GetProjectByIdAsync(int projectId)
    {
        return await _projectRepository.GetProjectByIdAsync(projectId);
    }
    public async Task<Project> UpdateProjectAsync(int projectId, string name, string description, string status)
    {
        var project = await _projectRepository.GetProjectByIdAsync(projectId);
        if (project == null)
        {
            throw new Exception($"Проєкт з ID {projectId} не знайдено.");
        }

        project.Name = name;
        project.Description = description;
        project.Status = status;

        return await _projectRepository.UpdateProjectAsync(project);
    }
    public async Task<List<ProjectUser>> GetProjectUsersAsync(int projectId)
    {
        return await _projectRepository.GetProjectUsersAsync(projectId);
    }
    public Task DeleteProjectAsync(int projectId)
    {
        return _projectRepository.DeleteProjectAsync(projectId);
    }
}