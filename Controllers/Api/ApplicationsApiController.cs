using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Route("api/applications")]
public class ApplicationsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationWorkflowService _applicationWorkflowService;

    public ApplicationsApiController(
        ApplicationDbContext context,
        IApplicationWorkflowService applicationWorkflowService)
    {
        _context = context;
        _applicationWorkflowService = applicationWorkflowService;
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var application = await _context.Applications
            .AsNoTracking()
            .Include(x => x.Resume)
            .ThenInclude(x => x!.Category)
            .Include(x => x.Vacancy)
            .ThenInclude(x => x!.EmployerProfile)
            .Include(x => x.CandidateUser)
            .Where(x => x.Id == id)
            .Select(x => new ApplicationApiDto
            {
                Id = x.Id,
                ResumeId = x.ResumeId,
                VacancyId = x.VacancyId,
                CandidateUserId = x.CandidateUserId,
                CoverLetter = x.CoverLetter,
                MatchingPercent = x.MatchingPercent,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                ResumeTitle = x.Resume != null ? x.Resume.Title : string.Empty,
                ResumeCategory = x.Resume != null && x.Resume.Category != null ? x.Resume.Category.Name : string.Empty,
                VacancyTitle = x.Vacancy != null ? x.Vacancy.Title : string.Empty,
                CompanyName = x.Vacancy != null && x.Vacancy.EmployerProfile != null ? x.Vacancy.EmployerProfile.CompanyName : string.Empty,
                CandidateName = x.CandidateUser != null ? x.CandidateUser.FullName : string.Empty,
                EmployerUserId = x.Vacancy != null && x.Vacancy.EmployerProfile != null ? x.Vacancy.EmployerProfile.UserId : string.Empty
            })
            .FirstOrDefaultAsync();

        if (application is null)
        {
            return NotFound();
        }

        var canView = User.IsInRole("Admin")
            || application.CandidateUserId == userId
            || application.EmployerUserId == userId;

        if (!canView)
        {
            return Forbid();
        }

        return Ok(application);
    }

    [Authorize(Roles = "Candidate")]
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyApplicationRequestDto request)
    {
        var userId = GetUserId();

        var resumeExists = await _context.Resumes
            .AnyAsync(x => x.Id == request.ResumeId && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        if (!resumeExists)
        {
            return BadRequest(new { message = "Резюме не знайдено або воно не належить поточному кандидату." });
        }

        var vacancyExists = await _context.Vacancies
            .AnyAsync(x => x.Id == request.VacancyId && x.IsActive && x.Status == VacancyStatus.Published);

        if (!vacancyExists)
        {
            return BadRequest(new { message = "Ця вакансія зараз недоступна для подання заявки." });
        }

        var duplicateExists = await _context.Applications
            .AnyAsync(x => x.VacancyId == request.VacancyId && x.CandidateUserId == userId);

        if (duplicateExists)
        {
            return Conflict(new { message = "Ви вже подавали заявку на цю вакансію." });
        }

        try
        {
            await _applicationWorkflowService.ApplyAsync(request.ResumeId, request.VacancyId, userId, request.CoverLetter);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        var createdApplication = await _context.Applications
            .AsNoTracking()
            .Where(x => x.ResumeId == request.ResumeId && x.VacancyId == request.VacancyId && x.CandidateUserId == userId)
            .OrderByDescending(x => x.Id)
            .Select(x => new ApplicationCreatedApiDto
            {
                Id = x.Id,
                ResumeId = x.ResumeId,
                VacancyId = x.VacancyId,
                MatchingPercent = x.MatchingPercent,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = createdApplication.Id }, createdApplication);
    }

    [Authorize(Roles = "Employer")]
    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateApplicationStatusRequestDto request)
    {
        var userId = GetUserId();

        var ownsApplication = await _context.Applications
            .AnyAsync(x => x.Id == id && x.Vacancy != null && x.Vacancy.EmployerProfile != null && x.Vacancy.EmployerProfile.UserId == userId);

        if (!ownsApplication)
        {
            return Forbid();
        }

        try
        {
            await _applicationWorkflowService.UpdateStatusAsync(id, request.Status, userId);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        return Ok(new
        {
            ApplicationId = id,
            Status = request.Status,
            Message = "Статус заявки оновлено."
        });
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Ідентифікатор поточного користувача не знайдено.");
    }
}
