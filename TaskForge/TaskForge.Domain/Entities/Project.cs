using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskForge.Domain.Entities;

[Table("Projects")]
public class Project
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [InverseProperty(nameof(ProjectUser.Project))]
    public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();

    [InverseProperty(nameof(Task.Project))]
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}