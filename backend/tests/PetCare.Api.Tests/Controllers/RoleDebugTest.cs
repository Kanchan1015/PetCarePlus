using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;
using Xunit.Abstractions;

namespace PetCare.Api.Tests.Controllers
{
    public class RoleDebugTest
    {
        private readonly ITestOutputHelper _output;

        public RoleDebugTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestRoleChecking()
        {
            // Arrange - Setup admin claims like the test does
            var claims = new List<Claim>
            {
                new("sub", "admin-user"),
                new(ClaimTypes.Role, "ADMIN") // Uppercase like the fix
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Act & Assert - Test different role checking approaches
            var isAdminUppercase = principal.IsInRole("ADMIN");
            var isAdminCapitalized = principal.IsInRole("Admin");
            var isVetUppercase = principal.IsInRole("VET");

            _output.WriteLine($"User has ADMIN role (uppercase): {isAdminUppercase}");
            _output.WriteLine($"User has Admin role (capitalized): {isAdminCapitalized}");
            _output.WriteLine($"User has VET role (uppercase): {isVetUppercase}");

            // Debug: Print all claims
            _output.WriteLine("All claims:");
            foreach (var claim in principal.Claims)
            {
                _output.WriteLine($"  Type: {claim.Type}, Value: {claim.Value}");
            }

            // The key test: IsInRole with the exact values used in the controller
            Assert.True(isAdminUppercase, "Should recognize ADMIN role");
            Assert.False(isVetUppercase, "Should NOT recognize VET role for admin user");
        }
    }
}