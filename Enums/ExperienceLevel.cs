using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.Enums;

public enum ExperienceLevel
{
    [Display(Name = "Без досвіду")]
    NoExperience = 1,

    [Display(Name = "Junior")]
    Junior = 2,

    [Display(Name = "Middle")]
    Middle = 3,

    [Display(Name = "Senior")]
    Senior = 4
}
