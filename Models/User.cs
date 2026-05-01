using Microsoft.AspNetCore.Identity;

namespace AisVacanciesAndResumes.Models;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string StatusComment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CandidateProfile? CandidateProfile { get; set; }
    public EmployerProfile? EmployerProfile { get; set; }

    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<SavedSearch> SavedSearches { get; set; } = new List<SavedSearch>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<ModerationLog> ModerationLogs { get; set; } = new List<ModerationLog>();
}
