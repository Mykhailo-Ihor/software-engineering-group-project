using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
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
}