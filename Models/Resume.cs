using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Models;

public class Resume
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DesiredPosition { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string SkillsDescription { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public int ExperienceYears { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Junior;
    public EducationLevel EducationLevel { get; set; } = EducationLevel.Bachelor;
    public decimal? DesiredSalary { get; set; }
    public bool IsPublished { get; set; }
    public ResumeStatus Status { get; set; } = ResumeStatus.Draft;
    public string? FilePath { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public DateTime? UploadedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string ModerationComment { get; set; } = string.Empty;
    public DateTime? ModeratedAt { get; set; }
    public string? ModeratedByUserId { get; set; }

    public CandidateProfile? CandidateProfile { get; set; }
    public Category? Category { get; set; }
    public User? ModeratedByUser { get; set; }
    public ICollection<ResumeSkill> ResumeSkills { get; set; } = new List<ResumeSkill>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
