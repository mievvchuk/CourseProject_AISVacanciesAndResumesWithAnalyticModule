using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Portfolio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

[Authorize(Roles = "Candidate")]
public class PortfolioController : Controller
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _portfolioService.GetUserPortfolioAsync(GetUserId());
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await _portfolioService.GetCreateModelAsync(GetUserId());
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PortfolioItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _portfolioService.CreateAsync(GetUserId(), model);
        TempData["StatusMessage"] = "Portfolio item created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _portfolioService.GetEditModelAsync(GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PortfolioItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _portfolioService.UpdateAsync(GetUserId(), model);
        TempData["StatusMessage"] = "Portfolio item updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _portfolioService.GetDeleteModelAsync(GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _portfolioService.DeleteAsync(GetUserId(), id);
        TempData["StatusMessage"] = "Portfolio item deleted.";
        return RedirectToAction(nameof(Index));
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }
}
