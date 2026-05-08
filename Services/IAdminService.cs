using AisVacanciesAndResumes.ViewModels.Admin;
using AisVacanciesAndResumes.ViewModels.Resumes;

namespace AisVacanciesAndResumes.Services;

public interface IAdminService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();
    Task<AdminUserIndexViewModel> GetUsersAsync(AdminUserFilterViewModel filter);
    Task<AdminVacancyIndexViewModel> GetVacanciesAsync(AdminVacancyFilterViewModel filter);
    Task<AdminResumeIndexViewModel> GetResumesAsync(AdminResumeFilterViewModel filter);
    Task<ResumeDetailsViewModel?> GetResumeDetailsAsync(int resumeId);
    Task<List<ModerationLogListItemViewModel>> GetModerationLogsAsync();
    Task ApproveVacancyAsync(string adminUserId, int vacancyId, string? comment);
    Task RejectVacancyAsync(string adminUserId, int vacancyId, string comment);
    Task ApproveResumeAsync(string adminUserId, int resumeId, string? comment);
    Task RejectResumeAsync(string adminUserId, int resumeId, string comment);
    Task ActivateUserAsync(string adminUserId, string userId, string? comment);
    Task DeactivateUserAsync(string adminUserId, string userId, string? comment);
}
