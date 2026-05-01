using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.ViewModels.Recommendations;

public class RecommendationIndexViewModel
{
    public bool HasResumes { get; set; }
    public int ResumeCount { get; set; }
    public List<RecommendedVacancyViewModel> Items { get; set; } = new();
}

public class RecommendedVacancyViewModel
{
    public int VacancyId { get; set; }
    public int ResumeId { get; set; }
    public string ResumeTitle { get; set; } = string.Empty;
    public string VacancyTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal SalaryFrom { get; set; }
    public decimal SalaryTo { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public int MatchPercentage { get; set; }
    public bool HasApplied { get; set; }
}
