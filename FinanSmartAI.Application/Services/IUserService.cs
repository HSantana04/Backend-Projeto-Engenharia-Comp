using FinanSmartAI.Application.DTOs.Auth;

namespace FinanSmartAI.Application.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<bool> UpdateUserAsync(Guid id, string firstName, string lastName, string? bio);
    Task<bool> DeleteUserAsync(Guid id);
}
