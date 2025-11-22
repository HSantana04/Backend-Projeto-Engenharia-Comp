namespace FinanSmartAI.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Transaction() { }

    public Transaction(Guid userId, string description, decimal amount, string category, DateTime date)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Description = description;
        Amount = amount;
        Category = category;
        Date = date;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
