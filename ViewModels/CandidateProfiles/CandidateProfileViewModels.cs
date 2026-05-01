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

    [Required]
    public string Headline { get; set; } = string.Empty;

    [Required]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    public string? PhotoPath { get; set; }

    [Display(Name = "Profile photo")]
    public IFormFile? PhotoFile { get; set; }

    [Range(0, 50)]
    [Display(Name = "Experience years")]
    public int ExperienceYears { get; set; }

    [Display(Name = "Experience level")]
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Junior;

    [Display(Name = "Education level")]
    public EducationLevel EducationLevel { get; set; } = EducationLevel.Bachelor;

    [Display(Name = "Desired employment type")]
    public EmploymentType DesiredEmploymentType { get; set; } = EmploymentType.FullTime;

    [Display(Name = "Desired salary")]
    public decimal? DesiredSalary { get; set; }
}
