namespace GiveAID.Application.Features.Health;

/// <summary>
/// DTO for health check response.
/// </summary>
public class HealthCheckDto
{
    public string Status { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Components { get; set; } = new();
}
