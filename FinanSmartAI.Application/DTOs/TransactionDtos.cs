namespace FinanSmartAI.Application.DTOs;

public record CreateTransactionRequest(
    string Type,           // "receita" ou "despesa"
    string Title,
    decimal Amount,
    string Category,
    DateTime Date
);

public record UpdateTransactionRequest(
    string Type,
    string Title,
    decimal Amount,
    string Category,
    DateTime Date
);

public record TransactionDto(
    Guid Id,
    string Description,
    decimal Amount,
    string Category,
    DateTime Date,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record TransactionSummary(
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo
);

public record TransactionListResponse(
    IEnumerable<TransactionDto> Transactions,
    TransactionSummary Summary,
    int Total
);
