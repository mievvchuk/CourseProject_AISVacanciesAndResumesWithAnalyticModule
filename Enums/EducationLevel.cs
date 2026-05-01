using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.Enums;

public enum EducationLevel
{
    [Display(Name = "Середня")]
    Secondary = 1,

    [Display(Name = "Бакалавр")]
    Bachelor = 2,

    [Display(Name = "Магістр")]
    Master = 3,

    [Display(Name = "Доктор філософії")]
    PhD = 4
}
