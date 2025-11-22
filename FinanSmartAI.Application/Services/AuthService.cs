using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FinanSmartAI.Application.DTOs.Auth;
using FinanSmartAI.Domain.Entities;
using FinanSmartAI.Infrastructure.Data;
using BCrypt.Net;

namespace FinanSmartAI.Application.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    // ------------------------------
    // LOGIN
    // ------------------------------
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !ValidatePassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Tentativa de login falhou para email: {Email}", request.Email);
                return null;
            }

            var accessToken = GenerateAccessToken(user.Id, user.Email);
            var refreshToken = GenerateRefreshToken();

            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new LoginResponse(
                accessToken,
                refreshToken,
                new UserDto(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Bio,
                    user.IsActive,
                    user.CreatedAt,
                    user.UpdatedAt
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login");
            throw;
        }
    }

    // ------------------------------
    // REGISTER
    // ------------------------------
    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (exists)
            {
                _logger.LogWarning("Tentativa de registrar com email existente: {Email}", request.Email);
                return null;
            }

            if (request.Password != request.ConfirmPassword)
            {
                _logger.LogWarning("Senhas não conferem: {Email}", request.Email);
                return null;
            }

            var user = new User(request.FirstName, request.LastName, request.Email)
            {
                PasswordHash = HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var accessToken = GenerateAccessToken(user.Id, user.Email);
            var refreshToken = GenerateRefreshToken();

            return new LoginResponse(
                accessToken,
                refreshToken,
                new UserDto(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Bio,
                    user.IsActive,
                    user.CreatedAt,
                    user.UpdatedAt
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário");
            throw;
        }
    }

    // ------------------------------
    // REFRESH TOKEN
    // ------------------------------
    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            // Obs: você ainda precisa implementar refresh token real
            var newRefreshToken = GenerateRefreshToken();
            var newAccessToken = GenerateAccessToken(Guid.NewGuid(), "user@example.com");

            return new LoginResponse(newAccessToken, newRefreshToken, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            throw;
        }
    }

    // ------------------------------
    // TOKEN
    // ------------------------------
    public string GenerateAccessToken(Guid userId, string email)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:SecretKey"] ?? "your-secret-key-here-minimum-32-characters!!!"
            )
        );

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "FinanSmartAI",
            audience: _configuration["Jwt:Audience"] ?? "FinanSmartAIUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    // ------------------------------
    // PASSWORD (BCrypt CORRETO)
    // ------------------------------
    public string HashPassword(string password)
    {
        // BCrypt.Net-Next → jeito certo
        return BCrypt.Net.BCrypt.HashPassword(password);

    }

    public bool ValidatePassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);

    }
}
