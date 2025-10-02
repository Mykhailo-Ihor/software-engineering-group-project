using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskForge.Domain.Entities;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [InverseProperty(nameof(ProjectUser.User))]
    public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();

    [InverseProperty(nameof(TaskUser.User))]
    public ICollection<TaskUser> TaskUsers { get; set; } = new List<TaskUser>();

    [InverseProperty(nameof(Password.User))]
    public ICollection<Password> Passwords { get; set; } = new List<Password>();

    [InverseProperty(nameof(Expense.User))]
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    [InverseProperty(nameof(Subscription.User))]
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}