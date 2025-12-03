using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly TaskForgeDbContext _context;

        public ExpenseRepository(TaskForgeDbContext context)
        {
            _context = context;
        }

        public async Task<Expense> CreateExpenseAsync(decimal amount, Currency currency, ExpenceCategory category, DateTime date, string description, TransactionType type, int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new InvalidOperationException($"Користувача з ID {userId} не знайдено.");
            }

            var expense = new Expense
            {
                Amount = amount,
                Currency = currency,
                Category = category,
                Date = date,
                Description = description,
                Type = type,
                UserId = userId
            };

            await _context.Expenses.AddAsync(expense);
            await _context.SaveChangesAsync();

            return expense;
        }

        public async Task<List<Expense>> GetExpensesByUserIdAsync(int userId)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
        {
            return await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == expenseId);
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId)
        {
            var expense = await _context.Expenses.FindAsync(expenseId);

            if (expense == null)
            {
                return false;
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Expense> UpdateExpenseAsync(Expense expense)
        {
            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
            return expense;
        }
    }
}