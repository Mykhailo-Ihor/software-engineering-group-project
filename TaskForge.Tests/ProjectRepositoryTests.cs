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
        private User CreateFakeUser(int id = 1, string authId = "auth0|123") => new User
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = $"john{id}@example.com",
            Auth0UserId = authId
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
            await using var context = CreateInMemoryDbContext(dbName);
            var user = CreateFakeUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var repo = new ProjectRepository(context);

            // Act
            var project = await repo.CreateProjectForUserAsync("Test Project", "Active", "A test project", user.Id, Role.Moderator);

            // Assert
            Assert.NotNull(project);
            Assert.Equal("Test Project", project.Name);
            var projectUser = await context.ProjectUsers.FirstOrDefaultAsync(pu => pu.ProjectId == project.Id && pu.UserId == user.Id);
            Assert.NotNull(projectUser);
            Assert.Equal(Role.Moderator, projectUser.Role);
        }

        [Fact]
        public async Task GetProjectUserAsync_ShouldReturnProjectUser_WhenExists()
        {
            // Arrange
            var dbName = nameof(GetProjectUserAsync_ShouldReturnProjectUser_WhenExists);
            await using var context = CreateInMemoryDbContext(dbName);
            var user = CreateFakeUser();
            // Додаємо обов'язкові поля Status та Description
            var project = new Project { Id = 1, Name = "Test Project", Status = "Active", Description = "Test Desc" };
            var projectUser = new ProjectUser { UserId = user.Id, ProjectId = project.Id, Role = Role.Moderator };
            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectUsers.Add(projectUser);
            await context.SaveChangesAsync();
            var repo = new ProjectRepository(context);

            // Act
            var result = await repo.GetProjectUserAsync(user.Id, project.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(project.Id, result.ProjectId);
            Assert.Equal(Role.Moderator, result.Role);
        }

        [Fact]
        public async Task GetProjectUserAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var dbName = nameof(GetProjectUserAsync_ShouldReturnNull_WhenNotExists);
            await using var context = CreateInMemoryDbContext(dbName);
            var repo = new ProjectRepository(context);

            // Act
            var result = await repo.GetProjectUserAsync(99, 99); 

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserRoleInProjectAsync_ShouldUpdateRole_WhenProjectUserExists()
        {
            // Arrange
            var dbName = nameof(UpdateUserRoleInProjectAsync_ShouldUpdateRole_WhenProjectUserExists);
            await using var context = CreateInMemoryDbContext(dbName);
            var user = CreateFakeUser();
            
            var project = new Project { Id = 1, Name = "Test Project", Status = "Active", Description = "Test Desc" };
            var projectUser = new ProjectUser { UserId = user.Id, ProjectId = project.Id, Role = Role.Member };
            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectUsers.Add(projectUser);
            await context.SaveChangesAsync();
            var repo = new UserRepository(context); 

            // Act
            await repo.UpdateUserRoleInProjectAsync(user.Id, project.Id, Role.Moderator); 

            // Assert
            var updatedProjectUser = await context.ProjectUsers.FindAsync(projectUser.Id);
            Assert.NotNull(updatedProjectUser);
            Assert.Equal(Role.Moderator, updatedProjectUser.Role);
        }
    }
}