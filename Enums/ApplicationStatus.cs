using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.Enums;

public enum ApplicationStatus
{
    [Display(Name = "Нова")]
    New = 1,

    [Display(Name = "Переглянута")]
    Reviewed = 2,

    [Display(Name = "Прийнята")]
    Accepted = 3,

    [Display(Name = "Відхилена")]
    Rejected = 4
}
