using PetCare.Infrastructure.Auth;

namespace PetCare.Infrastructure.Jwt;

public interface IJwtTokenGenerator : PetCare.Application.Common.Interfaces.IJwtTokenGenerator
{
    (string token, DateTime expiresAtUtc) Create(ApplicationUser user, string role);
}
