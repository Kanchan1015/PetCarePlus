namespace PetCare.Application.Common.Interfaces;

public interface IUserService
{
    Task<bool> UserExistsAsync(string userId);
    Task<string?> GetUserFullNameAsync(string userId);
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<bool> CreateUserAsync(string email, string password, string fullName, string role);
    Task<bool> UpdateUserAsync(string userId, string fullName);
    Task<(bool Success, string? UserId, string? ErrorMessage)> ValidateUserCredentialsAsync(string email, string password);
}