using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.Enums;

public enum ExperienceLevel
{
    [Display(Name = "Без досвіду")]
    NoExperience = 1,

    [Display(Name = "Початковий")]
    Junior = 2,

    [Display(Name = "Середній")]
    Middle = 3,

    [Display(Name = "Досвідчений")]
    Senior = 4
}
