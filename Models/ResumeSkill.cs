using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Models;

public class ResumeSkill
{
    public int ResumeId { get; set; }
    public int SkillId { get; set; }
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Intermediate;

    public Resume? Resume { get; set; }
    public Skill? Skill { get; set; }
}
