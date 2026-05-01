namespace AisVacanciesAndResumes.ViewModels.Analytics;

public class AnalyticsDashboardViewModel
{
    public int VacancyCount { get; set; }
    public int ResumeCount { get; set; }
    public int ApplicationCount { get; set; }
    public int UserCount { get; set; }
    public int CandidateCount { get; set; }
    public int EmployerCount { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal AverageMatchPercentage { get; set; }
    public int ActiveVacancyCount { get; set; }
    public int ClosedVacancyCount { get; set; }
    public bool IsAdminView { get; set; }
    public List<CategoryStatisticViewModel> VacanciesByCategory { get; set; } = new();
    public List<ExperienceStatisticViewModel> CandidateExperienceDistribution { get; set; } = new();
    public List<ExperienceStatisticViewModel> ResumeExperienceDistribution { get; set; } = new();
    public List<EmploymentTypeStatisticViewModel> VacancyEmploymentTypeDistribution { get; set; } = new();
    public List<SkillStatisticViewModel> PopularSkills { get; set; } = new();
}

public class CategoryStatisticViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class EmploymentTypeStatisticViewModel
{
    public string EmploymentTypeName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ExperienceStatisticViewModel
{
    public string ExperienceLevelName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SkillStatisticViewModel
{
    public string SkillName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}
