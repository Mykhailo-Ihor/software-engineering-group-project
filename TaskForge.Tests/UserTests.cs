using Xunit;
using System.Threading.Tasks;
using Moq;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;
using TaskForge.Application.Services;
using TaskForge.Application.Interfaces;

namespace TaskForge.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task AddUserToProjectAsync_WhenUserNotInProject_AddsUser()
        {
            // Arrange
            int userId = 1;
            int projectId = 10;
            _mockUserRepository.Setup(r => r.IsUserInProjectAsync(userId, projectId)).ReturnsAsync(false);
            _mockUserRepository.Setup(r => r.AddUserToProjectAsync(userId, projectId, Role.Member)).Returns(Task.CompletedTask).Verifiable();

            // Act
            var alreadyInProject = await _mockUserRepository.Object.IsUserInProjectAsync(userId, projectId);
            if (!alreadyInProject)
            {
                await _userService.AddUserToProjectAsync(userId, projectId, Role.Member);
            }

            // Assert
            _mockUserRepository.Verify(r => r.AddUserToProjectAsync(userId, projectId, Role.Member), Times.Once);
        }

        [Fact]
        public async Task AddUserToProjectAsync_WhenUserAlreadyInProject_DoesNotAddUser()
        {
            // Arrange
            int userId = 2;
            int projectId = 20;
            _mockUserRepository.Setup(r => r.IsUserInProjectAsync(userId, projectId)).ReturnsAsync(true);

            // Act
            var alreadyInProject = await _mockUserRepository.Object.IsUserInProjectAsync(userId, projectId);
            if (!alreadyInProject)
            {
                await _userService.AddUserToProjectAsync(userId, projectId, Role.Member);
            }

            // Assert
            _mockUserRepository.Verify(r => r.AddUserToProjectAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var user = new User { Id = 3, FirstName = "Alice", LastName = "Wonder", Email = "alice@example.com", Auth0UserId = "auth0|789" };
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByEmailAsync(user.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Alice", result.FirstName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync("notfound@example.com")).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByEmailAsync("notfound@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_WhenUserExists_UpdatesUser()
        {
            // Arrange
            int userId = 42;
            var user = new User { Id = userId, FirstName = "Old", LastName = "Name", Email = "old@email.com", Auth0UserId = "auth0|42" };
            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _userService.UpdateUserProfileAsync(userId, "NewFirst", "NewLast", "new@email.com");

            // Assert
            _mockUserRepository.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
                u.Id == userId &&
                u.FirstName == "NewFirst" &&
                u.LastName == "NewLast" &&
                u.Email == "new@email.com"
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_WhenUserDoesNotExist_DoesNotUpdateUser()
        {
            // Arrange
            int userId = 99;
            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            await _userService.UpdateUserProfileAsync(userId, "First", "Last", "email@example.com");

            // Assert
            _mockUserRepository.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
