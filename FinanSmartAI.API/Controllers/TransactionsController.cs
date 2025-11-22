using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FluentValidation;
using FinanSmartAI.Application.DTOs;
using FinanSmartAI.Application.Services;

namespace FinanSmartAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IValidator<CreateTransactionRequest> _createValidator;
    private readonly IValidator<UpdateTransactionRequest> _updateValidator;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionService transactionService,
        IValidator<CreateTransactionRequest> createValidator,
        IValidator<UpdateTransactionRequest> updateValidator,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim?.Value ?? throw new InvalidOperationException("User ID not found"));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransactionDto>> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dados de transação inválidos",
                    errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                });
            }

            var userId = GetUserId();
            var transaction = await _transactionService.CreateTransactionAsync(userId, request);

            if (transaction == null)
            {
                return BadRequest(new { message = "Erro ao criar transação" });
            }

            _logger.LogInformation("Transação criada pelo usuário {UserId}", userId);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar transação");
            return StatusCode(500, new { message = "Erro ao criar transação" });
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? category)
    {
        try
        {
            var userId = GetUserId();
            var transactions = await _transactionService.GetUserTransactionsAsync(userId, startDate, endDate, category);
            _logger.LogInformation("Transações recuperadas para o usuário {UserId}", userId);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações");
            return StatusCode(500, new { message = "Erro ao buscar transações" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransactionDto>> GetTransaction(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);

            if (transaction == null)
            {
                return NotFound(new { message = "Transação não encontrada" });
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transação {TransactionId}", id);
            return StatusCode(500, new { message = "Erro ao buscar transação" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransactionDto>> UpdateTransaction(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dados de transação inválidos",
                    errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                });
            }

            var userId = GetUserId();
            var success = await _transactionService.UpdateTransactionAsync(id, userId, request);

            if (!success)
            {
                return NotFound(new { message = "Transação não encontrada" });
            }

            var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);
            _logger.LogInformation("Transação {TransactionId} atualizada", id);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar transação {TransactionId}", id);
            return StatusCode(500, new { message = "Erro ao atualizar transação" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteTransaction(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var success = await _transactionService.DeleteTransactionAsync(id, userId);

            if (!success)
            {
                return NotFound(new { message = "Transação não encontrada" });
            }

            _logger.LogInformation("Transação {TransactionId} deletada", id);
            return Ok(new { message = "Transação deletada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar transação {TransactionId}", id);
            return StatusCode(500, new { message = "Erro ao deletar transação" });
        }
    }

    [HttpGet("summary/total")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransactionSummary>> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetUserId();
            var summary = await _transactionService.GetTransactionSummaryAsync(userId, startDate, endDate);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar resumo");
            return StatusCode(500, new { message = "Erro ao buscar resumo" });
        }
    }

    [HttpGet("categories/list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        try
        {
            var userId = GetUserId();
            var categories = await _transactionService.GetCategoriesAsync(userId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias");
            return StatusCode(500, new { message = "Erro ao buscar categorias" });
        }
    }
}
