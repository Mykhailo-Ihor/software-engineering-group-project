using System.Threading.Tasks;
using System.Collections.Generic;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Application.DTOs;

namespace TaskForge.Application.Interfaces
{
    public interface IUserService
    {
        Task AddUserFromAuth0ResponseAsync(string firstName, string lastName, string email, string auth0Id);
        Task<UserDto?> GetUserByAuth0IdAsync(string auth0Id);
        Task<List<UserDto>> GetUsersByProjectIdAsync(int projectId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<bool> IsUserInProjectAsync(int userId, int projectId);
        Task AddUserToProjectAsync(int userId, int projectId, Role role);
        Task UpdateUserRoleInProjectAsync(int userId, int projectId, Role newRole);
        Task UpdateUserProfileAsync(int userId, string firstName, string lastName, string email);
        Task RemoveUserFromProjectAsync(int userId, int projectId);
    }
}
