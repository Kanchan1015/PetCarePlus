using Microsoft.AspNetCore.Identity;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Users;
using PetCare.Infrastructure.Auth;

namespace PetCare.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null;
    }

    public async Task<string?> GetUserFullNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.FullName;
    }

    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();
        
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> CreateUserAsync(string email, string password, string fullName, string role)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null) return false;

            // Create user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                AccountStatus = AccountStatus.Active
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded) return false;

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            // Assign role
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            return roleResult.Succeeded;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(string userId, string fullName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.FullName = fullName;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, string? UserId, string? ErrorMessage)> ValidateUserCredentialsAsync(string email, string password)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, null, "invalid_credentials");

            // Check account status
            if (user.AccountStatus != AccountStatus.Active)
                return (false, null, "inactive");

            // Verify password
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
                return (false, null, "invalid_credentials");

            return (true, user.Id, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }
}