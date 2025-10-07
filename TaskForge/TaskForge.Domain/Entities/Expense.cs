using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskForge.Domain.Entities;

[Table("Expenses")]
public class Expense
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(50)]
    public string Category { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}