using System.Threading.Tasks;
using System.Collections.Generic;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Application.DTOs;
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
        public async Task<UserDto?> GetUserByAuth0IdAsync(string auth0Id)
        {
            var user = await _userRepository.GetUserByAuth0IdAsync(auth0Id);
            return user == null ? null : new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Auth0UserId = user.Auth0UserId
            };
        }
        public async Task<List<UserDto>> GetUsersByProjectIdAsync(int projectId)
        {
            var users = await _userRepository.GetUsersByProjectIdAsync(projectId);
            return users.ConvertAll(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Auth0UserId = u.Auth0UserId
            });
        }
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user == null ? null : new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Auth0UserId = user.Auth0UserId
            };
        }
        public Task<bool> IsUserInProjectAsync(int userId, int projectId)
            => _userRepository.IsUserInProjectAsync(userId, projectId);
        public Task AddUserToProjectAsync(int userId, int projectId, Role role)
            => _userRepository.AddUserToProjectAsync(userId, projectId, role);

        public async Task UpdateUserRoleInProjectAsync(int userId, int projectId, Role newRole)
        {
            await _userRepository.UpdateUserRoleInProjectAsync(userId, projectId, newRole);
        }

        public async Task UpdateUserProfileAsync(int userId, string firstName, string lastName, string email)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            await _userRepository.UpdateUserAsync(user);
        }
    }
}
