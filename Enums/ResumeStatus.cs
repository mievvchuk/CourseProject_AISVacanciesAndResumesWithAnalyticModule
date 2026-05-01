using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.Enums;

public enum ResumeStatus
{
    [Display(Name = "Чернетка")]
    Draft = 1,

    [Display(Name = "Активне")]
    Published = 2,

    [Display(Name = "Архівне")]
    Archived = 3
}
