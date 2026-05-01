using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Models;

public class Vacancy
{
    public int Id { get; set; }
    public int EmployerProfileId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public decimal SalaryFrom { get; set; }
    public decimal SalaryTo { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Junior;
    public VacancyStatus Status { get; set; } = VacancyStatus.Published;
    public bool IsActive { get; set; } = true;
    public string Location { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosingDate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string ModerationComment { get; set; } = string.Empty;
    public DateTime? ModeratedAt { get; set; }
    public string? ModeratedByUserId { get; set; }

    public EmployerProfile? EmployerProfile { get; set; }
    public Category? Category { get; set; }
    public User? ModeratedByUser { get; set; }
    public ICollection<VacancySkill> VacancySkills { get; set; } = new List<VacancySkill>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
