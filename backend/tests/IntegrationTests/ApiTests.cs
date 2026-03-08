using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests
{
    public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            // Arrange
            var loginData = new { Email = "testuser@test.com", Password = "testpassword" };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginData = new { Email = "invalid@nowhere.com", Password = "wrongpassword" };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Additional tests for other API endpoints can be added here
    }
}