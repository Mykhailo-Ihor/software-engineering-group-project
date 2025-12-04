using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskForge.Application.Services;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;

namespace TaskForge.Tests.Application.Services
{
    public class ExpenseServiceTests
    {
        private readonly Mock<IExpenseRepository> _mockExpenseRepository;
        private readonly ExpenseService _expenseService;

        public ExpenseServiceTests()
      {
   _mockExpenseRepository = new Mock<IExpenseRepository>();
  _expenseService = new ExpenseService(_mockExpenseRepository.Object);
     }

        [Fact]
    public async Task CreateExpenseAsync_ShouldCreateAndReturnExpense_WhenUserExists()
        {
  // Arrange
       var amount = 150.75m;
            var currency = Currency.UAH;
 var category = ExpenceCategory.Ресторани;
            var date = DateTime.UtcNow.AddDays(-1);
            var description = "Обід у кафе";
      var userId = 5;

        var createdExpense = new Expense
 {
              Id = 1,
      Amount = amount,
                Currency = currency,
       Category = category,
          Date = date,
      Description = description,
                UserId = userId
 };

            _mockExpenseRepository
      .Setup(repo => repo.CreateExpenseAsync(amount, currency, category, date, description, userId))
              .ReturnsAsync(createdExpense);

            // Act
            var result = await _expenseService.CreateExpenseAsync(amount, currency, category, date, description, userId);

      // Assert
            Assert.NotNull(result);
            Assert.Equal(amount, result.Amount);
      Assert.Equal(currency, result.Currency);
            Assert.Equal(category, result.Category);
Assert.Equal(description, result.Description);
            Assert.Equal(userId, result.UserId);
     _mockExpenseRepository.Verify(repo => repo.CreateExpenseAsync(amount, currency, category, date, description, userId), Times.Once);
        }

      [Fact]
        public async Task GetUserExpensesAsync_ShouldReturnMappedDtos_WhenExpensesExist()
        {
            // Arrange
     var userId = 3;
var expenses = new List<Expense>
     {
             new Expense
    {
             Id = 1,
 Amount = 100.50m,
   Currency = Currency.USD,
        Category = ExpenceCategory.Подорожі,
     Date = new DateTime(2025, 1, 15),
           Description = "Авіаквитки",
             UserId = userId
              },
                new Expense
           {
       Id = 2,
      Amount = 25.00m,
      Currency = Currency.EUR,
          Category = ExpenceCategory.Продукти,
       Date = new DateTime(2025, 1, 20),
    Description = "Продукти",
      UserId = userId
     }
            };

   _mockExpenseRepository
           .Setup(repo => repo.GetExpensesByUserIdAsync(userId))
   .ReturnsAsync(expenses);

  // Act
            var result = await _expenseService.GetUserExpensesAsync(userId);

   // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);

       var first = result[0];
          Assert.Equal(1, first.Id);
            Assert.Equal(100.50m, first.Amount);
            Assert.Equal("Подорожі", first.Category);
    Assert.Equal("USD", first.Currency);
  Assert.Equal(expenses[0].Date, first.Date);
 Assert.Equal("Авіаквитки", first.Description);

  _mockExpenseRepository.Verify(repo => repo.GetExpensesByUserIdAsync(userId), Times.Once);
     }

        [Fact]
 public async Task GetUserExpensesAsync_ShouldReturnEmptyList_WhenNoExpenses()
   {
      // Arrange
            var userId = 99;
            _mockExpenseRepository
  .Setup(repo => repo.GetExpensesByUserIdAsync(userId))
                .ReturnsAsync(new List<Expense>());

   // Act
       var result = await _expenseService.GetUserExpensesAsync(userId);

            // Assert
       Assert.NotNull(result);
   Assert.Empty(result);
   _mockExpenseRepository.Verify(repo => repo.GetExpensesByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ShouldReturnExpense_WhenExpenseExists()
        {
            // Arrange
     var expenseId = 10;
            var expectedExpense = new Expense
         {
    Id = expenseId,
           Amount = 75.00m,
     Currency = Currency.UAH,
       Category = ExpenceCategory.Підписки,
     Date = DateTime.UtcNow,
      Description = "Netflix"
       };

        _mockExpenseRepository
        .Setup(repo => repo.GetExpenseByIdAsync(expenseId))
   .ReturnsAsync(expectedExpense);

        // Act
 var result = await _expenseService.GetExpenseByIdAsync(expenseId);

            // Assert
            Assert.NotNull(result);
          Assert.Equal(expenseId, result.Id);
            Assert.Equal(expectedExpense.Amount, result.Amount);
            _mockExpenseRepository.Verify(repo => repo.GetExpenseByIdAsync(expenseId), Times.Once);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ShouldReturnNull_WhenExpenseDoesNotExist()
        {
 // Arrange
       var expenseId = 999;
    _mockExpenseRepository
  .Setup(repo => repo.GetExpenseByIdAsync(expenseId))
  .ReturnsAsync((Expense?)null);

            // Act
            var result = await _expenseService.GetExpenseByIdAsync(expenseId);

            // Assert
   Assert.Null(result);
        _mockExpenseRepository.Verify(repo => repo.GetExpenseByIdAsync(expenseId), Times.Once);
        }

        [Fact]
      public async Task DeleteExpenseAsync_ShouldReturnTrue_WhenExpenseExists()
     {
    // Arrange
            var expenseId = 7;
       _mockExpenseRepository
           .Setup(repo => repo.DeleteExpenseAsync(expenseId))
        .ReturnsAsync(true);

          // Act
            var result = await _expenseService.DeleteExpenseAsync(expenseId);

       // Assert
            Assert.True(result);
 _mockExpenseRepository.Verify(repo => repo.DeleteExpenseAsync(expenseId), Times.Once);
        }

        [Fact]
     public async Task DeleteExpenseAsync_ShouldReturnFalse_WhenExpenseDoesNotExist()
    {
         // Arrange
  var expenseId = 888;
            _mockExpenseRepository
     .Setup(repo => repo.DeleteExpenseAsync(expenseId))
   .ReturnsAsync(false);

         // Act
     var result = await _expenseService.DeleteExpenseAsync(expenseId);

        // Assert
         Assert.False(result);
         _mockExpenseRepository.Verify(repo => repo.DeleteExpenseAsync(expenseId), Times.Once);
        }

        [Fact]
   public async Task UpdateExpenseAsync_ShouldUpdateAndReturnExpense_WhenCalled()
        {
          // Arrange
      var updatedExpense = new Expense
            {
    Id = 4,
          Amount = 199.99m,
                Currency = Currency.USD,
       Category = ExpenceCategory.Книги,
        Date = new DateTime(2025, 2, 10),
          Description = "Книги по програмуванню",
                UserId = 2
};

   _mockExpenseRepository
                .Setup(repo => repo.UpdateExpenseAsync(updatedExpense))
     .ReturnsAsync(updatedExpense);

            // Act
  var result = await _expenseService.UpdateExpenseAsync(updatedExpense);

    // Assert
        Assert.NotNull(result);
            Assert.Equal(updatedExpense.Id, result.Id);
  Assert.Equal(199.99m, result.Amount);
  Assert.Equal("Книги по програмуванню", result.Description);
  _mockExpenseRepository.Verify(repo => repo.UpdateExpenseAsync(updatedExpense), Times.Once);
        }
    }
}
