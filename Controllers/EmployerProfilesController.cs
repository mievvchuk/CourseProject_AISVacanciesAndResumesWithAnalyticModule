using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.EmployerProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AisVacanciesAndResumes.Controllers;

[Authorize(Roles = "Employer")]
public class EmployerProfilesController : Controller
{
    private readonly IEmployerProfileService _employerProfileService;
    private readonly UserManager<User> _userManager;

    public EmployerProfilesController(
        IEmployerProfileService employerProfileService,
        UserManager<User> userManager)
    {
        _employerProfileService = employerProfileService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Details()
    {
        var user = await GetCurrentUserAsync();
        if (!await _employerProfileService.ExistsAsync(user.Id))
        {
            return RedirectToAction(nameof(Create));
        }

        var model = await _employerProfileService.GetDetailsAsync(user.Id, user.FullName, user.Email ?? string.Empty);
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
        if (await _employerProfileService.ExistsAsync(user.Id))
        {
            return RedirectToAction(nameof(Edit));
        }

        var model = await _employerProfileService.GetOrCreateFormAsync(user.Id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployerProfileFormViewModel model)
    {
        var user = await GetCurrentUserAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _employerProfileService.SaveAsync(user.Id, model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.LogoFile), exception.Message);
            return View(model);
        }

        TempData["StatusMessage"] = "Employer profile created.";
        return RedirectToAction(nameof(Details));
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await GetCurrentUserAsync();
        var model = await _employerProfileService.GetOrCreateFormAsync(user.Id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployerProfileFormViewModel model)
    {
        var user = await GetCurrentUserAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _employerProfileService.SaveAsync(user.Id, model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.LogoFile), exception.Message);
            return View(model);
        }

        TempData["StatusMessage"] = "Employer profile saved.";
        return RedirectToAction(nameof(Details));
    }

    private async Task<User> GetCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user ?? throw new InvalidOperationException("Current user was not found.");
    }
}
