using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Models;

public class ModerationLog
{
    public int Id { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public ModerationActionType ActionType { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? AdminUser { get; set; }
}
