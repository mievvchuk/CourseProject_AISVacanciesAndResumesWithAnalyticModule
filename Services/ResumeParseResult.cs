namespace AisVacanciesAndResumes.Services;

public class ResumeParseResult
{
    public string ExtractedText { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string SkillsDescription { get; set; } = string.Empty;
    public List<string> ParsedSkillNames { get; set; } = new();
    public string DesiredPosition { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int? YearsOfExperience { get; set; }
    public decimal? DesiredSalary { get; set; }
    public Enums.EducationLevel? EducationLevel { get; set; }
    public Enums.EmploymentType? EmploymentType { get; set; }
}
