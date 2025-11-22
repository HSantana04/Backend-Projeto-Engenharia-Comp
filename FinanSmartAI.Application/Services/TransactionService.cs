using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinanSmartAI.Application.DTOs;
using FinanSmartAI.Domain.Entities;
using FinanSmartAI.Infrastructure.Data;

namespace FinanSmartAI.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ApplicationDbContext context,
        ILogger<TransactionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TransactionDto?> CreateTransactionAsync(
        Guid userId,
        CreateTransactionRequest request)
    {
        try
        {
            var amount = request.Type == "despesa"
                ? -Math.Abs(request.Amount)
                : Math.Abs(request.Amount);

            var transaction = new Transaction(
                userId,
                request.Title,
                amount,
                request.Category,
                request.Date
            );

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Transação {TransactionId} criada para o usuário {UserId}",
                transaction.Id,
                userId);

            return MapToDto(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar transação para o usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? category = null)
    {
        try
        {
            var query = _context.Transactions.Where(t => t.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category == category);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return transactions.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            return transaction == null ? null : MapToDto(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao buscar transação {TransactionId} do usuário {UserId}",
                id,
                userId);
            throw;
        }
    }

    public async Task<bool> UpdateTransactionAsync(
        Guid id,
        Guid userId,
        UpdateTransactionRequest request)
    {
        try
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
            {
                return false;
            }

            var amount = request.Type == "despesa"
                ? -Math.Abs(request.Amount)
                : Math.Abs(request.Amount);

            transaction.Description = request.Title;
            transaction.Amount = amount;
            transaction.Category = request.Category;
            transaction.Date = request.Date;
            transaction.UpdatedAt = DateTime.UtcNow;

            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Transação {TransactionId} atualizada", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar transação {TransactionId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteTransactionAsync(Guid id, Guid userId)
    {
        try
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
            {
                return false;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Transação {TransactionId} deletada", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar transação {TransactionId}", id);
            throw;
        }
    }

    public async Task<TransactionSummary> GetTransactionSummaryAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var query = _context.Transactions.Where(t => t.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            var transactions = await query.ToListAsync();

            var totalReceitas = transactions
                .Where(t => t.Amount > 0)
                .Sum(t => t.Amount);

            var totalDespesas = Math.Abs(transactions
                .Where(t => t.Amount < 0)
                .Sum(t => t.Amount));

            var saldo = totalReceitas - totalDespesas;

            return new TransactionSummary(totalReceitas, totalDespesas, saldo);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao buscar resumo de transações do usuário {UserId}",
                userId);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(Guid userId)
    {
        try
        {
            var categories = await _context.Transactions
                .Where(t => t.UserId == userId)
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias do usuário {UserId}", userId);
            throw;
        }
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id,
            transaction.Description,
            transaction.Amount,
            transaction.Category,
            transaction.Date,
            transaction.CreatedAt,
            transaction.UpdatedAt
        );
    }
}
