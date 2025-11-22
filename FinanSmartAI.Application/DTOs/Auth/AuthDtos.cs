namespace FinanSmartAI.Application.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password
);

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User
);

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Bio,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record RefreshTokenRequest(
    string RefreshToken
);
