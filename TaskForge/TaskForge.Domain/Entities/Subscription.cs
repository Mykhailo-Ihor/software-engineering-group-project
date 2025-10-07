using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskForge.Domain.Enums;
namespace TaskForge.Domain.Entities;

[Table("Subscriptions")]
public class Subscription
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; }

    [Required]
    public DateTime BillingDate { get; set; }

    [Required]
    public bool Notify { get; set; }

    [Required]
    public int IntervalValue { get; set; }

    [Required]
    public IntervalUnit IntervalUnit { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}