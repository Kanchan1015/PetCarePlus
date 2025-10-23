// File: tests/PetCare.Integration.Tests/Inventory/InventoryControllerIntegrationTests.cs
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;

namespace PetCare.Integration.Tests.Inventory
{
    // IMPORTANT:
    // - Ensure the Program class referenced below is the entry point of your API project.
    //   If your Program class namespace isn't PetCare.Api (or file name differs), update the generic parameter.
    //
    // - By default this uses the application startup as-is. If you want DB isolation for tests,
    //   see the "OPTIONAL: Swap DbContext to SQLite in-memory" block below and uncomment/adjust it.
    //
    public class InventoryControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public InventoryControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Using the factory as provided by the app's Program configuration.
            _factory = factory;
            _client = _factory.CreateClient(); // uses random localhost port, in-memory server
        }

        [Fact(DisplayName = "POST /api/inventory -> then GET /api/inventory/{id} returns created item")]
        public async Task CreateAndGet_ReturnsCreatedAndCanRetrieve()
        {
            // Arrange - create DTO payload (match your CreateInventoryDto)
            var createDto = new
            {
                Name = "Integration Test Item " + Guid.NewGuid().ToString("N").Substring(0, 6),
                Quantity = 3,
                Category = "TestCategory",
                Supplier = "IntegrationSupplier",
                Description = "Created by integration test"
                // ExpiryDate and PhotoUrl are optional
            };
            var content = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

            // Act - create
            var createResp = await _client.PostAsync("/api/inventory", content);

            // Assert - created
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdJson = await createResp.Content.ReadAsStringAsync();
            createdJson.Should().Contain(createDto.Name);

            // Try to parse returned object to extract id (returns CreatedAtAction with created object)
            dynamic createdObj = JsonConvert.DeserializeObject(createdJson)!;
            Guid createdId = Guid.Parse((string)createdObj.id.ToString());

            // Act - get by id
            var getResp = await _client.GetAsync($"/api/inventory/{createdId}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var getJson = await getResp.Content.ReadAsStringAsync();
            getJson.Should().Contain(createDto.Name);
        }

        [Fact(DisplayName = "GET /api/inventory returns list (200)")]
        public async Task GetAll_ReturnsList()
        {
            // Act
            var resp = await _client.GetAsync("/api/inventory");

            // Assert
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            // Response should be a JSON array (could be empty). Just assert it's valid JSON & returns 200
            json.Should().StartWith("[");
        }

        /*
        OPTIONAL: Swap DbContext to SQLite in-memory for isolation (recommended in CI)

        If you want the integration tests to run against an isolated in-memory SQLite DB (so tests
        don't touch your developer DB), use WithWebHostBuilder to replace the real DbContext
        registration. Below is a sketch of how to do it; uncomment and adapt to your project.

        Note: Replace 'YourDbContext' with the actual DbContext class (e.g., AppDbContext or
        PetCareDbContext). Also ensure you have Microsoft.Data.Sqlite and Microsoft.EntityFrameworkCore.Sqlite
        packages referenced in the test project.

        Example:
        public InventoryControllerIntegrationTests()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
            connection.Open();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // remove existing DbContext registrations (if any)
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<YourDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // add sqlite in-memory db for tests
                    services.AddDbContext<YourDbContext>(options =>
                    {
                        options.UseSqlite(connection);
                    });

                    // Build the service provider and run migrations / ensure created
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<YourDbContext>();
                    db.Database.EnsureCreated(); // or Migrate()
                });
            });

            _client = _factory.CreateClient();
        }

        If you choose this path, update using statements to include:
          using Microsoft.EntityFrameworkCore;
          using Microsoft.Extensions.DependencyInjection;
          using System.Linq;
        */
    }
}
