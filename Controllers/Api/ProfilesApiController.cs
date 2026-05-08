using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/profiles")]
public class ProfilesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProfilesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Candidate")]
    [HttpGet("candidate/current")]
    public async Task<IActionResult> GetCurrentCandidateProfile()
    {
        var userId = GetUserId();
        var profile = await _context.CandidateProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .Select(x => new CandidateProfileApiDto
            {
                UserId = x.UserId,
                FullName = x.User != null ? x.User.FullName : string.Empty,
                Email = x.User != null ? x.User.Email ?? string.Empty : string.Empty,
                Headline = x.Headline,
                Summary = x.Summary,
                City = x.City,
                ExperienceYears = x.ExperienceYears,
                ExperienceLevel = x.ExperienceLevel,
                EducationLevel = x.EducationLevel,
                DesiredEmploymentType = x.DesiredEmploymentType,
                DesiredSalary = x.DesiredSalary
            })
            .FirstOrDefaultAsync();

        return profile is null ? NotFound() : Ok(profile);
    }

    [Authorize(Roles = "Employer")]
    [HttpGet("employer/current")]
    public async Task<IActionResult> GetCurrentEmployerProfile()
    {
        var userId = GetUserId();
        var profile = await _context.EmployerProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .Select(x => new EmployerProfileApiDto
            {
                UserId = x.UserId,
                FullName = x.User != null ? x.User.FullName : string.Empty,
                Email = x.User != null ? x.User.Email ?? string.Empty : string.Empty,
                CompanyName = x.CompanyName,
                Industry = x.Industry,
                Description = x.Description,
                CompanySize = x.CompanySize,
                Website = x.Website,
                City = x.City,
                Location = x.Location,
                FoundedYear = x.FoundedYear
            })
            .FirstOrDefaultAsync();

        return profile is null ? NotFound() : Ok(profile);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }
}
