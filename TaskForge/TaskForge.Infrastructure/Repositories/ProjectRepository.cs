using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Enums;

namespace TaskForge.Infrastructure.Repositories;

public class ProjectRepository
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
}
