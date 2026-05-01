using AisVacanciesAndResumes.ViewModels.Notifications;

namespace AisVacanciesAndResumes.Services;

public interface INotificationService
{
    Task<List<NotificationListItemViewModel>> GetUserNotificationsAsync(string userId);
    Task<NotificationDetailsViewModel?> GetDetailsAsync(string userId, int id);
    Task MarkAsReadAsync(string userId, int id);
    Task<int> GetUnreadCountAsync(string userId);
}
