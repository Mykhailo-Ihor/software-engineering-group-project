using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks; 
namespace TaskForge.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
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
        return await _context.Projects
            .Include(p => p.ProjectUsers)
            .ThenInclude(pu => pu.User)
            .Where(p => p.ProjectUsers.Any(pu => pu.UserId == userId))
            .ToListAsync();
    }
    public async Task<List<Project>> GetProjectsForUserAsync(int userId)
    {
        return await _context.ProjectUsers
            .Where(pu => pu.UserId == userId)
            .Select(pu => pu.Project)
            .ToListAsync();
    }

    public async Task<List<TaskEntity>> GetTasksByProjectIdAsync(int projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.TaskUsers)
            .ThenInclude(tu => tu.User)
            .ToListAsync();
    }
    public async Task<ProjectUser> GetProjectUserAsync(int userId, int projectId)
    {
        return await _context.ProjectUsers.FirstOrDefaultAsync(pu => pu.UserId == userId && pu.ProjectId == projectId);
    }
    public async Task<Project> GetProjectByIdAsync(int projectId)
    {
        return await _context.Projects.FindAsync(projectId);
    }
    public async Task<Project> UpdateProjectAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }
    public async Task<List<ProjectUser>> GetProjectUsersAsync(int projectId)
    {
        return await _context.ProjectUsers
            .Where(pu => pu.ProjectId == projectId)
            .Include(pu => pu.User)
            .ToListAsync();
    }
    public async Task DeleteProjectAsync(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }
}
