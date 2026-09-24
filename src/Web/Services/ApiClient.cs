using System.Net.Http.Headers;

namespace GiveAID.Web.Services;

/// <summary>
/// Wrapper around HttpClient that calls the GiveAID WebApi.
/// All controllers inject this instead of using HttpClient directly.
/// </summary>
public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AdminSession _session;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        AdminSession session,
        ILogger<ApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _session = session;
        _logger = logger;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("GiveAIDApi");
        if (!string.IsNullOrEmpty(_session.Token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _session.Token);
        }
        return client;
    }

    private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response, string action)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API error {StatusCode} during {Action}: {Body}",
                response.StatusCode, action, json);
            throw new HttpRequestException($"API call failed: {response.StatusCode}");
        }

        try
        {
            var envelope = System.Text.Json.JsonSerializer.Deserialize<ApiEnvelope<T>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (envelope == null)
                throw new InvalidOperationException("Empty API response");

            if (!envelope.Success)
                throw new InvalidOperationException(envelope.Message ?? "API returned failure");

            return envelope.Data;
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse API response for {Action}: {Body}", action, json);
            throw;
        }
    }

    /// <summary>GET /api/v1/{endpoint}</summary>
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"/api/v1/{endpoint.TrimStart('/')}");
        return await HandleResponseAsync<T>(response, $"GET {endpoint}");
    }

    /// <summary>GET /api/v1/{endpoint}?{query}</summary>
    public async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string?> queryParams)
    {
        var query = string.Join("&", queryParams
            .Where(kv => kv.Value != null)
            .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));
        return await GetAsync<T>($"{endpoint}?{query}");
    }

    /// <summary>POST /api/v1/{endpoint}</summary>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
    {
        using var client = CreateClient();
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/v1/{endpoint.TrimStart('/')}", content);
        return await HandleResponseAsync<TResponse>(response, $"POST {endpoint}");
    }

    /// <summary>POST /api/v1/{endpoint} with anonymous (no JWT)</summary>
    public async Task<TResponse?> PostAnonymousAsync<TRequest, TResponse>(string endpoint, TRequest body)
    {
        var handler = _httpClientFactory.CreateClient("GiveAIDApi");
        handler.DefaultRequestHeaders.Authorization = null;
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await handler.PostAsync($"/api/v1/{endpoint.TrimStart('/')}", content);
        return await HandleResponseAsync<TResponse>(response, $"POST {endpoint}");
    }

    /// <summary>PUT /api/v1/{endpoint}</summary>
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body)
    {
        using var client = CreateClient();
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"/api/v1/{endpoint.TrimStart('/')}", content);
        return await HandleResponseAsync<TResponse>(response, $"PUT {endpoint}");
    }

    /// <summary>DELETE /api/v1/{endpoint}</summary>
    public async Task<bool> DeleteAsync(string endpoint)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"/api/v1/{endpoint.TrimStart('/')}");
        return response.IsSuccessStatusCode;
    }
}
