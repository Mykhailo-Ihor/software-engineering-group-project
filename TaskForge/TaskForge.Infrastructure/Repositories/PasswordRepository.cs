using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Interfaces;
using TaskForge.Infrastructure.Data;


namespace TaskForge.Infrastructure.Repositories;

public class PasswordRepository : IPasswordRepository
{
    private readonly TaskForgeDbContext _context;

    public PasswordRepository(TaskForgeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Password>> GetPasswordsByUserIdAsync(int userId)
    {
        return await _context.Passwords.Where(p => p.UserId == userId).ToListAsync();
    }

    public async Task<Password?> GetPasswordByIdAsync(int id)
    {
        return await _context.Passwords.FindAsync(id);
    }

    public async Task AddPasswordAsync(Password password)
    {
        // Ensure the Id is not set to avoid conflicts with existing entities
        password.Id = 0;

        // Encrypt the password before saving
        password.PasswordEncrypted = EncryptPassword(password.PasswordEncrypted);

        await _context.Passwords.AddAsync(password);
        await _context.SaveChangesAsync();
    }

    private string EncryptPassword(string plainText)
    {
        // Simple encryption logic using Base64 encoding (for demonstration purposes)
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    private string DecryptPassword(string encryptedText)
    {
        // Simple decryption logic using Base64 decoding (for demonstration purposes)
        var encryptedBytes = System.Convert.FromBase64String(encryptedText);
        return System.Text.Encoding.UTF8.GetString(encryptedBytes);
    }

    public async Task UpdatePasswordAsync(Password password)
    {
        var existingPassword = await _context.Passwords.FindAsync(password.Id);
        if (existingPassword != null)
        {
            _context.Entry(existingPassword).CurrentValues.SetValues(password);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException("Password not found for update.");
        }
    }

    public async Task DeletePasswordAsync(int id)
    {
        var password = await GetPasswordByIdAsync(id);
        if (password != null)
        {
            _context.Passwords.Remove(password);
            await _context.SaveChangesAsync();
        }
    }
}
