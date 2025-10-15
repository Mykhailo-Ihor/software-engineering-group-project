using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;  

namespace TaskForge.Infrastructure.Repositories;

public class ProjectRepository:IProjectRepository
{
    private readonly TaskForgeDbContext _context;

    public ProjectRepository(TaskForgeDbContext context)
    {
        _context = context;
    }

    public async Task<Project> CreateProjectForUserAsync(string name, string status, string description, int userId, Role role = Role.Member)
    {
        var project = new Project
        {
            Name = name,
            Status = status,
            Description = description
        };
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        var projectUser = new ProjectUser
        {
            UserId = userId,
            ProjectId = project.Id,
            Role = role
        };
        await _context.ProjectUsers.AddAsync(projectUser);
        await _context.SaveChangesAsync();

        return project;
    }

    public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(int userId)
    {
        // We find all projects where the ProjectUsers navigation property contains
        // any link with the specified UserId.
        // We also use Include to load the related ProjectUsers data. 
        // This is crucial for determining the user's role in the Application layer.
        return await _context.Projects
            .Include(p => p.ProjectUsers)
            .Where(p => p.ProjectUsers.Any(pu => pu.UserId == userId))
            .ToListAsync();
    }
}
