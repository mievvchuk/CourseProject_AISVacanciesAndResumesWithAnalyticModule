using AisVacanciesAndResumes.Data;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class MatchingService : IMatchingService
{
    private readonly ApplicationDbContext _context;

    public MatchingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CalculateMatchPercentageAsync(int resumeId, int vacancyId)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(x => x.ResumeSkills)
            .Include(x => x.CandidateProfile)
            .FirstOrDefaultAsync(x => x.Id == resumeId);

        var vacancy = await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.VacancySkills)
            .Include(x => x.EmployerProfile)
            .FirstOrDefaultAsync(x => x.Id == vacancyId);

        if (resume is null || vacancy is null)
        {
            return 0;
        }

        var score = 0.0;

        var vacancySkillIds = vacancy.VacancySkills.Select(x => x.SkillId).ToHashSet();
        if (vacancySkillIds.Count > 0)
        {
            var resumeSkills = resume.ResumeSkills.ToDictionary(x => x.SkillId);
            var skillScore = 0.0;

            foreach (var vacancySkill in vacancy.VacancySkills)
            {
                if (!resumeSkills.TryGetValue(vacancySkill.SkillId, out var resumeSkill))
                {
                    continue;
                }

                var levelRatio = Math.Min(1.0, (double)resumeSkill.SkillLevel / (double)vacancySkill.SkillLevel);
                skillScore += levelRatio;
            }

            score += skillScore / vacancySkillIds.Count * 40;
        }

        if (resume.CategoryId == vacancy.CategoryId)
        {
            score += 15;
        }

        if (resume.EmploymentType == vacancy.EmploymentType)
        {
            score += 10;
        }

        if (resume.ExperienceLevel >= vacancy.ExperienceLevel)
        {
            score += 10;
        }
        else if ((int)vacancy.ExperienceLevel - (int)resume.ExperienceLevel == 1)
        {
            score += 5;
        }

        if (!resume.DesiredSalary.HasValue || resume.DesiredSalary.Value <= vacancy.SalaryTo)
        {
            score += 15;
        }
        else if (resume.DesiredSalary.Value <= vacancy.SalaryTo * 1.15m)
        {
            score += 7;
        }

        if (HasPositionMatch(resume.DesiredPosition, vacancy.Title))
        {
            score += 10;
        }

        return Math.Clamp((int)Math.Round(score), 0, 100);
    }

    private static bool HasPositionMatch(string desiredPosition, string vacancyTitle)
    {
        var desiredTerms = SplitTerms(desiredPosition);
        var titleTerms = SplitTerms(vacancyTitle);
        return desiredTerms.Count > 0 && desiredTerms.Intersect(titleTerms, StringComparer.OrdinalIgnoreCase).Any();
    }

    private static HashSet<string> SplitTerms(string value)
    {
        return value
            .Split([' ', ',', '.', ';', '-', '/', '\\', '(', ')'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 2)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
