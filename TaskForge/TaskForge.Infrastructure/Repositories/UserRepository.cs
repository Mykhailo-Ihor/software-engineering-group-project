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

    public async Task<bool> AddUserFromAuth0ResponseAsync(string firstName, string lastName, string email, string auth0UserId)
    {
        var existingUser = await GetUserByAuth0IdAsync(auth0UserId);
        if (existingUser != null)
        {
            return false;
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Auth0UserId = auth0UserId
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<User?> GetUserByAuth0IdAsync(string auth0UserId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId);
    }
}