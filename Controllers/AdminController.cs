using AisVacanciesAndResumes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IAnalyticsService _analyticsService;

    public AdminController(IAdminService adminService, IAnalyticsService analyticsService)
    {
        _adminService = adminService;
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var model = await _adminService.GetDashboardAsync();
        model.Analytics = await _analyticsService.GetDashboardAsync(GetUserId(), isAdmin: true);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var model = await _adminService.GetUsersAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Vacancies()
    {
        var model = await _adminService.GetVacanciesAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveVacancy(int id, string? comment)
    {
        try
        {
            await _adminService.ApproveVacancyAsync(GetUserId(), id, comment);
            TempData["StatusMessage"] = "Вакансію схвалено.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Vacancies));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectVacancy(int id, string comment)
    {
        try
        {
            await _adminService.RejectVacancyAsync(GetUserId(), id, comment);
            TempData["StatusMessage"] = "Вакансію відхилено, роботодавцю надіслано сповіщення.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Vacancies));
    }

    [HttpGet]
    public async Task<IActionResult> Resumes()
    {
        var model = await _adminService.GetResumesAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveResume(int id, string? comment)
    {
        try
        {
            await _adminService.ApproveResumeAsync(GetUserId(), id, comment);
            TempData["StatusMessage"] = "Резюме схвалено.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Resumes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectResume(int id, string comment)
    {
        try
        {
            await _adminService.RejectResumeAsync(GetUserId(), id, comment);
            TempData["StatusMessage"] = "Резюме відхилено, кандидату надіслано сповіщення.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Resumes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateUser(string id, string? comment)
    {
        try
        {
            await _adminService.ActivateUserAsync(GetUserId(), id, comment);
            TempData["StatusMessage"] = "Користувача активовано.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateUser(string id, string? comment)
    {
        try
        {
            await _adminService.DeactivateUserAsync(GetUserId(), id, comment);
            TempData["StatusMessage"] = "Користувача деактивовано.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> ModerationLog()
    {
        var model = await _adminService.GetModerationLogsAsync();
        return View(model);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Ідентифікатор поточного користувача не знайдено.");
    }
}
