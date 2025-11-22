using FinanSmartAI.Application.DTOs.Auth;

namespace FinanSmartAI.Application.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken();
    bool ValidatePassword(string password, string hash);
    string HashPassword(string password);
}
