using System.Collections.Generic;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Interfaces;

namespace TaskForge.Application.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly IPasswordRepository _passwordRepository;

        public PasswordService(IPasswordRepository passwordRepository)
        {
            _passwordRepository = passwordRepository;
        }

        public Task<List<Password>> GetPasswordsByUserIdAsync(int userId)
        {
            return _passwordRepository.GetPasswordsByUserIdAsync(userId);
        }

        public Task<Password?> GetPasswordByIdAsync(int id)
        {
            return _passwordRepository.GetPasswordByIdAsync(id);
        }

        public Task AddPasswordAsync(Password password)
        {
            return _passwordRepository.AddPasswordAsync(password);
        }

        public Task UpdatePasswordAsync(Password password)
        {
            return _passwordRepository.UpdatePasswordAsync(password);
        }

        public Task DeletePasswordAsync(int id)
        {
            return _passwordRepository.DeletePasswordAsync(id);
        }
    }
}