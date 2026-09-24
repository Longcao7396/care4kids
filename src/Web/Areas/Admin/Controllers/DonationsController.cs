using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GiveAID.Web.Areas.Admin.ViewModels;

namespace GiveAID.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DonationsController : Controller
{
    private readonly Services.ApiClient _api;

    public DonationsController(Services.ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(string? status = null, int page = 1)
    {
        ViewData["Title"] = "Donations";

        try
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["page"] = page.ToString(),
                ["pageSize"] = "20"
            };

            var result = await _api.GetAsync<object>("donations", queryParams);
            ViewData["Donations"] = result;
        }
        catch
        {
        }

        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Donation Details";

        try
        {
            var donation = await _api.GetAsync<DonationViewModel>($"donations/{id}");
            ViewData["Donation"] = donation;
        }
        catch
        {
        }

        return View();
    }
}
