using AisVacanciesAndResumes.Enums;
using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.ViewModels.SavedSearches;

public class SavedSearchDeleteViewModel
{
    public int Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class SavedSearchFormViewModel
{
    [Display(Name = "Title")]
    public string Query { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Display(Name = "Employment type")]
    public EmploymentType? EmploymentType { get; set; }

    [Display(Name = "Experience level")]
    public ExperienceLevel? ExperienceLevel { get; set; }
}

public class SavedSearchListItemViewModel
{
    public int Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public EmploymentType? EmploymentType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Summary { get; set; } = string.Empty;
}
