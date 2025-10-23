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
    }
}
