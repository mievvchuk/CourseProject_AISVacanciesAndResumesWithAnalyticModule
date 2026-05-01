using AisVacanciesAndResumes.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.ViewModels.Vacancies;

public class VacancyDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal SalaryFrom { get; set; }
    public decimal SalaryTo { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public VacancyStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime? ClosingDate { get; set; }
    public bool CanManage { get; set; }
    public bool HasApplied { get; set; }
    public List<string> SkillNames { get; set; } = new();
    public List<SelectListItem> ResumeOptions { get; set; } = new();
}

public class VacancyFilterViewModel
{
    public string Title { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
}

public class VacancyFormViewModel
{
    public int Id { get; set; }
    public int EmployerProfileId { get; set; }

    [Required(ErrorMessage = "Оберіть категорію")]
    [Display(Name = "Категорія")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Вкажіть назву вакансії")]
    [Display(Name = "Назва вакансії")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Додайте опис вакансії")]
    [Display(Name = "Опис")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Вимоги")]
    public string Requirements { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "Зарплата має бути в межах від 0 до 1 000 000")]
    [Display(Name = "Зарплата від")]
    public decimal SalaryFrom { get; set; }

    [Range(0, 1000000, ErrorMessage = "Зарплата має бути в межах від 0 до 1 000 000")]
    [Display(Name = "Зарплата до")]
    public decimal SalaryTo { get; set; }

    [Display(Name = "Тип зайнятості")]
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

    [Display(Name = "Рівень досвіду")]
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Junior;

    [Display(Name = "Статус")]
    public VacancyStatus Status { get; set; } = VacancyStatus.UnderModeration;

    [Display(Name = "Активна")]
    public bool IsActive { get; set; }

    [Display(Name = "Локація")]
    public string Location { get; set; } = string.Empty;

    [Display(Name = "Актуальна до")]
    public DateTime? ClosingDate { get; set; }

    [Display(Name = "Навички зі списку")]
    public List<int> SelectedSkillIds { get; set; } = new();

    [Display(Name = "Додаткові навички")]
    public string SkillsText { get; set; } = string.Empty;

    public List<string> SelectedSkillNames { get; set; } = new();

    public string CompanyName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<SelectListItem> SkillOptions { get; set; } = new();
}

public class VacancyIndexViewModel
{
    public VacancyFilterViewModel Filter { get; set; } = new();
    public List<VacancyListItemViewModel> Items { get; set; } = new();
    public List<SelectListItem> ResumeOptions { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
}

public class VacancyListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal SalaryFrom { get; set; }
    public decimal SalaryTo { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public VacancyStatus Status { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool CanManage { get; set; }
    public bool HasApplied { get; set; }
    public List<string> Skills { get; set; } = new();
}
