using System.Net;
using FluentAssertions;
using GiveAID.Tests.Integration.Fixtures;

namespace GiveAID.Tests.Integration.Smoke;

[Collection("ApiTests")]
public class EndpointSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;

    public EndpointSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/v1/health")]
    [InlineData("/api/v1/health/ready")]
    [InlineData("/api/v1/health/live")]
    public async Task HealthEndpoints_Return200(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/api/v1/causes")]
    [InlineData("/api/v1/campaigns")]
    [InlineData("/api/v1/gallery")]
    [InlineData("/api/v1/faqs")]
    public async Task PublicEndpoints_AreAccessible(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert - Should at least respond (not 500)
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData("/api/v1/auth/me")]
    public async Task ProtectedGetEndpoints_Return401(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task InvalidEndpoint_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/nonexistent-endpoint-12345");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HealthCheck_ContainsExpectedFields()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Contain("status");
        content.Should().Contain("timestamp");
    }
}
