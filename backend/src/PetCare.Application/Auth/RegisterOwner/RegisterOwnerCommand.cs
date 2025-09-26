using PetCare.Domain.Pets;
using PetCare.Domain.Users;
using PetCare.Application.Common.Interfaces;

namespace PetCare.Application.Auth.RegisterOwner;

public sealed class RegisterOwnerCommand
{
    private readonly IUserService _userService;
    private readonly IPetRepository _petRepository;

    public RegisterOwnerCommand(
        IUserService userService,
        IPetRepository petRepository)
    {
        _userService = userService;
        _petRepository = petRepository;
    }

    public async Task<(bool ok, string? error)> ExecuteAsync(RegisterOwnerRequest request, CancellationToken ct = default)
    {
        // 1) Create user with Owner role
        var userCreated = await _userService.CreateUserAsync(request.Email, request.Password, request.FullName, "Owner");
        if (!userCreated)
            return (false, "user_creation_failed");

        // Note: Pet creation during registration is not part of the core Pet Profile Management feature
        // The admin will create and assign pets separately using the Pet Management API

        return (true, null);
    }
}
