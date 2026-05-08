using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Route("api/reference-data")]
public class ReferenceDataApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReferenceDataApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryApiDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("skills")]
    public async Task<IActionResult> GetSkills()
    {
        var skills = await _context.Skills
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SkillApiDto
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category,
                Description = x.Description
            })
            .ToListAsync();

        return Ok(skills);
    }
}
