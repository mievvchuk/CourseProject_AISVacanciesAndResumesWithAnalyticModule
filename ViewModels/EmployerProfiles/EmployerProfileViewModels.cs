using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.ViewModels.EmployerProfiles;

public class EmployerProfileDetailsViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanySize { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
}

public class EmployerProfileFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    public string Industry { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Company size")]
    public string CompanySize { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    [Display(Name = "Founded year")]
    public int? FoundedYear { get; set; }

    public string? LogoPath { get; set; }

    [Display(Name = "Company logo")]
    public IFormFile? LogoFile { get; set; }
}
