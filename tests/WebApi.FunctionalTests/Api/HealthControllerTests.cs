using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using GiveAID.Tests.Integration.Fixtures;

namespace GiveAID.Tests.Integration.Api;

[Collection("ApiTests")]
public class HealthControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthControllerTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Get_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task Health_Ready_ReturnsReady()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ready");
    }

    [Fact]
    public async Task Health_Live_ReturnsAlive()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("alive");
    }

    [Fact]
    public async Task Health_ContainsTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("timestamp");
    }

    [Fact]
    public async Task Health_ContainsVersion()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"version\":\"2.0\"");
    }
}
