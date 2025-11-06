using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Infrastructure.Repositories;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Interfaces;

namespace TaskForge.Tests.Application.Services
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly ProjectService _projectService;

        public ProjectServiceTests()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _projectService = new ProjectService(_mockProjectRepository.Object);
        }

        [Fact]
        public async Task GetUserProjectsAsync_WhenUserHasProjects_ReturnsProjectDtos()
        {
            // Arrange
            int userId = 1;
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    Name = "Project 1",
                    Description = "Description 1",
                    Status = "Active",
                    ProjectUsers = new List<ProjectUser>
                    {
                        new ProjectUser { UserId = 1, Role = Role.Member }
                    }
                },
                new Project
                {
                    Id = 2,
                    Name = "Project 2",
                    Description = "Description 2",
                    Status = "Completed",
                    ProjectUsers = new List<ProjectUser>
                    {
                        new ProjectUser { UserId = 1, Role = Role.Moderator }
                    }
                }
            };

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(projects);

            // Act
            var result = await _projectService.GetUserProjectsAsync(userId);

            // Assert
            var projectDtos = result.ToList();
            Assert.NotNull(projectDtos);
            Assert.Equal(2, projectDtos.Count);

            Assert.Equal(1, projectDtos[0].Id);
            Assert.Equal("Project 1", projectDtos[0].Name);
            Assert.Equal("Description 1", projectDtos[0].Description);
            Assert.Equal("Active", projectDtos[0].Status);
            Assert.Equal("Member", projectDtos[0].UserRoleInProject);

            Assert.Equal(2, projectDtos[1].Id);
            Assert.Equal("Project 2", projectDtos[1].Name);
            Assert.Equal("Description 2", projectDtos[1].Description);
            Assert.Equal("Completed", projectDtos[1].Status);
            Assert.Equal("Moderator", projectDtos[1].UserRoleInProject);
        }

        [Fact]
        public async Task GetUserProjectsAsync_WhenUserHasNoProjects_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            var emptyProjects = new List<Project>();

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(emptyProjects);

            // Act
            var result = await _projectService.GetUserProjectsAsync(userId);

            // Assert
            var projectDtos = result.ToList();
            Assert.NotNull(projectDtos);
            Assert.Empty(projectDtos);
        }

        [Fact]
        public async Task GetUserProjectsAsync_WhenProjectHasNoUserRole_ReturnsUnknownRole()
        {
            // Arrange
            int userId = 1;
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    Name = "Project Without Role",
                    Description = "Test Description",
                    Status = "Active",
                    ProjectUsers = new List<ProjectUser>() // Порожній список
                }
            };

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(projects);

            // Act
            var result = await _projectService.GetUserProjectsAsync(userId);

            // Assert
            var projectDtos = result.ToList();
            Assert.Single(projectDtos);
            Assert.Equal("Unknown", projectDtos[0].UserRoleInProject);
        }

        [Fact]
        public async Task GetUserProjectsAsync_WhenProjectHasNullProjectUsers_ReturnsUnknownRole()
        {
            // Arrange
            int userId = 1;
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    Name = "Project With Null Users",
                    Description = "Test Description",
                    Status = "Active",
                    ProjectUsers = null // Null колекція
                }
            };

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(projects);

            // Act & Assert - перевіряємо, що не викидається NullReferenceException
            var exception = await Record.ExceptionAsync(async () =>
            {
                var result = await _projectService.GetUserProjectsAsync(userId);
                var projectDtos = result.ToList();
            });

            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task GetUserProjectsAsync_WithMultipleRoles_ReturnsCorrectRole()
        {
            // Arrange
            int userId = 1;
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    Name = "Multi-User Project",
                    Description = "Project with multiple users",
                    Status = "Active",
                    ProjectUsers = new List<ProjectUser>
                    {
                        new ProjectUser { UserId = 1, Role = Role.Moderator },
                        new ProjectUser { UserId = 2, Role = Role.Member }
                    }
                }
            };

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(projects);

            // Act
            var result = await _projectService.GetUserProjectsAsync(userId);

            // Assert
            var projectDtos = result.ToList();
            Assert.Single(projectDtos);
            Assert.Equal("Moderator", projectDtos[0].UserRoleInProject);
        }

        [Fact]
        public async Task GetUserProjectsAsync_VerifiesRepositoryCalledOnce()
        {
            // Arrange
            int userId = 1;
            var projects = new List<Project>();

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(projects);

            // Act
            await _projectService.GetUserProjectsAsync(userId);

            // Assert
            _mockProjectRepository.Verify(
                repo => repo.GetProjectsByUserIdAsync(userId),
                Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(999)]
        [InlineData(0)]
        public async Task GetUserProjectsAsync_WithDifferentUserIds_CallsRepositoryWithCorrectId(int userId)
        {
            // Arrange
            var projects = new List<Project>();
            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(projects);

            // Act
            await _projectService.GetUserProjectsAsync(userId);

            // Assert
            _mockProjectRepository.Verify(
                repo => repo.GetProjectsByUserIdAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task GetUserProjectsAsync_MapsAllPropertiesCorrectly()
        {
            // Arrange
            int userId = 5;
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 100,
                    Name = "Test Project Name",
                    Description = "Test Project Description",
                    Status = "In Progress",
                    ProjectUsers = new List<ProjectUser>
                    {
                        new ProjectUser { UserId = 5, Role = Role.Member }
                    }
                }
            };

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(projects);

            // Act
            var result = await _projectService.GetUserProjectsAsync(userId);

            // Assert
            var dto = result.First();
            Assert.Equal(projects[0].Id, dto.Id);
            Assert.Equal(projects[0].Name, dto.Name);
            Assert.Equal(projects[0].Description, dto.Description);
            Assert.Equal(projects[0].Status, dto.Status);
            Assert.Equal(projects[0].ProjectUsers.First().Role.ToString(), dto.UserRoleInProject);
        }

        [Fact]
        public async Task GetUserProjectsAsync_WithMemberRole_ReturnsMember()
        {
            // Arrange
            int userId = 1;
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    Name = "Member Project",
                    Description = "User is a member",
                    Status = "Active",
                    ProjectUsers = new List<ProjectUser>
                    {
                        new ProjectUser { UserId = 1, Role = Role.Member }
                    }
                }
            };

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(projects);

            // Act
            var result = await _projectService.GetUserProjectsAsync(userId);

            // Assert
            var dto = result.First();
            Assert.Equal("Member", dto.UserRoleInProject);
        }

        [Fact]
        public async Task GetUserProjectsAsync_WithModeratorRole_ReturnsModerator()
        {
            // Arrange
            int userId = 2;
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 2,
                    Name = "Moderator Project",
                    Description = "User is a moderator",
                    Status = "Active",
                    ProjectUsers = new List<ProjectUser>
                    {
                        new ProjectUser { UserId = 2, Role = Role.Moderator }
                    }
                }
            };

            _mockProjectRepository
                .Setup(repo => repo.GetProjectsByUserIdAsync(userId))
                .ReturnsAsync(projects);

            // Act
            var result = await _projectService.GetUserProjectsAsync(userId);

            // Assert
            var dto = result.First();
            Assert.Equal("Moderator", dto.UserRoleInProject);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldReturnProject_WhenProjectExists()
        {
            // Arrange
            var projectId = 1;
            var expectedProject = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Desc",
                Status = "Active"
            };

            _mockProjectRepository
                .Setup(repo => repo.GetProjectByIdAsync(projectId))
                .ReturnsAsync(expectedProject);

            // Act
            var result = await _projectService.GetProjectByIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectId, result.Id);
            Assert.Equal(expectedProject.Name, result.Name);
            _mockProjectRepository.Verify(repo => repo.GetProjectByIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldReturnNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = 99;
            _mockProjectRepository
                .Setup(repo => repo.GetProjectByIdAsync(projectId))
                .ReturnsAsync((Project)null);

            // Act
            var result = await _projectService.GetProjectByIdAsync(projectId);

            // Assert
            Assert.Null(result);
            _mockProjectRepository.Verify(repo => repo.GetProjectByIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_ShouldUpdateAndReturnProject_WhenProjectExists()
        {
            // Arrange
            var projectId = 1;
            var originalProject = new Project
            {
                Id = projectId,
                Name = "Original Name",
                Description = "Original Desc",
                Status = "Active"
            };

            var newName = "Updated Name";
            var newDesc = "Updated Desc";
            var newStatus = "Completed";

            _mockProjectRepository
                .Setup(repo => repo.GetProjectByIdAsync(projectId))
                .ReturnsAsync(originalProject);

            _mockProjectRepository
                .Setup(repo => repo.UpdateProjectAsync(It.IsAny<Project>()))
                .ReturnsAsync((Project p) => p); 

            // Act
            var result = await _projectService.UpdateProjectAsync(projectId, newName, newDesc, newStatus);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectId, result.Id);
            Assert.Equal(newName, result.Name);
            Assert.Equal(newDesc, result.Description);
            Assert.Equal(newStatus, result.Status);

            _mockProjectRepository.Verify(repo => repo.GetProjectByIdAsync(projectId), Times.Once);
            _mockProjectRepository.Verify(repo => repo.UpdateProjectAsync(It.Is<Project>(p =>
                p.Id == projectId &&
                p.Name == newName &&
                p.Description == newDesc &&
                p.Status == newStatus
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_ShouldThrowException_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = 99;
            var newName = "Updated Name";
            var newDesc = "Updated Desc";
            var newStatus = "Completed";

            _mockProjectRepository
                .Setup(repo => repo.GetProjectByIdAsync(projectId))
                .ReturnsAsync((Project)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _projectService.UpdateProjectAsync(projectId, newName, newDesc, newStatus)
            );

            Assert.Equal($"Проєкт з ID {projectId} не знайдено.", ex.Message);

            _mockProjectRepository.Verify(repo => repo.GetProjectByIdAsync(projectId), Times.Once);
            _mockProjectRepository.Verify(repo => repo.UpdateProjectAsync(It.IsAny<Project>()), Times.Never);
        }
    }
}