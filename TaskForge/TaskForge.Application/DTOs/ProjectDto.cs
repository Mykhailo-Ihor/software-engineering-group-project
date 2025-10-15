namespace TaskForge.Application.DTOs;

/// <summary>
/// A Data Transfer Object representing a project for list views.
/// </summary>
public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string UserRoleInProject { get; set; }
}