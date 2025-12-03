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
        public event Action ExpensesChanged;
        public ExpenseService(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public async Task<Expense> CreateExpenseAsync(decimal amount, Currency currency, ExpenceCategory category, DateTime date, string description, TransactionType type, int userId)
        {
            var result = await _expenseRepository.CreateExpenseAsync(amount, currency, category, date, description, type, userId);
            ExpensesChanged?.Invoke();
            return result;
        }

        public async Task<List<ExpenceRecordDto>> GetUserExpensesAsync(int userId)
        {
            var expenses = await _expenseRepository.GetExpensesByUserIdAsync(userId);

            return expenses.Select(e => new ExpenceRecordDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Category = e.Type == TransactionType.Income ? "Дохід" : e.Category.ToString(),
                Date = e.Date,
                Currency = e.Currency.ToString(),
                Description = e.Description,
                Type = e.Type.ToString()
            }).ToList();
        }

        public Task<Expense?> GetExpenseByIdAsync(int expenseId)
            => _expenseRepository.GetExpenseByIdAsync(expenseId);

        public async Task<bool> DeleteExpenseAsync(int expenseId)
        {
            var result = await _expenseRepository.DeleteExpenseAsync(expenseId);

            if (result)
            {
                ExpensesChanged?.Invoke();
            }

            return result;
        }

        public async Task<Expense> UpdateExpenseAsync(Expense expense)
        {
            var result = await _expenseRepository.UpdateExpenseAsync(expense);
            ExpensesChanged?.Invoke();
            return result;
        }
    }
}