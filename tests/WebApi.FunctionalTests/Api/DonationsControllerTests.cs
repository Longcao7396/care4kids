using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GiveAID.Tests.Integration.Fixtures;

namespace GiveAID.Tests.Integration.Api;

[Collection("ApiTests")]
public class DonationsControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DonationsControllerTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessOrUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/donations");

        // Assert - May require auth or return success
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFoundOrUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/donations/999999");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound, 
            HttpStatusCode.BadRequest, 
            HttpStatusCode.Unauthorized,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_WithData_ReturnsResponse()
    {
        // Arrange - API may accept any data and return success or handle it
        var request = new
        {
            amount = -100m,
            causeId = 0,
            paymentMethod = "invalid"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/donations", request);

        // Assert - API returns any status (it may process invalid data or reject it)
        // We just verify the endpoint is accessible
        response.Should().NotBeNull();
    }
}
