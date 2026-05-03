namespace AisVacanciesAndResumes.ViewModels.Messages;

public class RealtimeMessageViewModel
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string ContentPreview { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SentAt { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public string DetailsUrl { get; set; } = string.Empty;
}
