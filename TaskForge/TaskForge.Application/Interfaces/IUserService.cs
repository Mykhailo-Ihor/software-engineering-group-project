using System.Threading.Tasks;
using System.Collections.Generic;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Interfaces
{
    public interface IUserService
    {
        Task AddUserFromAuth0ResponseAsync(string firstName, string lastName, string email, string auth0Id);
        Task<User?> GetUserByAuth0IdAsync(string auth0Id);
        Task<List<User>> GetUsersByProjectIdAsync(int projectId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> IsUserInProjectAsync(int userId, int projectId);
        Task AddUserToProjectAsync(int userId, int projectId, Role role);
    }
}
