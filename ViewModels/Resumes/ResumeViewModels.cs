using AisVacanciesAndResumes.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using AisVacanciesAndResumes.ViewModels.Portfolio;
namespace AisVacanciesAndResumes.ViewModels.Resumes;

public class ResumeDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DesiredPosition { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string SkillsDescription { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CandidateUserId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public int ExperienceYears { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public EducationLevel EducationLevel { get; set; }
    public decimal? DesiredSalary { get; set; }
    public ResumeStatus Status { get; set; }
    public bool IsPublished { get; set; }
    public string? FilePath { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public DateTime? UploadedAt { get; set; }
    public List<string> SkillNames { get; set; } = new();
    public List<PortfolioItemListItemViewModel> PortfolioItems { get; set; } = new();
}

public class ResumeFormViewModel
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }

    [Display(Name = "Категорія")]
    public int CategoryId { get; set; }

    [StringLength(80, ErrorMessage = "Категорія має містити не більше 80 символів")]
    [Display(Name = "Категорія")]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть назву резюме")]
    [Display(Name = "Назва резюме")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Бажана посада")]
    public string DesiredPosition { get; set; } = string.Empty;

    [Display(Name = "Короткий опис")]
    public string Summary { get; set; } = string.Empty;

    [Display(Name = "Освіта")]
    public string Education { get; set; } = string.Empty;

    [Display(Name = "Досвід")]
    public string Experience { get; set; } = string.Empty;

    [Display(Name = "Опис навичок")]
    public string SkillsDescription { get; set; } = string.Empty;

    [Display(Name = "Тип зайнятості")]
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

    [Range(0, 50, ErrorMessage = "Досвід має бути від 0 до 50 років")]
    [Display(Name = "Роки досвіду")]
    public int ExperienceYears { get; set; }

    [Display(Name = "Рівень досвіду")]
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Junior;

    [Display(Name = "Рівень освіти")]
    public EducationLevel EducationLevel { get; set; } = EducationLevel.Bachelor;

    [Display(Name = "Бажана зарплата")]
    public decimal? DesiredSalary { get; set; }

    [Display(Name = "Опубліковано")]
    public bool IsPublished { get; set; }

    [Display(Name = "Статус")]
    public ResumeStatus Status { get; set; } = ResumeStatus.Draft;

    [Display(Name = "Файл резюме")]
    public IFormFile? ResumeFile { get; set; }
    public bool ReplaceFieldsFromFile { get; set; }

    public string? FilePath { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public DateTime? UploadedAt { get; set; }

    public List<string> ParsedSkillNames { get; set; } = new();

    public string FullName { get; set; } = string.Empty;
    public List<SelectListItem> CategoryOptions { get; set; } = new();
}

public class ResumeListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DesiredPosition { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public int ExperienceYears { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public EducationLevel EducationLevel { get; set; }
    public decimal? DesiredSalary { get; set; }
    public ResumeStatus Status { get; set; }
    public string? FilePath { get; set; }
    public string? OriginalFileName { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Skills { get; set; } = new();
}

public class ResumeSearchFilterViewModel
{
    [Display(Name = "Бажана посада")]
    public string DesiredPosition { get; set; } = string.Empty;

    [Display(Name = "Категорія")]
    public int? CategoryId { get; set; }

    [Display(Name = "Місто")]
    public string City { get; set; } = string.Empty;

    [Display(Name = "Тип зайнятості")]
    public EmploymentType? EmploymentType { get; set; }

    [Display(Name = "Рівень досвіду")]
    public ExperienceLevel? ExperienceLevel { get; set; }

    [Display(Name = "Рівень освіти")]
    public EducationLevel? EducationLevel { get; set; }

    [Display(Name = "Зарплата від")]
    public decimal? DesiredSalaryFrom { get; set; }

    [Display(Name = "Зарплата до")]
    public decimal? DesiredSalaryTo { get; set; }

    [Display(Name = "Навички")]
    public string Skills { get; set; } = string.Empty;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ResumeSearchListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DesiredPosition { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public EducationLevel EducationLevel { get; set; }
    public decimal? DesiredSalary { get; set; }
    public List<string> Skills { get; set; } = new();
}

public class ResumeSearchViewModel
{
    public ResumeSearchFilterViewModel Filter { get; set; } = new();
    public List<ResumeSearchListItemViewModel> Items { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
