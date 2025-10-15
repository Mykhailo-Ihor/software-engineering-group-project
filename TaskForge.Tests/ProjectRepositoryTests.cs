using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TaskForge.Infrastructure.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using TaskForge.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace TaskForge.Tests
{
    public class ProjectRepositoryTests
    {
        private User CreateFakeUser() => new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Auth0UserId = "auth0|123"
        };

        private TaskForgeDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TaskForgeDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new TaskForgeDbContext(options);
        }

        [Fact]
        public async Task CreateProjectForUserAsync_ShouldCreateProjectAndAssignUser()
        {
            // Arrange
            var dbName = nameof(CreateProjectForUserAsync_ShouldCreateProjectAndAssignUser);
            using var context = CreateInMemoryDbContext(dbName);
            var user = CreateFakeUser();
            context.Users.Add(user);
            context.SaveChanges();
            var repo = new ProjectRepository(context);

            // Act
            var project = await repo.CreateProjectForUserAsync("Test Project", "Active", "A test project", user.Id, Role.Moderator);

            // Assert
            Assert.NotNull(project);
            Assert.Equal("Test Project", project.Name);
            Assert.Equal("Active", project.Status);
            Assert.Equal("A test project", project.Description);
            var projectUser = context.ProjectUsers.FirstOrDefault(pu => pu.ProjectId == project.Id && pu.UserId == user.Id);
            Assert.NotNull(projectUser);
            Assert.Equal(Role.Moderator, projectUser.Role);
        }
    }
}
