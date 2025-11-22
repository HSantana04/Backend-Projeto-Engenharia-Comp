using Microsoft.AspNetCore.Mvc;
using FinanSmartAI.Application.DTOs.Auth;
using FinanSmartAI.Application.Services;

namespace FinanSmartAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // ========== VALIDAÇÃO BÁSICA ==========
            if (request == null)
            {
                return BadRequest(new { message = "Dados da requisição são obrigatórios" });
            }

            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
            {
                _logger.LogWarning("Tentativa de registro com campos faltando");
                return BadRequest(new { message = "Todos os campos são obrigatórios" });
            }

            if (request.Password != request.ConfirmPassword)
            {
                _logger.LogWarning("Tentativa de registro com senhas não conferindo");
                return BadRequest(new { message = "As senhas não conferem" });
            }

            // ========== CHAMAR SERVIÇO ==========
            var response = await _authService.RegisterAsync(request);

            if (response == null)
            {
                _logger.LogWarning("Falha ao registrar usuário: {Email}", request.Email);
                return BadRequest(new { message = "Email já existe ou erro ao registrar" });
            }

            _logger.LogInformation("Novo usuário registrado: {Email}", request.Email);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário: {Email}", request?.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao registrar usuário", detail = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // ========== VALIDAÇÃO BÁSICA ==========
            if (request == null)
            {
                return BadRequest(new { message = "Email e senha são obrigatórios" });
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Tentativa de login com campos faltando");
                return BadRequest(new { message = "Email e senha são obrigatórios" });
            }

            // ========== CHAMAR SERVIÇO ==========
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                _logger.LogWarning("Login falhou para email: {Email}", request.Email);
                return Unauthorized(new { message = "Email ou senha inválidos" });
            }

            _logger.LogInformation("Login bem-sucedido para email: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login: {Email}", request?.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao fazer login", detail = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // ========== VALIDAÇÃO BÁSICA ==========
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Tentativa de refresh sem token");
                return BadRequest(new { message = "Refresh token é obrigatório" });
            }

            // ========== CHAMAR SERVIÇO ==========
            var response = await _authService.RefreshTokenAsync(request);

            if (response == null)
            {
                _logger.LogWarning("Falha ao renovar token");
                return BadRequest(new { message = "Token inválido ou expirado" });
            }

            _logger.LogInformation("Token renovado com sucesso");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao renovar token", detail = ex.Message });
        }
    }
}