using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GiveAID.Web.Areas.Admin.ViewModels;

namespace GiveAID.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthController : Controller
{
    private readonly Services.ApiClient _api;
    private readonly Services.AdminSession _session;

    public AuthController(Services.ApiClient api, Services.AdminSession session)
    {
        _api = api;
        _session = session;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_session.IsAuthenticated)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Username"] = model.Username;
            return View(model);
        }

        try
        {
            // Sign in with username (email is no longer accepted as a login identifier).
            var request = new { Username = model.Username, Password = model.Password };
            var result = await _api.PostAnonymousAsync<object, Services.LoginResponse>("auth/login", request);

            if (result == null)
            {
                TempData["Error"] = "Invalid response from server.";
                ViewData["Username"] = model.Username;
                return View(model);
            }

            // Store session data
            _session.Token = result.Token;
            _session.UserId = result.UserId;
            _session.Email = result.Email;
            _session.FullName = result.FullName;
            _session.Role = result.Role;

            // Create cookie identity
            var claims = new List<System.Security.Claims.Claim>
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new(System.Security.Claims.ClaimTypes.Email, result.Email),
                new(System.Security.Claims.ClaimTypes.Name, result.FullName),
                new(System.Security.Claims.ClaimTypes.Role, result.Role),
                new("Token", result.Token)
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, "AdminCookie");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("AdminCookie", principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"Cannot connect to API: {ex.Message}";
            ViewData["Username"] = model.Username;
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            ViewData["Username"] = model.Username;
            return View(model);
        }
        catch (Exception)
        {
            TempData["Error"] = "Login failed. Please try again.";
            ViewData["Username"] = model.Username;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        _session.Clear();
        await HttpContext.SignOutAsync("AdminCookie");
        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
