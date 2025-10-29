using System.Threading.Tasks;
using System.Collections.Generic;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Interfaces;
namespace TaskForge.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public Task AddUserFromAuth0ResponseAsync(string firstName, string lastName, string email, string auth0Id)
            => _userRepository.AddUserFromAuth0ResponseAsync(firstName, lastName, email, auth0Id);
        public Task<User?> GetUserByAuth0IdAsync(string auth0Id)
            => _userRepository.GetUserByAuth0IdAsync(auth0Id);
        public Task<List<User>> GetUsersByProjectIdAsync(int projectId)
            => _userRepository.GetUsersByProjectIdAsync(projectId);
        public Task<User?> GetUserByEmailAsync(string email)
            => _userRepository.GetUserByEmailAsync(email);
        public Task<bool> IsUserInProjectAsync(int userId, int projectId)
            => _userRepository.IsUserInProjectAsync(userId, projectId);
        public Task AddUserToProjectAsync(int userId, int projectId, Role role)
            => _userRepository.AddUserToProjectAsync(userId, projectId, role);

        public async Task UpdateUserRoleInProjectAsync(int userId, int projectId, Role newRole)
        {
            await _userRepository.UpdateUserRoleInProjectAsync(userId, projectId, newRole);
        }
    }
}
