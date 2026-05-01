using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Resumes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AisVacanciesAndResumes.Controllers;

[Authorize(Roles = "Employer,Admin")]
public class EmployerCandidatesController : Controller
{
    private readonly IResumeService _resumeService;

    public EmployerCandidatesController(IResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ResumeSearchFilterViewModel filter)
    {
        var model = await _resumeService.SearchPublishedResumesAsync(filter);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _resumeService.GetEmployerCandidateDetailsModelAsync(id);
        return model is null ? NotFound() : View(model);
    }
}
