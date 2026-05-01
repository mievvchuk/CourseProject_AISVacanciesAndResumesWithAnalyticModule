using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Models;

public class CandidateProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public int ExperienceYears { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Junior;
    public EducationLevel EducationLevel { get; set; } = EducationLevel.Bachelor;
    public EmploymentType DesiredEmploymentType { get; set; } = EmploymentType.FullTime;
    public decimal? DesiredSalary { get; set; }

    public User? User { get; set; }
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();
}
