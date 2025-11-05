using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskForge.Domain.Enums;

namespace TaskForge.Domain.Entities;

[Table("Passwords")]
public class Password
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(2083)]
    public string Url { get; set; }

    [Required]
    [MaxLength(100)]
    public string Login { get; set; }

    [Required]
    public string PasswordEncrypted { get; set; }

    [MaxLength(500)]
    public string Note { get; set; }

    public PasswordCategory Category { get; set; }
    [Required]
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}