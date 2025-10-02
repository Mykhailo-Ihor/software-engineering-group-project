using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskForge.Domain.Entities;

[Table("Tasks")]
public class Task
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public bool IsCompleted { get; set; }

    [InverseProperty(nameof(TaskUser.Task))]
    public ICollection<TaskUser> TaskUsers { get; set; } = new List<TaskUser>();
    public int ProjectId { get; set; }
    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; }
}