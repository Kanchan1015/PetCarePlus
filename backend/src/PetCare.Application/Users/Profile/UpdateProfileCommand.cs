using PetCare.Application.Common.Interfaces;

namespace PetCare.Application.Users.Profile;

public sealed class UpdateProfileCommand
{
    private readonly IUserService _userService;

    public UpdateProfileCommand(IUserService userService)
    {
        _userService = userService;
    }

    // userId comes from JWT (ClaimTypes.NameIdentifier) in the controller
    public async Task<(bool ok, string? error, UpdateProfileResponse? data)> ExecuteAsync(
        string userId,
        UpdateProfileRequest req,
        CancellationToken ct = default)
    {
        var userExists = await _userService.UserExistsAsync(userId);
        if (!userExists)
            return (false, "not_found", null);

        var updated = await _userService.UpdateUserAsync(userId, req.FullName);
        if (!updated)
            return (false, "update_failed", null);

        var fullName = await _userService.GetUserFullNameAsync(userId);
        var roles = await _userService.GetUserRolesAsync(userId);
        
        return (true, null, new UpdateProfileResponse
        {
            Id = userId,
            FullName = fullName ?? "",
            Email = "", // Will be populated by controller from claims
            PhoneNumber = null, // Phone number update not implemented in service yet
            Role = roles.FirstOrDefault() ?? "Owner"
        });
    }
}
