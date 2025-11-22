using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanSmartAI.Application.DTOs.Auth;
using FinanSmartAI.Application.Services;

namespace FinanSmartAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim?.Value ?? throw new InvalidOperationException("User ID not found"));
    }

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            var userId = GetUserId();
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar perfil do usuário");
            return StatusCode(500, new { message = "Erro ao buscar perfil" });
        }
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new { message = "Nome e sobrenome são obrigatórios" });
            }

            if (request.FirstName.Length < 2 || request.LastName.Length < 2)
            {
                return BadRequest(new { message = "Nome e sobrenome devem ter pelo menos 2 caracteres" });
            }

            var userId = GetUserId();
            var success = await _userService.UpdateUserAsync(userId, request.FirstName, request.LastName, request.Bio);

            if (!success)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            var updatedUser = await _userService.GetUserByIdAsync(userId);
            _logger.LogInformation("Perfil do usuário {UserId} atualizado", userId);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil do usuário");
            return StatusCode(500, new { message = "Erro ao atualizar perfil" });
        }
    }

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAccount()
    {
        try
        {
            var userId = GetUserId();
            var success = await _userService.DeleteUserAsync(userId);

            if (!success)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            _logger.LogInformation("Conta do usuário {UserId} foi deletada", userId);
            return Ok(new { message = "Conta deletada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar conta do usuário");
            return StatusCode(500, new { message = "Erro ao deletar conta" });
        }
    }
}

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Bio
);
