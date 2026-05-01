using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.Enums;

public enum NotificationType
{
    [Display(Name = "Інформація")]
    Info = 1,

    [Display(Name = "Успішно")]
    Success = 2,

    [Display(Name = "Попередження")]
    Warning = 3,

    [Display(Name = "Помилка")]
    Error = 4
}
