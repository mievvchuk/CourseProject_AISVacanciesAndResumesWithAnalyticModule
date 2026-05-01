namespace AisVacanciesAndResumes.Models;

public class PortfolioItem
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;

    public CandidateProfile? CandidateProfile { get; set; }
}
