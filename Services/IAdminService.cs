using AisVacanciesAndResumes.ViewModels.Admin;

namespace AisVacanciesAndResumes.Services;

public interface IAdminService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();
    Task<List<AdminUserListItemViewModel>> GetUsersAsync();
    Task<List<AdminVacancyListItemViewModel>> GetVacanciesAsync();
    Task<List<AdminResumeListItemViewModel>> GetResumesAsync();
    Task<List<ModerationLogListItemViewModel>> GetModerationLogsAsync();
    Task ApproveVacancyAsync(string adminUserId, int vacancyId, string? comment);
    Task RejectVacancyAsync(string adminUserId, int vacancyId, string comment);
    Task ApproveResumeAsync(string adminUserId, int resumeId, string? comment);
    Task RejectResumeAsync(string adminUserId, int resumeId, string comment);
    Task ActivateUserAsync(string adminUserId, string userId, string? comment);
    Task DeactivateUserAsync(string adminUserId, string userId, string? comment);
}
