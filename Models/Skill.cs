namespace AisVacanciesAndResumes.Models;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ResumeSkill> ResumeSkills { get; set; } = new List<ResumeSkill>();
    public ICollection<VacancySkill> VacancySkills { get; set; } = new List<VacancySkill>();
}
