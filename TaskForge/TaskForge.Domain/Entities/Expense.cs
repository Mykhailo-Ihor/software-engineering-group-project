using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskForge.Domain.Enums;

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
    public Currency Currency { get; set; }

    [Required]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required]
    public DateTime Date { get; set; }

    public ExpenceCategory Category { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}