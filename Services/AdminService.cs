using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.Admin;
using AisVacanciesAndResumes.ViewModels.Resumes;
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

    public async Task<AdminUserIndexViewModel> GetUsersAsync(AdminUserFilterViewModel filter)
    {
        filter ??= new AdminUserFilterViewModel();

        var query = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(search) ||
                (x.Email != null && x.Email.ToLower().Contains(search)));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        var users = await query
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

        if (!string.IsNullOrWhiteSpace(filter.RoleName))
        {
            result = result
                .Where(x => string.Equals(x.RoleName, filter.RoleName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return new AdminUserIndexViewModel
        {
            Filter = filter,
            Items = result
        };
    }

    public async Task<AdminVacancyIndexViewModel> GetVacanciesAsync(AdminVacancyFilterViewModel filter)
    {
        filter ??= new AdminVacancyFilterViewModel();

        var query = _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.EmployerProfile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(search) ||
                (x.EmployerProfile != null && x.EmployerProfile.CompanyName.ToLower().Contains(search)) ||
                (x.Category != null && x.Category.Name.ToLower().Contains(search)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        var items = await query
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

        return new AdminVacancyIndexViewModel
        {
            Filter = filter,
            Items = items
        };
    }

    public async Task<AdminResumeIndexViewModel> GetResumesAsync(AdminResumeFilterViewModel filter)
    {
        filter ??= new AdminResumeFilterViewModel();

        var query = _context.Resumes
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.CandidateProfile)
            .ThenInclude(x => x!.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(search) ||
                (x.CandidateProfile != null &&
                    x.CandidateProfile.User != null &&
                    x.CandidateProfile.User.FullName.ToLower().Contains(search)) ||
                (x.Category != null && x.Category.Name.ToLower().Contains(search)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.IsPublished.HasValue)
        {
            query = query.Where(x => x.IsPublished == filter.IsPublished.Value);
        }

        var items = await query
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

        return new AdminResumeIndexViewModel
        {
            Filter = filter,
            Items = items
        };
    }

    public async Task<ResumeDetailsViewModel?> GetResumeDetailsAsync(int resumeId)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(x => x.CandidateProfile)
            .ThenInclude(x => x!.User)
            .Include(x => x.Category)
            .Include(x => x.ResumeSkills)
            .ThenInclude(x => x.Skill)
            .FirstOrDefaultAsync(x => x.Id == resumeId);

        if (resume is null)
        {
            return null;
        }

        return new ResumeDetailsViewModel
        {
            Id = resume.Id,
            Title = resume.Title,
            DesiredPosition = resume.DesiredPosition,
            City = resume.CandidateProfile?.City ?? string.Empty,
            Summary = resume.Summary,
            Education = resume.Education,
            Experience = resume.Experience,
            SkillsDescription = resume.SkillsDescription,
            FullName = resume.CandidateProfile?.User?.FullName ?? string.Empty,
            CandidateUserId = resume.CandidateProfile?.UserId ?? string.Empty,
            CategoryName = resume.Category?.Name ?? string.Empty,
            EmploymentType = resume.EmploymentType,
            ExperienceYears = resume.ExperienceYears,
            ExperienceLevel = resume.ExperienceLevel,
            EducationLevel = resume.EducationLevel,
            DesiredSalary = resume.DesiredSalary,
            Status = resume.Status,
            IsPublished = resume.IsPublished,
            FilePath = resume.FilePath,
            OriginalFileName = resume.OriginalFileName,
            ContentType = resume.ContentType,
            FileSize = resume.FileSize,
            UploadedAt = resume.UploadedAt,
            SkillNames = resume.ResumeSkills
                .Select(x => x.Skill != null ? x.Skill.Name : string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList()
        };
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
