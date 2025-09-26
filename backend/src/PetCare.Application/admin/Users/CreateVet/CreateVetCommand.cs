using PetCare.Application.Common.Interfaces;

namespace PetCare.Application.Admin.Users.CreateVet;

public sealed class CreateVetCommand
{
    private readonly IUserService _userService;

    public CreateVetCommand(IUserService userService)
    {
        _userService = userService;
    }

    // returns (ok, error, data)
    // errors: "validation_failed" | "email_in_use" | "user_creation_failed"
    public async Task<(bool ok, string? error, CreateVetResponse? data)> ExecuteAsync(
        CreateVetRequest request,
        CancellationToken ct = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant();
        var fullName = request.FullName?.Trim();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return (false, "validation_failed", null);
        }

        var userCreated = await _userService.CreateUserAsync(email, password, fullName, "VET");
        if (!userCreated)
        {
            return (false, "user_creation_failed", null);
        }

        return (true, null, new CreateVetResponse("user-id", email));
    }
}
