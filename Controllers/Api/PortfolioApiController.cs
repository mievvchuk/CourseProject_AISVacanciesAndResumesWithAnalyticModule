using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Authorize(Roles = "Candidate")]
[Route("api/portfolio")]
public class PortfolioApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PortfolioApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUserPortfolio()
    {
        var userId = GetUserId();
        var items = await ProjectPortfolio(userId)
            .OrderBy(x => x.Title)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var item = await ProjectPortfolio(userId)
            .FirstOrDefaultAsync(x => x.Id == id);

        return item is null ? NotFound() : Ok(item);
    }

    private IQueryable<PortfolioItemApiDto> ProjectPortfolio(string userId)
    {
        return _context.PortfolioItems
            .AsNoTracking()
            .Where(x => x.CandidateProfile != null && x.CandidateProfile.UserId == userId)
            .Select(x => new PortfolioItemApiDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Url = x.Url,
                ImagePath = x.ImagePath
            });
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }
}
