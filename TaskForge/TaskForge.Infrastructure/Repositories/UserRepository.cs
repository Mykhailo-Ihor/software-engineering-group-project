using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskForge.Infrastructure.Repositories;

public class UserRepository
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
}