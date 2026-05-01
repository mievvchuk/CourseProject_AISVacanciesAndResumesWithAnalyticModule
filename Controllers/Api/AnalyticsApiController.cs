using AisVacanciesAndResumes.Services;
using Microsoft.AspNetCore.Mvc;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Route("api/analytics")]
public class AnalyticsApiController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsApiController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var model = await _analyticsService.GetDashboardAsync(null, true);
        return Ok(new
        {
            model.VacancyCount,
            model.ResumeCount,
            model.ApplicationCount,
            model.CandidateCount,
            model.EmployerCount,
            model.AverageSalary,
            model.AverageMatchPercentage,
            model.ActiveVacancyCount,
            model.ClosedVacancyCount
        });
    }
}
