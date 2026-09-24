using System.Net.Http.Headers;

namespace GiveAID.Web.Services;

/// <summary>
/// Stores the currently logged-in admin's info in session.
/// </summary>
public class AdminSession
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get => _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
        set
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (value.HasValue) session?.SetInt32("UserId", value.Value);
            else session?.Remove("UserId");
        }
    }

    public string? Email
    {
        get => _httpContextAccessor.HttpContext?.Session.GetString("Email");
        set
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (!string.IsNullOrEmpty(value)) session?.SetString("Email", value);
            else session?.Remove("Email");
        }
    }

    public string? FullName
    {
        get => _httpContextAccessor.HttpContext?.Session.GetString("FullName");
        set
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (!string.IsNullOrEmpty(value)) session?.SetString("FullName", value);
            else session?.Remove("FullName");
        }
    }

    public string? Role
    {
        get => _httpContextAccessor.HttpContext?.Session.GetString("Role");
        set
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (!string.IsNullOrEmpty(value)) session?.SetString("Role", value);
            else session?.Remove("Role");
        }
    }

    public string? Token
    {
        get => _httpContextAccessor.HttpContext?.Session.GetString("Token");
        set
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (!string.IsNullOrEmpty(value)) session?.SetString("Token", value);
            else session?.Remove("Token");
        }
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public void Clear()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Clear();
    }
}
