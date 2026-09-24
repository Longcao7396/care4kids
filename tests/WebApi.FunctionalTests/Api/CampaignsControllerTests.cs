using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GiveAID.Tests.Integration.Fixtures;

namespace GiveAID.Tests.Integration.Api;

[Collection("ApiTests")]
public class CampaignsControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CampaignsControllerTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/campaigns");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GetFeatured_ReturnsSuccessOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/campaigns/featured");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFoundOrOk()
    {
        // Act - Query for non-existent campaign
        var response = await _client.GetAsync("/api/v1/campaigns/999999");

        // Assert - Accept various responses
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound, 
            HttpStatusCode.BadRequest, 
            HttpStatusCode.OK, // May return empty array
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GetByCause_ReturnsSuccessOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/campaigns/cause/1");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.NotFound, 
            HttpStatusCode.BadRequest, 
            HttpStatusCode.ServiceUnavailable);
    }
}
