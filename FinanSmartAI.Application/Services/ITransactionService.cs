using FinanSmartAI.Application.DTOs;

namespace FinanSmartAI.Application.Services;

public interface ITransactionService
{
    Task<TransactionDto?> CreateTransactionAsync(Guid userId, CreateTransactionRequest request);
    Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? category = null);
    Task<TransactionDto?> GetTransactionByIdAsync(Guid id, Guid userId);
    Task<bool> UpdateTransactionAsync(Guid id, Guid userId, UpdateTransactionRequest request);
    Task<bool> DeleteTransactionAsync(Guid id, Guid userId);
    Task<TransactionSummary> GetTransactionSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<string>> GetCategoriesAsync(Guid userId);
}
