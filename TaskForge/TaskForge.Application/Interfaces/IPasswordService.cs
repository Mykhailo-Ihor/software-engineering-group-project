using System.Collections.Generic;
using System.Threading.Tasks;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces
{
    public interface IPasswordService
    {
        Task<List<Password>> GetPasswordsByUserIdAsync(int userId);
        Task<Password?> GetPasswordByIdAsync(int id);
        Task AddPasswordAsync(Password password);
        Task UpdatePasswordAsync(Password password);
        Task DeletePasswordAsync(int id);
    }
}