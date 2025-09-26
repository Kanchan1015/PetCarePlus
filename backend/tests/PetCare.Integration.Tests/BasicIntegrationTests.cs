using System.Net;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using FluentAssertions;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Integration.Tests;

/// <summary>
/// Simple integration tests that focus on basic API functionality.
/// These tests use in-memory database and bypass complex authentication.
/// </summary>
public class BasicIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BasicIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PetCareDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<PetCareDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetPets_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/pets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApiHealth_ShouldReturnOk()
    {
        // Act - Test a public endpoint if available, or just test that the server starts
        var response = await _client.GetAsync("/");

        // Assert - 404 is expected since there's no root endpoint, but it means the server is running
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPets_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

        // Act
        var response = await _client.GetAsync("/api/pets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}