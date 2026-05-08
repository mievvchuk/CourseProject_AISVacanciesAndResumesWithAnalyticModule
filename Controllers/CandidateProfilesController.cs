using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.CandidateProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AisVacanciesAndResumes.Controllers;

[Authorize(Roles = "Candidate")]
public class CandidateProfilesController : Controller
{
    private readonly ICandidateProfileService _candidateProfileService;
    private readonly UserManager<User> _userManager;

    public CandidateProfilesController(
        ICandidateProfileService candidateProfileService,
        UserManager<User> userManager)
    {
        _candidateProfileService = candidateProfileService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Details()
    {
        var user = await GetCurrentUserAsync();
        if (!await _candidateProfileService.ExistsAsync(user.Id))
        {
            return RedirectToAction(nameof(Create));
        }

        var model = await _candidateProfileService.GetDetailsAsync(user.Id, user.FullName, user.Email ?? string.Empty);
        if (model is null)
        {
            return RedirectToAction(nameof(Create));
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var user = await GetCurrentUserAsync();
        if (await _candidateProfileService.ExistsAsync(user.Id))
        {
            return RedirectToAction(nameof(Edit));
        }

        var model = await _candidateProfileService.GetOrCreateFormAsync(user.Id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CandidateProfileFormViewModel model)
    {
        var user = await GetCurrentUserAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _candidateProfileService.SaveAsync(user.Id, model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.PhotoFile), exception.Message);
            return View(model);
        }

        TempData["StatusMessage"] = "Профіль кандидата створено.";
        return RedirectToAction(nameof(Details));
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await GetCurrentUserAsync();
        var model = await _candidateProfileService.GetOrCreateFormAsync(user.Id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CandidateProfileFormViewModel model)
    {
        var user = await GetCurrentUserAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _candidateProfileService.SaveAsync(user.Id, model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.PhotoFile), exception.Message);
            return View(model);
        }

        TempData["StatusMessage"] = "Профіль кандидата збережено.";
        return RedirectToAction(nameof(Details));
    }

    private async Task<User> GetCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user ?? throw new InvalidOperationException("Поточного користувача не знайдено.");
    }
}
