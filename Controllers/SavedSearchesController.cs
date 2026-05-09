using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.SavedSearches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

[Authorize]
public class SavedSearchesController : Controller
{
    private readonly ISavedSearchService _savedSearchService;

    public SavedSearchesController(ISavedSearchService savedSearchService)
    {
        _savedSearchService = savedSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _savedSearchService.GetUserSavedSearchesAsync(GetUserId());
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create([FromQuery] SavedSearchFormViewModel filter)
    {
        var model = await _savedSearchService.GetCreateModelAsync(filter);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost([FromForm] SavedSearchFormViewModel model)
    {
        await _savedSearchService.CreateAsync(GetUserId(), model);
        TempData["StatusMessage"] = "\u041f\u043e\u0448\u0443\u043a \u0437\u0431\u0435\u0440\u0435\u0436\u0435\u043d\u043e."; return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Open(int id)
    {
        var filter = await _savedSearchService.GetSavedFilterAsync(GetUserId(), id);
        if (filter is null)
        {
            return NotFound();
        }

        return RedirectToAction("Index", "Vacancies", new
        {
            title = filter.Query,
            city = filter.City,
            categoryId = filter.CategoryId,
            employmentType = filter.EmploymentType,
            experienceLevel = filter.ExperienceLevel
        });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _savedSearchService.GetDeleteModelAsync(GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _savedSearchService.DeleteAsync(GetUserId(), id);
        TempData["StatusMessage"] = "\u0417\u0431\u0435\u0440\u0435\u0436\u0435\u043d\u0438\u0439 \u043f\u043e\u0448\u0443\u043a \u0432\u0438\u0434\u0430\u043b\u0435\u043d\u043e.";
        return RedirectToAction(nameof(Index));
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }
}
