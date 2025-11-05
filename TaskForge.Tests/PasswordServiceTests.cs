using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Interfaces;
using TaskForge.Application.Services;
using TaskForge.Application.Interfaces;

namespace TaskForge.Tests
{
    public class PasswordServiceTests
    {
        private readonly Mock<IPasswordRepository> _mockPasswordRepository;
        private readonly IPasswordService _passwordService;

        public PasswordServiceTests()
        {
            _mockPasswordRepository = new Mock<IPasswordRepository>();
            _passwordService = new PasswordService(_mockPasswordRepository.Object);
        }

        [Fact]
        public async Task GetPasswordsByUserIdAsync_WhenCalled_ReturnsPasswords()
        {
            // Arrange
            int userId = 1;
            var passwords = new List<Password>
            {
                new Password { Id = 1, UserId = userId, Login = "Password1" },
                new Password { Id = 2, UserId = userId, Login = "Password2" }
            };
            _mockPasswordRepository.Setup(r => r.GetPasswordsByUserIdAsync(userId)).ReturnsAsync(passwords);

            // Act
            var result = await _passwordService.GetPasswordsByUserIdAsync(userId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Password1", result[0].Login);
        }

        [Fact]
        public async Task GetPasswordByIdAsync_WhenPasswordExists_ReturnsPassword()
        {
            // Arrange
            int passwordId = 1;
            var password = new Password { Id = passwordId, Login = "Password1" };
            _mockPasswordRepository.Setup(r => r.GetPasswordByIdAsync(passwordId)).ReturnsAsync(password);

            // Act
            var result = await _passwordService.GetPasswordByIdAsync(passwordId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Password1", result.Login);
        }

        [Fact]
        public async Task GetPasswordByIdAsync_WhenPasswordDoesNotExist_ReturnsNull()
        {
            // Arrange
            int passwordId = 99;
            _mockPasswordRepository.Setup(r => r.GetPasswordByIdAsync(passwordId)).ReturnsAsync((Password)null);

            // Act
            var result = await _passwordService.GetPasswordByIdAsync(passwordId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddPasswordAsync_WhenCalled_AddsPassword()
        {
            // Arrange
            var password = new Password { Id = 1, Login = "NewPassword" };
            _mockPasswordRepository.Setup(r => r.AddPasswordAsync(password)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _passwordService.AddPasswordAsync(password);

            // Assert
            _mockPasswordRepository.Verify(r => r.AddPasswordAsync(password), Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenCalled_UpdatesPassword()
        {
            // Arrange
            var password = new Password { Id = 1, Login = "UpdatedPassword" };
            _mockPasswordRepository.Setup(r => r.UpdatePasswordAsync(password)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _passwordService.UpdatePasswordAsync(password);

            // Assert
            _mockPasswordRepository.Verify(r => r.UpdatePasswordAsync(password), Times.Once);
        }

        [Fact]
        public async Task DeletePasswordAsync_WhenCalled_DeletesPassword()
        {
            // Arrange
            int passwordId = 1;
            _mockPasswordRepository.Setup(r => r.DeletePasswordAsync(passwordId)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _passwordService.DeletePasswordAsync(passwordId);

            // Assert
            _mockPasswordRepository.Verify(r => r.DeletePasswordAsync(passwordId), Times.Once);
        }
    }
}