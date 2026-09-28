using GiveAID.Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GiveAID.Infrastructure.Services;

/// <summary>
/// Retrieves the current authenticated user's ID from the HTTP context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Extract user ID from the "sub" claim (standard JWT subject claim)
        // or fallback to NameIdentifier claim
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
    }
}
