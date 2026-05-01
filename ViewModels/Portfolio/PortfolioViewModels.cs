using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.ViewModels.Portfolio;

public class PortfolioItemDeleteViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

public class PortfolioItemFormViewModel
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Project URL")]
    public string Url { get; set; } = string.Empty;

    [Display(Name = "Image path")]
    public string ImagePath { get; set; } = string.Empty;
}

public class PortfolioItemListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}
