using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

[Authorize]
public class ApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationWorkflowService _applicationWorkflowService;

    public ApplicationsController(
        ApplicationDbContext context,
        IApplicationWorkflowService applicationWorkflowService)
    {
        _context = context;
        _applicationWorkflowService = applicationWorkflowService;
    }

    [Authorize(Roles = "Candidate")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int resumeId, int vacancyId, string? coverLetter)
    {
        if (resumeId <= 0 || vacancyId <= 0)
        {
            TempData["StatusMessage"] = "Оберіть резюме перед поданням заявки.";
            return RedirectToAction("Index", "Vacancies");
        }

        var userId = GetUserId();

        var resumeExists = await _context.Resumes
            .AnyAsync(x => x.Id == resumeId && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        var vacancyExists = await _context.Vacancies
            .AnyAsync(x => x.Id == vacancyId && x.IsActive && x.Status == VacancyStatus.Published);

        var duplicateExists = await _context.Applications
            .AnyAsync(x => x.VacancyId == vacancyId && x.CandidateUserId == userId);

        if (!resumeExists || !vacancyExists)
        {
            return NotFound();
        }

        if (duplicateExists)
        {
            TempData["StatusMessage"] = "Ви вже подавали заявку на цю вакансію.";
            return RedirectToAction("Index", "Vacancies");
        }

        try
        {
            await _applicationWorkflowService.ApplyAsync(resumeId, vacancyId, userId, coverLetter);
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("already", StringComparison.OrdinalIgnoreCase))
        {
            TempData["StatusMessage"] = "Ви вже подавали заявку на цю вакансію.";
            return RedirectToAction("Index", "Vacancies");
        }

        TempData["StatusMessage"] = "Заявку успішно подано.";
        return RedirectToAction(nameof(MyApplications));
    }

    [Authorize(Roles = "Candidate")]
    [HttpGet]
    public async Task<IActionResult> MyApplications([FromQuery] MyApplicationsFilterViewModel filter)
    {
        var query = _context.Applications
            .AsNoTracking()
            .Include(x => x.Resume)
            .Include(x => x.Vacancy)
            .ThenInclude(x => x!.EmployerProfile)
            .Where(x => x.CandidateUserId == GetUserId())
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.VacancyTitle))
        {
            var vacancyTitle = filter.VacancyTitle.Trim().ToLower();
            query = query.Where(x => x.Vacancy != null && x.Vacancy.Title.ToLower().Contains(vacancyTitle));
        }

        if (!string.IsNullOrWhiteSpace(filter.CompanyName))
        {
            var companyName = filter.CompanyName.Trim().ToLower();
            query = query.Where(x =>
                x.Vacancy != null &&
                x.Vacancy.EmployerProfile != null &&
                x.Vacancy.EmployerProfile.CompanyName.ToLower().Contains(companyName));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.SubmittedFrom.HasValue)
        {
            var submittedFrom = filter.SubmittedFrom.Value.Date;
            query = query.Where(x => x.CreatedAt >= submittedFrom);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ApplicationListItemViewModel
            {
                Id = x.Id,
                VacancyId = x.VacancyId,
                ResumeId = x.ResumeId,
                VacancyTitle = x.Vacancy != null ? x.Vacancy.Title : string.Empty,
                ResumeTitle = x.Resume != null ? x.Resume.Title : string.Empty,
                CompanyName = x.Vacancy != null && x.Vacancy.EmployerProfile != null ? x.Vacancy.EmployerProfile.CompanyName : string.Empty,
                MatchingPercent = x.MatchingPercent,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return View(new MyApplicationsIndexViewModel
        {
            Filter = filter,
            Items = items
        });
    }

    [Authorize(Roles = "Employer")]
    [HttpGet]
    public async Task<IActionResult> EmployerApplications()
    {
        var employerUserId = GetUserId();

        var items = await _context.Applications
            .AsNoTracking()
            .Include(x => x.Resume)
            .Include(x => x.CandidateUser)
            .Include(x => x.Vacancy)
            .ThenInclude(x => x!.EmployerProfile)
            .Where(x => x.Vacancy != null &&
                x.Vacancy.EmployerProfile != null &&
                x.Vacancy.EmployerProfile.UserId == employerUserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ApplicationListItemViewModel
            {
                Id = x.Id,
                VacancyId = x.VacancyId,
                ResumeId = x.ResumeId,
                VacancyTitle = x.Vacancy != null ? x.Vacancy.Title : string.Empty,
                ResumeTitle = x.Resume != null ? x.Resume.Title : string.Empty,
                CandidateFullName = x.CandidateUser != null ? x.CandidateUser.FullName : string.Empty,
                MatchingPercent = x.MatchingPercent,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return View(items);
    }

    [Authorize(Roles = "Employer")]
    [HttpGet]
    public async Task<IActionResult> VacancyApplications(int vacancyId)
    {
        var ownsVacancy = await _context.Vacancies
            .AnyAsync(x => x.Id == vacancyId && x.EmployerProfile != null && x.EmployerProfile.UserId == GetUserId());

        if (!ownsVacancy)
        {
            return NotFound();
        }

        var items = await _context.Applications
            .AsNoTracking()
            .Include(x => x.Resume)
            .Include(x => x.CandidateUser)
            .Include(x => x.Vacancy)
            .Where(x => x.VacancyId == vacancyId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ApplicationListItemViewModel
            {
                Id = x.Id,
                VacancyId = x.VacancyId,
                ResumeId = x.ResumeId,
                VacancyTitle = x.Vacancy != null ? x.Vacancy.Title : string.Empty,
                ResumeTitle = x.Resume != null ? x.Resume.Title : string.Empty,
                CandidateFullName = x.CandidateUser != null ? x.CandidateUser.FullName : string.Empty,
                MatchingPercent = x.MatchingPercent,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        ViewBag.VacancyId = vacancyId;
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var application = await _context.Applications
            .AsNoTracking()
            .Include(x => x.Resume)
            .Include(x => x.CandidateUser)
            .Include(x => x.Vacancy)
            .ThenInclude(x => x!.EmployerProfile)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (application is null)
        {
            return NotFound();
        }

        var isCandidateOwner = application.CandidateUserId == GetUserId();
        var isEmployerOwner = application.Vacancy != null &&
            application.Vacancy.EmployerProfile != null &&
            application.Vacancy.EmployerProfile.UserId == GetUserId();

        if (!isCandidateOwner && !isEmployerOwner)
        {
            return Forbid();
        }

        if (isEmployerOwner && application.Status == ApplicationStatus.New)
        {
            await _applicationWorkflowService.MarkAsReviewedAsync(id, GetUserId());
            application.Status = ApplicationStatus.Reviewed;
        }

        var model = new ApplicationViewModel
        {
            Id = application.Id,
            ResumeId = application.ResumeId,
            VacancyId = application.VacancyId,
            VacancyTitle = application.Vacancy != null ? application.Vacancy.Title : string.Empty,
            ResumeTitle = application.Resume != null ? application.Resume.Title : string.Empty,
            CandidateFullName = application.CandidateUser != null ? application.CandidateUser.FullName : string.Empty,
            CandidateEmail = application.CandidateUser != null ? application.CandidateUser.Email ?? string.Empty : string.Empty,
            CompanyName = application.Vacancy != null && application.Vacancy.EmployerProfile != null
                ? application.Vacancy.EmployerProfile.CompanyName
                : string.Empty,
            CoverLetter = application.CoverLetter ?? string.Empty,
            MatchingPercent = application.MatchingPercent,
            Status = application.Status,
            CreatedAt = application.CreatedAt,
            CanUpdateStatus = isEmployerOwner
        };

        return View(model);
    }

    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ApplicationStatus status)
    {
        if (status is not (ApplicationStatus.Accepted or ApplicationStatus.Rejected))
        {
            return BadRequest();
        }

        var ownsApplication = await _context.Applications
            .AnyAsync(x => x.Id == id && x.Vacancy != null && x.Vacancy.EmployerProfile != null && x.Vacancy.EmployerProfile.UserId == GetUserId());

        if (!ownsApplication)
        {
            return Forbid();
        }

        await _applicationWorkflowService.UpdateStatusAsync(id, status, GetUserId());
        return RedirectToAction(nameof(Details), new { id });
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Ідентифікатор поточного користувача не знайдено.");
    }
}
