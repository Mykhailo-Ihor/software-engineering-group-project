using TaskForge.Domain.Enums;
namespace TaskForge.Application.DTOs;

public class ExpenceRecordDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTime Date { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
}