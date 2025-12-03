using System.Collections.Generic;
using System.Threading.Tasks;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Domain.Interfaces
{
    public interface IExpenseRepository
    {
        Task<Expense> CreateExpenseAsync(decimal amount, Currency currency, ExpenceCategory category, DateTime date, string description, TransactionType type, int userId);
        Task<List<Expense>> GetExpensesByUserIdAsync(int userId);
        Task<Expense?> GetExpenseByIdAsync(int expenseId);
        Task<bool> DeleteExpenseAsync(int expenseId);
        Task<Expense> UpdateExpenseAsync(Expense expense);
    }
}