using System.Collections.Generic;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<Expense> CreateExpenseAsync(decimal amount, Currency currency, ExpenceCategory category, DateTime date, string description, int userId);
        Task<List<ExpenceRecordDto>> GetUserExpensesAsync(int userId);
        Task<Expense?> GetExpenseByIdAsync(int expenseId);
        Task<bool> DeleteExpenseAsync(int expenseId);
        Task<Expense> UpdateExpenseAsync(Expense expense);
    }
}