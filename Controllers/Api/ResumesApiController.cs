using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Route("api/resumes")]
public class ResumesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ResumesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.CandidateProfile)
            .ThenInclude(x => x!.User)
            .Include(x => x.ResumeSkills)
            .ThenInclude(x => x.Skill)
            .Where(x => x.Id == id && x.IsPublished && x.Status == ResumeStatus.Published)
            .Select(x => new ResumeApiDto
            {
                Id = x.Id,
                Title = x.Title,
                DesiredPosition = x.DesiredPosition,
                Summary = x.Summary,
                Education = x.Education,
                Experience = x.Experience,
                SkillsDescription = x.SkillsDescription,
                EmploymentType = x.EmploymentType,
                ExperienceYears = x.ExperienceYears,
                ExperienceLevel = x.ExperienceLevel,
                EducationLevel = x.EducationLevel,
                DesiredSalary = x.DesiredSalary,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Category = x.Category != null ? x.Category.Name : string.Empty,
                CandidateName = x.CandidateProfile != null && x.CandidateProfile.User != null
                    ? x.CandidateProfile.User.FullName
                    : string.Empty,
                Skills = x.ResumeSkills.Select(s => s.Skill != null ? s.Skill.Name : string.Empty).ToList()
            })
            .FirstOrDefaultAsync();

        return resume is null ? NotFound() : Ok(resume);
    }
}
