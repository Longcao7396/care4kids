using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GiveAID.Web.Areas.Admin.ViewModels;

namespace GiveAID.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class UsersController : Controller
{
    private readonly Services.ApiClient _api;
    public UsersController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Users";
        // Let exceptions propagate to global handler; view handles null gracefully
        ViewData["Users"] = await _api.GetAsync<object>("users");
        return View();
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create User";
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Only catch HTTP-level errors; let unexpected exceptions propagate
        try
        {
            await _api.PostAsync<CreateUserViewModel, object>("auth/register", model);
            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Failed to communicate with the server. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit User";
        // Let exceptions propagate; global handler returns a 500 page
        var user = await _api.GetAsync<UserViewModel>($"users/{id}");
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction();
        }
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Only catch HTTP-level errors; let unexpected exceptions propagate
        try
        {
            await _api.PutAsync<UserViewModel, object>($"users/{id}", model);
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Failed to communicate with the server. Please try again.";
            return View(model);
        }
    }
}
