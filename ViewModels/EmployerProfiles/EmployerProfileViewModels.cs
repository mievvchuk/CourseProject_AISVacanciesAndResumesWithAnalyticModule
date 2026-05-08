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

    [Required(ErrorMessage = "Вкажіть назву компанії")]
    [Display(Name = "Назва компанії")]
    public string CompanyName { get; set; } = string.Empty;

    [Display(Name = "Галузь")]
    public string Industry { get; set; } = string.Empty;

    [Required(ErrorMessage = "Опишіть компанію")]
    [Display(Name = "Опис компанії")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Розмір компанії")]
    public string CompanySize { get; set; } = string.Empty;

    [Display(Name = "Сайт")]
    public string Website { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть місто")]
    [Display(Name = "Місто")]
    public string City { get; set; } = string.Empty;

    [Display(Name = "Локація")]
    public string Location { get; set; } = string.Empty;

    [Display(Name = "Рік заснування")]
    public int? FoundedYear { get; set; }

    public string? LogoPath { get; set; }

    [Display(Name = "Логотип компанії")]
    public IFormFile? LogoFile { get; set; }
}
