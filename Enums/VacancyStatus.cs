using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.Enums;

public enum VacancyStatus
{
    [Display(Name = "Чернетка")]
    Draft = 1,

    [Display(Name = "Активна")]
    Published = 2,

    [Display(Name = "Закрита")]
    Closed = 3,

    [Display(Name = "Архівована")]
    Archived = 4,

    [Display(Name = "На модерації")]
    UnderModeration = 5,

    [Display(Name = "Відхилена")]
    Rejected = 6
}
