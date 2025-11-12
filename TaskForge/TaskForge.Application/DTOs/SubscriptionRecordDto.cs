namespace TaskForge.Application.DTOs;

public class SubscriptionRecordDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime BillingDate { get; set; }
    public bool Notify { get; set; }
    public int IntervalValue { get; set; }
    public string IntervalUnit { get; set; }
}