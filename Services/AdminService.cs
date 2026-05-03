using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public AdminService(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        return new AdminDashboardViewModel
        {
            UserCount = await _context.Users.CountAsync(),
            VacancyCount = await _context.Vacancies.CountAsync(),
            ResumeCount = await _context.Resumes.CountAsync(),
            ModerationLogCount = await _context.ModerationLogs.CountAsync()
        };
    }

    public async Task<List<AdminUserListItemViewModel>> GetUsersAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync();

        var result = new List<AdminUserListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                RoleName = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return result;
    }

    public async Task<List<AdminVacancyListItemViewModel>> GetVacanciesAsync()
    {
        return await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.EmployerProfile)
            .OrderByDescending(x => x.Status == VacancyStatus.UnderModeration)
            .ThenByDescending(x => x.PublishedAt)
            .Select(x => new AdminVacancyListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                CompanyName = x.EmployerProfile != null ? x.EmployerProfile.CompanyName : string.Empty,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                Status = x.Status,
                IsActive = x.IsActive,
                PublishedAt = x.PublishedAt
            })
            .ToListAsync();
    }

    public async Task<List<AdminResumeListItemViewModel>> GetResumesAsync()
    {
        return await _context.Resumes
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.CandidateProfile)
            .ThenInclude(x => x!.User)
            .OrderByDescending(x => x.Status == ResumeStatus.UnderModeration)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new AdminResumeListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                CandidateName = x.CandidateProfile != null && x.CandidateProfile.User != null
                    ? x.CandidateProfile.User.FullName
                    : string.Empty,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                Status = x.Status,
                IsPublished = x.IsPublished,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ModerationLogListItemViewModel>> GetModerationLogsAsync()
    {
        return await _context.ModerationLogs
            .AsNoTracking()
            .Include(x => x.AdminUser)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ModerationLogListItemViewModel
            {
                Id = x.Id,
                AdminName = x.AdminUser != null ? x.AdminUser.FullName : string.Empty,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                ActionType = x.ActionType,
                Note = x.Note,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task ApproveVacancyAsync(string adminUserId, int vacancyId, string? comment)
    {
        var vacancy = await _context.Vacancies
            .Include(x => x.EmployerProfile)
            .FirstAsync(x => x.Id == vacancyId);

        if (vacancy.Status != VacancyStatus.UnderModeration)
        {
            throw new InvalidOperationException("Приймати можна тільки вакансії, які очікують модерації.");
        }

        vacancy.Status = VacancyStatus.Published;
        vacancy.IsActive = true;
        vacancy.PublishedAt = DateTime.UtcNow;

        AddModerationLog(adminUserId, nameof(Vacancy), vacancy.Id, ModerationActionType.Approved,
            BuildNote($"Вакансію «{vacancy.Title}» схвалено.", comment));

        if (vacancy.EmployerProfile is not null)
        {
            AddNotification(
                vacancy.EmployerProfile.UserId,
                "Вакансію схвалено",
                $"Вашу вакансію «{vacancy.Title}» схвалено та опубліковано.",
                NotificationType.Success);
        }

        await _context.SaveChangesAsync();
    }

    public async Task RejectVacancyAsync(string adminUserId, int vacancyId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new InvalidOperationException("Для відхилення вакансії потрібно вказати коментар.");
        }

        var vacancy = await _context.Vacancies
            .Include(x => x.EmployerProfile)
            .FirstAsync(x => x.Id == vacancyId);

        if (vacancy.Status != VacancyStatus.UnderModeration)
        {
            throw new InvalidOperationException("Відхиляти можна тільки вакансії, які очікують модерації.");
        }

        vacancy.Status = VacancyStatus.Rejected;
        vacancy.IsActive = false;

        AddModerationLog(adminUserId, nameof(Vacancy), vacancy.Id, ModerationActionType.Rejected,
            BuildNote($"Вакансію «{vacancy.Title}» відхилено.", comment));

        if (vacancy.EmployerProfile is not null)
        {
            AddNotification(
                vacancy.EmployerProfile.UserId,
                "Вакансію відхилено",
                $"Вашу вакансію «{vacancy.Title}» відхилено. Коментар адміністратора: {comment}",
                NotificationType.Warning);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ApproveResumeAsync(string adminUserId, int resumeId, string? comment)
    {
        var resume = await _context.Resumes
            .Include(x => x.CandidateProfile)
            .FirstAsync(x => x.Id == resumeId);

        if (resume.Status != ResumeStatus.UnderModeration)
        {
            throw new InvalidOperationException("Приймати можна тільки резюме, які очікують модерації.");
        }

        resume.Status = ResumeStatus.Published;
        resume.IsPublished = true;
        resume.UpdatedAt = DateTime.UtcNow;

        AddModerationLog(adminUserId, nameof(Resume), resume.Id, ModerationActionType.Approved,
            BuildNote($"Резюме «{resume.Title}» схвалено.", comment));

        if (resume.CandidateProfile is not null)
        {
            AddNotification(
                resume.CandidateProfile.UserId,
                "Резюме схвалено",
                $"Ваше резюме «{resume.Title}» схвалено та доступне роботодавцям.",
                NotificationType.Success);
        }

        await _context.SaveChangesAsync();
    }

    public async Task RejectResumeAsync(string adminUserId, int resumeId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new InvalidOperationException("Для відхилення резюме потрібно вказати коментар.");
        }

        var resume = await _context.Resumes
            .Include(x => x.CandidateProfile)
            .FirstAsync(x => x.Id == resumeId);

        if (resume.Status != ResumeStatus.UnderModeration)
        {
            throw new InvalidOperationException("Відхиляти можна тільки резюме, які очікують модерації.");
        }

        resume.Status = ResumeStatus.Rejected;
        resume.IsPublished = false;
        resume.UpdatedAt = DateTime.UtcNow;

        AddModerationLog(adminUserId, nameof(Resume), resume.Id, ModerationActionType.Rejected,
            BuildNote($"Резюме «{resume.Title}» відхилено.", comment));

        if (resume.CandidateProfile is not null)
        {
            AddNotification(
                resume.CandidateProfile.UserId,
                "Резюме відхилено",
                $"Ваше резюме «{resume.Title}» відхилено. Коментар адміністратора: {comment}",
                NotificationType.Warning);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ActivateUserAsync(string adminUserId, string userId, string? comment)
    {
        var user = await _context.Users.FirstAsync(x => x.Id == userId);
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateSecurityStampAsync(user);

        AddModerationLog(adminUserId, nameof(User), 0, ModerationActionType.Updated,
            BuildNote($"Користувача {user.Email} активовано.", comment));

        AddNotification(
            user.Id,
            "Акаунт активовано",
            "Ваш акаунт знову активний. Можете користуватися системою.",
            NotificationType.Success);

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateUserAsync(string adminUserId, string userId, string? comment)
    {
        if (adminUserId == userId)
        {
            throw new InvalidOperationException("Адміністратор не може деактивувати власний акаунт.");
        }

        var user = await _context.Users.FirstAsync(x => x.Id == userId);
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateSecurityStampAsync(user);

        AddModerationLog(adminUserId, nameof(User), 0, ModerationActionType.Deactivated,
            BuildNote($"Користувача {user.Email} деактивовано.", comment));

        AddNotification(
            user.Id,
            "Акаунт деактивовано",
            string.IsNullOrWhiteSpace(comment)
                ? "Ваш акаунт деактивовано адміністратором."
                : $"Ваш акаунт деактивовано адміністратором. Коментар: {comment}",
            NotificationType.Warning);

        await _context.SaveChangesAsync();
    }

    private void AddModerationLog(string adminUserId, string entityName, int entityId, ModerationActionType actionType, string note)
    {
        _context.ModerationLogs.Add(new ModerationLog
        {
            AdminUserId = adminUserId,
            EntityName = entityName,
            EntityId = entityId,
            ActionType = actionType,
            Note = note
        });
    }

    private void AddNotification(string userId, string title, string content, NotificationType type)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = type
        });
    }

    private static string BuildNote(string action, string? comment)
    {
        return string.IsNullOrWhiteSpace(comment)
            ? action
            : $"{action} Коментар: {comment}";
    }
}
