using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Models;

public class SavedSearch
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public SearchType SearchType { get; set; } = SearchType.Vacancies;
    public string Query { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Category? Category { get; set; }
}
