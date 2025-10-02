using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskForge.Domain.Entities;

[Table("TaskUsers")]
public class TaskUser
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int TaskId { get; set; }
    [ForeignKey(nameof(TaskId))]
    public Task Task { get; set; }
}