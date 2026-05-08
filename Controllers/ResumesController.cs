using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Resumes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

[Authorize(Roles = "Candidate")]
public class ResumesController : Controller
{
    private readonly IResumeService _resumeService;
    private readonly IResumeParserService _resumeParserService;
    private readonly UserManager<User> _userManager;

    public ResumesController(
        IResumeService resumeService,
        IResumeParserService resumeParserService,
        UserManager<User> userManager)
    {
        _resumeService = resumeService;
        _resumeParserService = resumeParserService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (!await _resumeService.HasCandidateProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "CandidateProfiles");
        }

        var items = await _resumeService.GetUserResumesAsync(GetUserId());
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await _resumeService.HasCandidateProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "CandidateProfiles");
        }

        var model = await _resumeService.GetCreateModelAsync(GetUserId());
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResumeFormViewModel model)
    {
        if (!await _resumeService.HasCandidateProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "CandidateProfiles");
        }

        var parseResult = await ApplyParsedValuesAsync(model, model.ReplaceFieldsFromFile);

        if (!ModelState.IsValid)
        {
            await PrepareFormAfterValidationErrorAsync(model, parseResult);
            return View(model);
        }

        try
        {
            await _resumeService.CreateAsync(GetUserId(), model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PrepareFormAfterValidationErrorAsync(model, parseResult);
            return View(model);
        }

        TempData["StatusMessage"] = model.IsPublished
            ? "Резюме збережено та відправлено на модерацію."
            : "Резюме збережено як чернетку.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Parse(ResumeFormViewModel model)
    {
        if (model.ResumeFile is null || model.ResumeFile.Length == 0)
        {
            return BadRequest(new { message = "Оберіть PDF або DOCX файл резюме." });
        }

        var parsed = await _resumeParserService.ParseAsync(model.ResumeFile);
        var skillsDescription = !string.IsNullOrWhiteSpace(parsed.SkillsDescription)
            ? parsed.SkillsDescription
            : string.Join(", ", parsed.ParsedSkillNames);

        return Json(new
        {
            desiredPosition = parsed.DesiredPosition,
            categoryName = parsed.CategoryName,
            summary = parsed.Summary,
            education = parsed.Education,
            experience = parsed.Experience,
            skillsDescription,
            employmentType = parsed.EmploymentType?.ToString(),
            educationLevel = parsed.EducationLevel?.ToString(),
            experienceLevel = parsed.ExperienceLevel?.ToString(),
            experienceYears = parsed.YearsOfExperience,
            desiredSalary = parsed.DesiredSalary,
            parsedSkillNames = parsed.ParsedSkillNames,
            message = GetParseMessage(parsed)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (!await _resumeService.HasCandidateProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "CandidateProfiles");
        }

        var model = await _resumeService.GetDetailsModelAsync(GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await _resumeService.HasCandidateProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "CandidateProfiles");
        }

        var model = await _resumeService.GetEditModelAsync(GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ResumeFormViewModel model)
    {
        if (!await _resumeService.HasCandidateProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "CandidateProfiles");
        }

        var parseResult = await ApplyParsedValuesAsync(model, model.ReplaceFieldsFromFile);

        if (!ModelState.IsValid)
        {
            await PrepareFormAfterValidationErrorAsync(model, parseResult);
            return View(model);
        }

        try
        {
            await _resumeService.UpdateAsync(GetUserId(), model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PrepareFormAfterValidationErrorAsync(model, parseResult);
            return View(model);
        }

        TempData["StatusMessage"] = model.IsPublished
            ? "Резюме оновлено та повторно відправлено на модерацію."
            : "Резюме оновлено як чернетку.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _resumeService.HasCandidateProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "CandidateProfiles");
        }

        var model = await _resumeService.GetDeleteModelAsync(GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _resumeService.HasCandidateProfileAsync(GetUserId()))
        {
            return RedirectToAction("Create", "CandidateProfiles");
        }

        await _resumeService.DeleteAsync(GetUserId(), id);
        TempData["StatusMessage"] = "Резюме заархівовано.";
        return RedirectToAction(nameof(Index));
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }

    private async Task<ResumeParseResult> ApplyParsedValuesAsync(ResumeFormViewModel model, bool replaceExistingValues)
    {
        if (model.ResumeFile is null || model.ResumeFile.Length == 0)
        {
            TryValidateModel(model);
            return new ResumeParseResult();
        }

        var parsed = await _resumeParserService.ParseAsync(model.ResumeFile);

        model.DesiredPosition = MergeParsedValue(model.DesiredPosition, parsed.DesiredPosition, replaceExistingValues);
        model.CategoryName = MergeParsedValue(model.CategoryName, parsed.CategoryName, replaceExistingValues);
        model.Summary = MergeParsedValue(model.Summary, parsed.Summary, replaceExistingValues);
        model.Education = MergeParsedValue(model.Education, parsed.Education, replaceExistingValues);
        model.Experience = MergeParsedValue(model.Experience, parsed.Experience, replaceExistingValues);
        model.SkillsDescription = MergeParsedValue(
            model.SkillsDescription,
            !string.IsNullOrWhiteSpace(parsed.SkillsDescription) ? parsed.SkillsDescription : string.Join(", ", parsed.ParsedSkillNames),
            replaceExistingValues);

        if ((replaceExistingValues || model.ParsedSkillNames.Count == 0) && parsed.ParsedSkillNames.Count > 0)
        {
            model.ParsedSkillNames = parsed.ParsedSkillNames;
        }

        if (parsed.EmploymentType.HasValue)
        {
            model.EmploymentType = parsed.EmploymentType.Value;
        }

        if (parsed.EducationLevel.HasValue)
        {
            model.EducationLevel = parsed.EducationLevel.Value;
        }

        if (parsed.ExperienceLevel.HasValue)
        {
            model.ExperienceLevel = parsed.ExperienceLevel.Value;
        }

        if ((replaceExistingValues || model.ExperienceYears == 0) && parsed.YearsOfExperience.HasValue)
        {
            model.ExperienceYears = parsed.YearsOfExperience.Value;
        }

        if ((replaceExistingValues || !model.DesiredSalary.HasValue) && parsed.DesiredSalary.HasValue)
        {
            model.DesiredSalary = parsed.DesiredSalary.Value;
        }

        await UpdateCurrentUserContactsAsync(parsed);

        ModelState.Clear();
        TryValidateModel(model);

        return parsed;
    }

    private static string MergeParsedValue(string currentValue, string parsedValue, bool replaceExistingValue)
    {
        return replaceExistingValue || string.IsNullOrWhiteSpace(currentValue) ? parsedValue : currentValue;
    }

    private async Task PrepareFormAfterValidationErrorAsync(ResumeFormViewModel model, ResumeParseResult parseResult)
    {
        model.CategoryOptions = await _resumeService.GetCategoriesAsync();
        ViewData["ParseMessage"] = GetParseMessage(parseResult);
    }

    private async Task UpdateCurrentUserContactsAsync(ResumeParseResult parsed)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return;
        }

        var shouldUpdateUser = false;

        if (string.IsNullOrWhiteSpace(currentUser.Email) && !string.IsNullOrWhiteSpace(parsed.Email))
        {
            currentUser.Email = parsed.Email;
            currentUser.UserName = parsed.Email;
            shouldUpdateUser = true;
        }

        if (string.IsNullOrWhiteSpace(currentUser.PhoneNumber) && !string.IsNullOrWhiteSpace(parsed.PhoneNumber))
        {
            currentUser.PhoneNumber = parsed.PhoneNumber;
            shouldUpdateUser = true;
        }

        if (shouldUpdateUser)
        {
            await _userManager.UpdateAsync(currentUser);
        }
    }

    private static string GetParseMessage(ResumeParseResult parsed)
    {
        return HasParsedData(parsed)
            ? "Дані з файлу підтягнуто. Перевірте поля перед збереженням."
            : "Не вдалося автоматично витягнути достатньо даних із файлу. Перевірте PDF/DOCX або доповніть поля вручну.";
    }

    private static bool HasParsedData(ResumeParseResult parsed)
    {
        return !string.IsNullOrWhiteSpace(parsed.DesiredPosition)
            || !string.IsNullOrWhiteSpace(parsed.CategoryName)
            || !string.IsNullOrWhiteSpace(parsed.Summary)
            || !string.IsNullOrWhiteSpace(parsed.Education)
            || !string.IsNullOrWhiteSpace(parsed.Experience)
            || !string.IsNullOrWhiteSpace(parsed.SkillsDescription)
            || parsed.ParsedSkillNames.Count > 0
            || !string.IsNullOrWhiteSpace(parsed.Email)
            || !string.IsNullOrWhiteSpace(parsed.PhoneNumber)
            || parsed.YearsOfExperience.HasValue
            || parsed.DesiredSalary.HasValue
            || parsed.EducationLevel.HasValue
            || parsed.ExperienceLevel.HasValue
            || parsed.EmploymentType.HasValue;
    }
}
