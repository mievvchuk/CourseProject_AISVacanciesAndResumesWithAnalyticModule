using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.ViewModels.Api;

public class ApplicationApiDto
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public int VacancyId { get; set; }
    public string CandidateUserId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string ResumeTitle { get; set; } = string.Empty;
    public string ResumeCategory { get; set; } = string.Empty;
    public string VacancyTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public int MatchingPercent { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string EmployerUserId { get; set; } = string.Empty;
}

public class ApplicationCreatedApiDto
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public int VacancyId { get; set; }
    public int MatchingPercent { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApplyApplicationRequestDto
{
    public int ResumeId { get; set; }
    public int VacancyId { get; set; }
    public string? CoverLetter { get; set; }
}

public class CandidateProfileApiDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public EducationLevel EducationLevel { get; set; }
    public EmploymentType DesiredEmploymentType { get; set; }
    public decimal? DesiredSalary { get; set; }
}

public class CategoryApiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class EmployerProfileApiDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanySize { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int? FoundedYear { get; set; }
}

public class MessageApiDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}

public class NotificationApiDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PortfolioItemApiDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

public class ResumeApiDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DesiredPosition { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string SkillsDescription { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public int ExperienceYears { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public EducationLevel EducationLevel { get; set; }
    public decimal? DesiredSalary { get; set; }
    public ResumeStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Category { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}

public class SavedSearchApiDto
{
    public int Id { get; set; }
    public SearchType SearchType { get; set; }
    public string Query { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SkillApiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateApplicationStatusRequestDto
{
    public ApplicationStatus Status { get; set; }
}

public class VacancyApiDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SalaryFrom { get; set; }
    public decimal SalaryTo { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public VacancyStatus Status { get; set; }
    public DateTime PublishedAt { get; set; }
    public string Category { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}
