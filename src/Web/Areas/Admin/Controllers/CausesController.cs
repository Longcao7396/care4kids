using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GiveAID.Web.Areas.Admin.ViewModels;

namespace GiveAID.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CausesController : Controller
{
    private readonly Services.ApiClient _api;
    public CausesController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Causes";
        try { ViewData["Causes"] = await _api.GetAsync<object>("causes"); }
        catch { }
        return View();
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create Cause";
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CauseViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _api.PostAsync<CauseViewModel, object>("causes", model);
            TempData["Success"] = "Cause created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Cause";
        try
        {
            var cause = await _api.GetAsync<CauseViewModel>($"causes/{id}");
            if (cause == null) { TempData["Error"] = "Cause not found."; return RedirectToAction(); }
            return View(cause);
        }
        catch { TempData["Error"] = "Failed to load cause."; return RedirectToAction(); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CauseViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _api.PutAsync<CauseViewModel, object>($"causes/{id}", model);
            TempData["Success"] = "Cause updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _api.DeleteAsync($"causes/{id}"); TempData["Success"] = "Cause deleted."; }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
