using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.ViewModels.Applications;

public class ApplicationListItemViewModel
{
    public int Id { get; set; }
    public int VacancyId { get; set; }
    public int ResumeId { get; set; }
    public string VacancyTitle { get; set; } = string.Empty;
    public string ResumeTitle { get; set; } = string.Empty;
    public string CandidateFullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int MatchingPercent { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApplicationViewModel
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public int VacancyId { get; set; }
    public string VacancyTitle { get; set; } = string.Empty;
    public string ResumeTitle { get; set; } = string.Empty;
    public string CandidateFullName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CoverLetter { get; set; } = string.Empty;
    public int MatchingPercent { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool CanUpdateStatus { get; set; }
}

public class MyApplicationsFilterViewModel
{
    public string VacancyTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public ApplicationStatus? Status { get; set; }
    public DateTime? SubmittedFrom { get; set; }
}

public class MyApplicationsIndexViewModel
{
    public MyApplicationsFilterViewModel Filter { get; set; } = new();
    public List<ApplicationListItemViewModel> Items { get; set; } = new();
}
