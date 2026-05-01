using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Extensions;
using AisVacanciesAndResumes.Models;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class ApplicationWorkflowService : IApplicationWorkflowService
{
    private readonly ApplicationDbContext _context;
    private readonly IMatchingService _matchingService;

    public ApplicationWorkflowService(ApplicationDbContext context, IMatchingService matchingService)
    {
        _context = context;
        _matchingService = matchingService;
    }

    public async Task ApplyAsync(int resumeId, int vacancyId, string candidateUserId, string? coverLetter)
    {
        var resume = await _context.Resumes
            .Include(x => x.CandidateProfile)
            .FirstOrDefaultAsync(x => x.Id == resumeId);

        if (resume is null || resume.CandidateProfile?.UserId != candidateUserId)
        {
            throw new InvalidOperationException("Резюме не знайдено або воно не належить поточному кандидату.");
        }

        var vacancy = await _context.Vacancies
            .Include(x => x.EmployerProfile)
            .FirstOrDefaultAsync(x => x.Id == vacancyId);

        if (vacancy is null || !vacancy.IsActive || vacancy.Status != VacancyStatus.Published)
        {
            throw new InvalidOperationException("Ця вакансія зараз недоступна для подання заявки.");
        }

        var exists = await _context.Applications
            .AnyAsync(x => x.VacancyId == vacancyId && x.CandidateUserId == candidateUserId);

        if (exists)
        {
            throw new InvalidOperationException("Ви вже подавали заявку на цю вакансію.");
        }

        var matchingPercent = await _matchingService.CalculateMatchPercentageAsync(resumeId, vacancyId);

        var application = new Application
        {
            ResumeId = resumeId,
            VacancyId = vacancyId,
            CandidateUserId = candidateUserId,
            CoverLetter = coverLetter,
            MatchingPercent = matchingPercent,
            Status = ApplicationStatus.New,
            AppliedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Applications.Add(application);

        await CreateNotificationAsync(
            candidateUserId,
            "Заявку подано",
            "Вашу заявку надіслано роботодавцю.",
            NotificationType.Success);

        if (vacancy.EmployerProfile is not null)
        {
            await CreateNotificationAsync(
                vacancy.EmployerProfile.UserId,
                "Нова заявка кандидата",
                $"Кандидат подав заявку на вакансію «{vacancy.Title}».",
                NotificationType.Info);
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReviewedAsync(int applicationId, string actorUserId)
    {
        var application = await GetOwnedApplicationAsync(applicationId, actorUserId);

        if (application.Status != ApplicationStatus.New)
        {
            return;
        }

        application.Status = ApplicationStatus.Reviewed;

        _context.ModerationLogs.Add(new ModerationLog
        {
            AdminUserId = actorUserId,
            EntityName = nameof(Application),
            EntityId = application.Id,
            ActionType = ModerationActionType.Updated,
            Note = "Заявку автоматично позначено як переглянуту."
        });

        await CreateNotificationAsync(
            application.CandidateUserId,
            "Заявку переглянуто",
            "Роботодавець переглянув вашу заявку.",
            NotificationType.Info);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int applicationId, ApplicationStatus status, string actorUserId)
    {
        if (status is not (ApplicationStatus.Accepted or ApplicationStatus.Rejected))
        {
            throw new InvalidOperationException("Вручну можна встановити тільки фінальне рішення за заявкою.");
        }

        var application = await GetOwnedApplicationAsync(applicationId, actorUserId);
        application.Status = status;

        _context.ModerationLogs.Add(new ModerationLog
        {
            AdminUserId = actorUserId,
            EntityName = nameof(Application),
            EntityId = application.Id,
            ActionType = ModerationActionType.Updated,
            Note = $"Статус заявки змінено на {status.GetDisplayName()}."
        });

        await CreateNotificationAsync(
            application.CandidateUserId,
            "Статус заявки оновлено",
            $"Статус вашої заявки змінено на «{status.GetDisplayName()}».",
            status == ApplicationStatus.Rejected ? NotificationType.Warning : NotificationType.Info);

        await _context.SaveChangesAsync();
    }

    private async Task<Application> GetOwnedApplicationAsync(int applicationId, string actorUserId)
    {
        var application = await _context.Applications
            .Include(x => x.Vacancy)
            .ThenInclude(x => x!.EmployerProfile)
            .FirstOrDefaultAsync(x => x.Id == applicationId);

        if (application is null)
        {
            throw new InvalidOperationException("Заявку не знайдено.");
        }

        if (application.Vacancy?.EmployerProfile?.UserId != actorUserId)
        {
            throw new InvalidOperationException("Змінювати цю заявку може тільки власник вакансії.");
        }

        return application;
    }

    private async Task CreateNotificationAsync(string userId, string title, string content, NotificationType type)
    {
        await _context.Notifications.AddAsync(new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = type
        });
    }
}
