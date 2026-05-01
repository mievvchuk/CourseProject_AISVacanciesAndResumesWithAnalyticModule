using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.ViewModels.Analytics;

namespace AisVacanciesAndResumes.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int UserCount { get; set; }
    public int VacancyCount { get; set; }
    public int ResumeCount { get; set; }
    public int ModerationLogCount { get; set; }
    public AnalyticsDashboardViewModel Analytics { get; set; } = new();
}

public class AdminResumeListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public ResumeStatus Status { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminVacancyListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public VacancyStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime PublishedAt { get; set; }
}

public class ModerationLogListItemViewModel
{
    public int Id { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public ModerationActionType ActionType { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
