using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.Enums;

public enum EmploymentType
{
    [Display(Name = "Повна зайнятість")]
    FullTime = 1,

    [Display(Name = "Часткова зайнятість")]
    PartTime = 2,

    [Display(Name = "Віддалена робота")]
    Remote = 3,

    [Display(Name = "Стажування")]
    Internship = 4,

    [Display(Name = "Контракт")]
    Contract = 5,

    [Display(Name = "Фриланс")]
    Freelance = 6,

    [Display(Name = "Гібридний формат")]
    Hybrid = 7
}
