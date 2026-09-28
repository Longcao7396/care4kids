using System.Net;
using System.Text.Json;
using FluentValidation;

namespace GiveAID.V2.WebApi.Middleware;

/// <summary>
/// Global exception handling middleware that returns JSON error responses
/// conforming to the API contract: { success: false, message: "...", data: null }
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized access",
                (Dictionary<string, string[]>?)null
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                exception.Message,
                (Dictionary<string, string[]>?)null
            ),
            // M-04: Duplicate resource → 409 Conflict
            Domain.Exceptions.DuplicateResourceException => (
                HttpStatusCode.Conflict,
                exception.Message,
                (Dictionary<string, string[]>?)null
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                exception.Message,
                (Dictionary<string, string[]>?)null
            ),
            ArgumentException => (
                HttpStatusCode.BadRequest,
                exception.Message,
                (Dictionary<string, string[]>?)null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                _env.IsDevelopment() ? exception.Message : "An unexpected error occurred",
                (Dictionary<string, string[]>?)null
            )
        };

        // Log the full exception server-side
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
