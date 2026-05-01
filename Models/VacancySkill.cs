using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Models;

public class VacancySkill
{
    public int VacancyId { get; set; }
    public int SkillId { get; set; }
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Intermediate;

    public Vacancy? Vacancy { get; set; }
    public Skill? Skill { get; set; }
}
