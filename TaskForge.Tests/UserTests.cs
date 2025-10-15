using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TaskForge.Infrastructure.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;

namespace TaskForge.Tests
{
    public class UserRepositoryTests
    {
        private List<User> GetFakeUsers() => new List<User>
        {
            new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Auth0UserId = "auth0|123" },
            new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Auth0UserId = "auth0|456" }
        };

        private TaskForgeDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TaskForgeDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new TaskForgeDbContext(options);
        }

        [Fact]
        public async Task AddUserFromAuth0ResponseAsync_ShouldAddUser_WhenUserDoesNotExist()
        {
            // Arrange
            var dbName = nameof(AddUserFromAuth0ResponseAsync_ShouldAddUser_WhenUserDoesNotExist);
            using var context = CreateInMemoryDbContext(dbName);
            context.Users.AddRange(GetFakeUsers());
            context.SaveChanges();
            var repo = new UserRepository(context);

            // Act
            var result = await repo.AddUserFromAuth0ResponseAsync("Alice", "Wonder", "alice@example.com", "auth0|789");

            // Assert
            Assert.True(result);
            Assert.Contains(context.Users, u => u.Auth0UserId == "auth0|789");
        }

        [Fact]
        public async Task AddUserFromAuth0ResponseAsync_ShouldNotAddUser_WhenUserExists()
        {
            // Arrange
            var dbName = nameof(AddUserFromAuth0ResponseAsync_ShouldNotAddUser_WhenUserExists);
            using var context = CreateInMemoryDbContext(dbName);
            context.Users.AddRange(GetFakeUsers());
            context.SaveChanges();
            var repo = new UserRepository(context);

            // Act
            var result = await repo.AddUserFromAuth0ResponseAsync("John", "Doe", "john@example.com", "auth0|123");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserByAuth0IdAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            var dbName = nameof(GetUserByAuth0IdAsync_ShouldReturnUser_WhenExists);
            using var context = CreateInMemoryDbContext(dbName);
            context.Users.AddRange(GetFakeUsers());
            context.SaveChanges();
            var repo = new UserRepository(context);

            // Act
            var user = await repo.GetUserByAuth0IdAsync("auth0|123");

            // Assert
            Assert.NotNull(user);
            Assert.Equal("John", user.FirstName);
        }

        [Fact]
        public async Task GetUserByAuth0IdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var dbName = nameof(GetUserByAuth0IdAsync_ShouldReturnNull_WhenNotExists);
            using var context = CreateInMemoryDbContext(dbName);
            context.Users.AddRange(GetFakeUsers());
            context.SaveChanges();
            var repo = new UserRepository(context);

            // Act
            var user = await repo.GetUserByAuth0IdAsync("auth0|999");

            // Assert
            Assert.Null(user);
        }
    }
}
