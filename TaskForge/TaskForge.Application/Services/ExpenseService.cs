using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;

namespace TaskForge.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;

        public ExpenseService(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public Task<Expense> CreateExpenseAsync(decimal amount, Currency currency, ExpenceCategory category, DateTime date, string description, int userId)
            => _expenseRepository.CreateExpenseAsync(amount, currency, category, date, description, userId);

        public async Task<List<ExpenceRecordDto>> GetUserExpensesAsync(int userId)
        {
            var expenses = await _expenseRepository.GetExpensesByUserIdAsync(userId);

            return expenses.Select(e => new ExpenceRecordDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Category = e.Category.ToString(),
                Date = e.Date,
                Currency = e.Currency.ToString(),
                Description = e.Description
            }).ToList();
        }

        public Task<Expense?> GetExpenseByIdAsync(int expenseId)
            => _expenseRepository.GetExpenseByIdAsync(expenseId);

        public Task<bool> DeleteExpenseAsync(int expenseId)
            => _expenseRepository.DeleteExpenseAsync(expenseId);

        public Task<Expense> UpdateExpenseAsync(Expense expense)
            => _expenseRepository.UpdateExpenseAsync(expense);
    }
}