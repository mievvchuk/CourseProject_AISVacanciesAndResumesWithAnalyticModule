using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

public class VacanciesController : Controller
{
    private readonly IVacancyService _vacancyService;
    private readonly IResumeService _resumeService;

    public VacanciesController(IVacancyService vacancyService, IResumeService resumeService)
    {
        _vacancyService = vacancyService;
        _resumeService = resumeService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index([FromQuery] VacancyFilterViewModel filter)
    {
        var model = await _vacancyService.SearchAsync(filter, GetUserIdOrNull(), User.IsInRole("Employer"));
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Candidate"))
        {
            model.ResumeOptions = await _resumeService.GetResumeOptionsAsync(GetUserId());
        }

        return View(model);
    }

    [Authorize(Roles = "Employer")]
    [HttpGet]
    public async Task<IActionResult> My()
    {
        if (!await _vacancyService.HasEmployerProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "EmployerProfiles");
        }

        var model = await _vacancyService.GetEmployerVacanciesAsync(GetUserId());
        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _vacancyService.GetDetailsModelAsync(id, GetUserIdOrNull(), User.IsInRole("Employer"), User.IsInRole("Admin"));
        if (model is not null && User.Identity?.IsAuthenticated == true && User.IsInRole("Candidate"))
        {
            model.ResumeOptions = await _resumeService.GetResumeOptionsAsync(GetUserId());
        }

        return model is null ? NotFound() : View(model);
    }

    [Authorize(Roles = "Employer")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await _vacancyService.HasEmployerProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "EmployerProfiles");
        }

        var model = await _vacancyService.GetCreateModelAsync(GetUserId());
        return View(model);
    }

    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VacancyFormViewModel model)
    {
        if (!await _vacancyService.HasEmployerProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "EmployerProfiles");
        }

        await ValidateVacancyModelAsync(model);

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await _vacancyService.GetCategoriesAsync();
            model.SkillOptions = await _vacancyService.GetSkillsAsync();
            return View(model);
        }

        await _vacancyService.CreateAsync(GetUserId(), model);
        TempData["StatusMessage"] = "Вакансію створено та надіслано на модерацію.";
        return RedirectToAction(nameof(My));
    }

    [Authorize(Roles = "Employer")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await _vacancyService.HasEmployerProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "EmployerProfiles");
        }

        var model = await _vacancyService.GetEditModelAsync(GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VacancyFormViewModel model)
    {
        if (!await _vacancyService.HasEmployerProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "EmployerProfiles");
        }

        await ValidateVacancyModelAsync(model);

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await _vacancyService.GetCategoriesAsync();
            model.SkillOptions = await _vacancyService.GetSkillsAsync();
            return View(model);
        }

        await _vacancyService.UpdateAsync(GetUserId(), model);
        TempData["StatusMessage"] = "Вакансію оновлено та повторно надіслано на модерацію.";
        return RedirectToAction(nameof(My));
    }

    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        if (!await _vacancyService.HasEmployerProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "EmployerProfiles");
        }

        await _vacancyService.CloseAsync(GetUserId(), id);
        TempData["StatusMessage"] = "Вакансію закрито.";
        return RedirectToAction(nameof(My));
    }

    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _vacancyService.HasEmployerProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "EmployerProfiles");
        }

        await _vacancyService.DeleteAsync(GetUserId(), id);
        TempData["StatusMessage"] = "Вакансію архівовано.";
        return RedirectToAction(nameof(My));
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Ідентифікатор поточного користувача не знайдено.");
    }

    private string? GetUserIdOrNull()
    {
        return User.Identity?.IsAuthenticated == true ? GetUserId() : null;
    }

    private async Task ValidateVacancyModelAsync(VacancyFormViewModel model)
    {
        if (model.CategoryId <= 0)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Оберіть категорію.");
        }

        if (model.SalaryFrom > model.SalaryTo)
        {
            ModelState.AddModelError(nameof(model.SalaryFrom), "Мінімальна зарплата не може бути більшою за максимальну.");
        }

        var categoryIds = (await _vacancyService.GetCategoriesAsync())
            .Select(x => int.TryParse(x.Value, out var id) ? id : 0)
            .ToHashSet();

        if (model.CategoryId > 0 && !categoryIds.Contains(model.CategoryId))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Обрана категорія не існує.");
        }

        var selectedSkillIds = model.SelectedSkillIds.Distinct().ToList();
        if (selectedSkillIds.Count == 0)
        {
            return;
        }

        var skillIds = (await _vacancyService.GetSkillsAsync())
            .Select(x => int.TryParse(x.Value, out var id) ? id : 0)
            .ToHashSet();

        if (selectedSkillIds.Any(x => x <= 0 || !skillIds.Contains(x)))
        {
            ModelState.AddModelError(nameof(model.SelectedSkillIds), "Одна або кілька обраних навичок не існують.");
        }
    }
}
