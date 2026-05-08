using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/saved-searches")]
public class SavedSearchesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SavedSearchesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUserSavedSearches()
    {
        var userId = GetUserId();
        var searches = await ProjectSavedSearches(userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(searches);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var search = await ProjectSavedSearches(userId)
            .FirstOrDefaultAsync(x => x.Id == id);

        return search is null ? NotFound() : Ok(search);
    }

    private IQueryable<SavedSearchApiDto> ProjectSavedSearches(string userId)
    {
        return _context.SavedSearches
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.UserId == userId)
            .Select(x => new SavedSearchApiDto
            {
                Id = x.Id,
                SearchType = x.SearchType,
                Query = x.Query,
                City = x.City,
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                MinSalary = x.MinSalary,
                MaxSalary = x.MaxSalary,
                EmploymentType = x.EmploymentType,
                ExperienceLevel = x.ExperienceLevel,
                CreatedAt = x.CreatedAt
            });
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }
}
