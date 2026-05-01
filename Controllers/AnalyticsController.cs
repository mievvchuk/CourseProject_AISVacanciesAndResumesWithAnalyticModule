using AisVacanciesAndResumes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

[Authorize(Roles = "Admin,Employer")]
public class AnalyticsController : Controller
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        var model = await _analyticsService.GetDashboardAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole("Admin"));
        return View(model);
    }
}
