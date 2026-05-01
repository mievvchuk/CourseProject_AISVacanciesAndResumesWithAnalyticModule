using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

public class ExportController : Controller
{
    private readonly IVacancyService _vacancyService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IExportService _exportService;

    public ExportController(
        IVacancyService vacancyService,
        IAnalyticsService analyticsService,
        IExportService exportService)
    {
        _vacancyService = vacancyService;
        _analyticsService = analyticsService;
        _exportService = exportService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Vacancies([FromQuery] VacancyFilterViewModel filter)
    {
        var model = await _vacancyService.SearchAsync(filter);
        var fileBytes = _exportService.GenerateVacanciesCsv(model.Items);
        var fileName = $"vacancies-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(fileBytes, "text/csv", fileName);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Employer")]
    public async Task<IActionResult> Analytics()
    {
        var model = await _analyticsService.GetDashboardAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole("Admin"));

        var fileBytes = _exportService.GenerateAnalyticsCsv(model);
        var fileName = $"analytics-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(fileBytes, "text/csv", fileName);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VacanciesPdf([FromQuery] VacancyFilterViewModel filter)
    {
        var model = await _vacancyService.SearchAsync(filter);
        var fileBytes = _exportService.GenerateVacanciesPdf(model.Items);
        var fileName = $"vacancies-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";

        return File(fileBytes, "application/pdf", fileName);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Employer")]
    public async Task<IActionResult> AnalyticsPdf()
    {
        var model = await _analyticsService.GetDashboardAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole("Admin"));

        var fileBytes = _exportService.GenerateAnalyticsPdf(model);
        var fileName = $"analytics-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";

        return File(fileBytes, "application/pdf", fileName);
    }
}
