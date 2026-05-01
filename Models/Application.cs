using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Models;

public class Application
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public int VacancyId { get; set; }
    public string CandidateUserId { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public int MatchingPercent { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Resume? Resume { get; set; }
    public Vacancy? Vacancy { get; set; }
    public User? CandidateUser { get; set; }
}
