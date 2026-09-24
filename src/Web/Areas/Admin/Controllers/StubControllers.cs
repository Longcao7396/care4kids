using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.Web.Areas.Admin.Controllers;

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class GalleryController : Controller
{
    private readonly Services.ApiClient _api;
    public GalleryController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Gallery";
        ViewData["Items"] = await _api.GetAsync<object>("gallery");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class CampaignReportsController : Controller
{
    private readonly Services.ApiClient _api;
    public CampaignReportsController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Campaign Reports";
        ViewData["Reports"] = await _api.GetAsync<object>("campaign-reports");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class OrganizationsController : Controller
{
    private readonly Services.ApiClient _api;
    public OrganizationsController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Organizations";
        ViewData["Items"] = await _api.GetAsync<object>("organizations");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class TeamController : Controller
{
    private readonly Services.ApiClient _api;
    public TeamController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Team";
        ViewData["Items"] = await _api.GetAsync<object>("team");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class AchievementsController : Controller
{
    private readonly Services.ApiClient _api;
    public AchievementsController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Achievements";
        ViewData["Items"] = await _api.GetAsync<object>("achievements");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class CareersController : Controller
{
    private readonly Services.ApiClient _api;
    public CareersController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Careers";
        ViewData["Items"] = await _api.GetAsync<object>("careers");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class FaqsController : Controller
{
    private readonly Services.ApiClient _api;
    public FaqsController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "FAQs";
        ViewData["Items"] = await _api.GetAsync<object>("faqs");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class CmsPagesAdminController : Controller
{
    private readonly Services.ApiClient _api;
    public CmsPagesAdminController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "CMS Pages";
        ViewData["Items"] = await _api.GetAsync<object>("cms-pages");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class EmailLogsController : Controller
{
    private readonly Services.ApiClient _api;
    public EmailLogsController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Email Logs";
        ViewData["Items"] = await _api.GetAsync<object>("email-logs");
        return View();
    }
}

/// <summary>
/// Legacy placeholder controller — delegates to API endpoints.
/// </summary>
[Area("Admin")]
[Authorize]
public class ConversationsController : Controller
{
    private readonly Services.ApiClient _api;
    public ConversationsController(Services.ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Conversations";
        ViewData["Items"] = await _api.GetAsync<object>("conversations");
        return View();
    }
}

/// <summary>
/// Settings controller — currently static, no API dependency.
/// </summary>
[Area("Admin")]
[Authorize]
public class SettingsController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Settings";
        return View();
    }
}
