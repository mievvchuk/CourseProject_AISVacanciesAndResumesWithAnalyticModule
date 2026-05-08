using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.ViewModels.Portfolio;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.ViewModels.CandidateProfiles;

public class CandidateProfileDetailsViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public int ExperienceYears { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public EducationLevel EducationLevel { get; set; }
    public decimal? DesiredSalary { get; set; }
    public List<PortfolioItemListItemViewModel> PortfolioItems { get; set; } = new();
}

public class CandidateProfileFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Професійна роль")]
    [Required(ErrorMessage = "Вкажіть професійну роль")]
    public string Headline { get; set; } = string.Empty;

    [Display(Name = "Короткий опис")]
    [Required(ErrorMessage = "Напишіть короткий опис")]
    public string Summary { get; set; } = string.Empty;

    [Display(Name = "Місто")]
    [Required(ErrorMessage = "Вкажіть місто")]
    public string City { get; set; } = string.Empty;

    public string? PhotoPath { get; set; }

    [Display(Name = "Фото профілю")]
    public IFormFile? PhotoFile { get; set; }

    [Range(0, 50)]
    [Display(Name = "Роки досвіду")]
    public int ExperienceYears { get; set; }

    [Display(Name = "Рівень досвіду")]
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Junior;

    [Display(Name = "Рівень освіти")]
    public EducationLevel EducationLevel { get; set; } = EducationLevel.Bachelor;

    [Display(Name = "Бажаний тип зайнятості")]
    public EmploymentType DesiredEmploymentType { get; set; } = EmploymentType.FullTime;

    [Display(Name = "Бажана зарплата")]
    public decimal? DesiredSalary { get; set; }
}
