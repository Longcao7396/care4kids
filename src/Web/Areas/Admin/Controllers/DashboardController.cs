using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DashboardController : Controller
{
    private readonly Services.ApiClient _api;

    public DashboardController(Services.ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";

        try
        {
            // Try to load dashboard data from API
            var stats = await _api.GetAsync<object>("admin/stats");
            var recentDonations = await _api.GetAsync<object>("admin/recent-donations?count=10");
            var overview = await _api.GetAsync<object>("statistics/overview");

            // If API returns null (not implemented yet), use empty model
            ViewData["Stats"] = stats;
            ViewData["RecentDonations"] = recentDonations;
            ViewData["Overview"] = overview;
        }
        catch
        {
            // API not available or not implemented yet — show empty state
        }

        return View();
    }
}
