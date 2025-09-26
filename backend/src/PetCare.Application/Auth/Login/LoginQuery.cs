using PetCare.Domain.Users;                       // AccountStatus
using PetCare.Application.Common.Interfaces;      // IJwtTokenGenerator, IUserService

namespace PetCare.Application.Auth.Login;

public sealed class LoginQuery
{
    private readonly IUserService _userService;
    private readonly IJwtTokenGenerator _tokens;

    public LoginQuery(IUserService userService, IJwtTokenGenerator tokens)
    {
        _userService = userService;
        _tokens = tokens;
    }

    public async Task<(bool ok, string? error, LoginResponse? data)> ExecuteAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        // 1) Validate user credentials
        var (success, userId, errorMessage) = await _userService.ValidateUserCredentialsAsync(request.Email, request.Password);
        if (!success || string.IsNullOrEmpty(userId))
            return (false, errorMessage ?? "invalid_credentials", null);

        // 2) Get user details
        var fullName = await _userService.GetUserFullNameAsync(userId);
        var roles = await _userService.GetUserRolesAsync(userId);
        var role = roles.FirstOrDefault() ?? "Owner";

        // 3) Generate token
        var token = _tokens.GenerateToken(userId, request.Email, fullName ?? "", roles);

        var response = new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddHours(2), // Set appropriate expiry
            User = new LoginResponse.UserDto
            {
                Id = userId,
                Role = role,
                FullName = fullName ?? "",
                Email = request.Email
            }
        };

        return (true, null, response);
    }
}
