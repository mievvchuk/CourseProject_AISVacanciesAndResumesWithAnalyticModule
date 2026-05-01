using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.ViewModels.Notifications;

public class NotificationDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
