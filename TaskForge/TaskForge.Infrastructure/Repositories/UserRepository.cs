using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Interfaces;

namespace TaskForge.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TaskForgeDbContext _context;

    public UserRepository(TaskForgeDbContext context)
    {
        _context = context;
    }

    public async Task AddUserFromAuth0ResponseAsync(string firstName, string lastName, string email, string auth0Id)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Auth0UserId == auth0Id);

        if (!userExists)
        {
            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Auth0UserId = auth0Id
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User?> GetUserByAuth0IdAsync(string auth0Id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Auth0UserId == auth0Id);
    }

    public async Task<List<User>> GetUsersByProjectIdAsync(int projectId)
    {
        return await _context.ProjectUsers
            .Where(pu => pu.ProjectId == projectId)
            .Select(pu => pu.User)
            .Distinct()
            .ToListAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task<bool> IsUserInProjectAsync(int userId, int projectId)
    {
        return await _context.ProjectUsers.AnyAsync(pu => pu.UserId == userId && pu.ProjectId == projectId);
    }

    public async Task AddUserToProjectAsync(int userId, int projectId, TaskForge.Domain.Enums.Role role)
    {
        var projectUser = new ProjectUser
        {
            UserId = userId,
            ProjectId = projectId,
            Role = role
        };
        _context.ProjectUsers.Add(projectUser);
        await _context.SaveChangesAsync();
    }
}