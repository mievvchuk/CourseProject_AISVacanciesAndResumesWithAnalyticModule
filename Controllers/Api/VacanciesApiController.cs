using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Route("api/vacancies")]
public class VacanciesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VacanciesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vacancies = await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.EmployerProfile)
            .Include(x => x.VacancySkills)
            .ThenInclude(x => x.Skill)
            .Where(x => x.IsActive && x.Status == VacancyStatus.Published)
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => new VacancyApiDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                EmploymentType = x.EmploymentType,
                ExperienceLevel = x.ExperienceLevel,
                Status = x.Status,
                PublishedAt = x.PublishedAt,
                Category = x.Category != null ? x.Category.Name : string.Empty,
                CompanyName = x.EmployerProfile != null ? x.EmployerProfile.CompanyName : string.Empty,
                City = x.EmployerProfile != null ? x.EmployerProfile.City : string.Empty,
                Skills = x.VacancySkills.Select(s => s.Skill != null ? s.Skill.Name : string.Empty).ToList()
            })
            .ToListAsync();

        return Ok(vacancies);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vacancy = await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.EmployerProfile)
            .Include(x => x.VacancySkills)
            .ThenInclude(x => x.Skill)
            .Where(x => x.Id == id && x.IsActive && x.Status == VacancyStatus.Published)
            .Select(x => new VacancyApiDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                EmploymentType = x.EmploymentType,
                ExperienceLevel = x.ExperienceLevel,
                Status = x.Status,
                PublishedAt = x.PublishedAt,
                Category = x.Category != null ? x.Category.Name : string.Empty,
                CompanyName = x.EmployerProfile != null ? x.EmployerProfile.CompanyName : string.Empty,
                City = x.EmployerProfile != null ? x.EmployerProfile.City : string.Empty,
                Skills = x.VacancySkills.Select(s => s.Skill != null ? s.Skill.Name : string.Empty).ToList()
            })
            .FirstOrDefaultAsync();

        return vacancy is null ? NotFound() : Ok(vacancy);
    }
}
