using AisVacanciesAndResumes.ViewModels.Analytics;

namespace AisVacanciesAndResumes.Services;

public interface IAnalyticsService
{
    Task<AnalyticsDashboardViewModel> GetDashboardAsync(string? userId, bool isAdmin);
}
