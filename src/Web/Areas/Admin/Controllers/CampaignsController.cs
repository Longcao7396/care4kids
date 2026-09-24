using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GiveAID.Web.Areas.Admin.ViewModels;

namespace GiveAID.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CampaignsController : Controller
{
    private readonly Services.ApiClient _api;

    public CampaignsController(Services.ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(string? status = null, int page = 1)
    {
        ViewData["Title"] = "Campaigns";

        try
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["page"] = page.ToString(),
                ["pageSize"] = "20",
                ["status"] = status
            };

            var result = await _api.GetAsync<object>($"campaigns", queryParams);
            ViewData["Campaigns"] = result;
        }
        catch
        {
            // API not implemented — pass null
        }

        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Campaign Details";

        try
        {
            var campaign = await _api.GetAsync<object>($"campaigns/{id}");
            ViewData["Campaign"] = campaign;
        }
        catch
        {
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create Campaign";

        try
        {
            var causes = await _api.GetAsync<object>("causes?activeOnly=true");
            ViewData["Causes"] = causes;
        }
        catch { }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CampaignViewModel model)
    {
        if (!ModelState.IsValid)
        {
            try
            {
                var causes = await _api.GetAsync<object>("causes?activeOnly=true");
                ViewData["Causes"] = causes;
            }
            catch { }
            return View(model);
        }

        try
        {
            var result = await _api.PostAsync<CampaignViewModel, object>("campaigns", model);
            TempData["Success"] = "Campaign created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to create campaign: {ex.Message}";
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Campaign";

        try
        {
            var campaign = await _api.GetAsync<CampaignViewModel>($"campaigns/{id}");
            var causes = await _api.GetAsync<object>("causes?activeOnly=true");
            ViewData["Causes"] = causes;

            if (campaign == null)
            {
                TempData["Error"] = "Campaign not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(campaign);
        }
        catch
        {
            TempData["Error"] = "Failed to load campaign.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CampaignViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _api.PutAsync<CampaignViewModel, object>($"campaigns/{id}", model);
            TempData["Success"] = "Campaign updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to update campaign: {ex.Message}";
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _api.DeleteAsync($"campaigns/{id}");
            if (success)
                TempData["Success"] = "Campaign deleted successfully.";
            else
                TempData["Error"] = "Failed to delete campaign.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete campaign: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
