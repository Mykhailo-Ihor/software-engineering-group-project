using System.Threading.Tasks;
using System.Collections.Generic;
using TaskForge.Domain.Entities;

namespace TaskForge.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserFromAuth0ResponseAsync(string firstName, string lastName, string email, string auth0Id);
        Task<User?> GetUserByAuth0IdAsync(string auth0Id);
        Task<List<User>> GetUsersByProjectIdAsync(int projectId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> IsUserInProjectAsync(int userId, int projectId);
        Task AddUserToProjectAsync(int userId, int projectId, TaskForge.Domain.Enums.Role role);
        Task UpdateUserRoleInProjectAsync(int userId, int projectId, TaskForge.Domain.Enums.Role newRole);
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateUserAsync(User user);
    }
}
