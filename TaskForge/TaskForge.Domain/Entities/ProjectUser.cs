using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskForge.Domain.Enums;

namespace TaskForge.Domain.Entities;

[Table("ProjectUsers")]
public class ProjectUser
{

    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int ProjectId { get; set; }
    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; }

    [Required]
    public Role Role { get; set; } = Role.Member;

}