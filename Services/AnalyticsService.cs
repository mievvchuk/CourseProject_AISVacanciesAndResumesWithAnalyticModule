using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Extensions;
using AisVacanciesAndResumes.ViewModels.Analytics;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsDashboardViewModel> GetDashboardAsync(string? userId, bool isAdmin)
    {
        var vacancyQuery = _context.Vacancies
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin && !string.IsNullOrWhiteSpace(userId))
        {
            vacancyQuery = vacancyQuery.Where(x => x.EmployerProfile != null && x.EmployerProfile.UserId == userId);
        }

        var vacancyIds = await vacancyQuery
            .Select(x => x.Id)
            .ToListAsync();

        var vacanciesByCategory = await vacancyQuery
            .Include(x => x.Category)
            .GroupBy(x => x.Category != null ? x.Category.Name : "Without category")
            .Select(x => new CategoryStatisticViewModel
            {
                CategoryName = x.Key,
                Count = x.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var vacancyEmploymentTypeItems = await vacancyQuery
            .GroupBy(x => x.EmploymentType)
            .Select(x => new
            {
                EmploymentType = x.Key,
                Count = x.Count()
            })
            .ToListAsync();

        var vacancyEmploymentTypeDistribution = vacancyEmploymentTypeItems
            .Select(x => new EmploymentTypeStatisticViewModel
            {
                EmploymentTypeName = x.EmploymentType.GetDisplayName(),
                Count = x.Count
            })
            .OrderBy(x => x.EmploymentTypeName)
            .ToList();

        var popularSkills = await _context.VacancySkills
            .AsNoTracking()
            .Include(x => x.Skill)
            .Where(x => vacancyIds.Contains(x.VacancyId))
            .GroupBy(x => x.Skill != null ? x.Skill.Name : string.Empty)
            .Select(x => new SkillStatisticViewModel
            {
                SkillName = x.Key,
                UsageCount = x.Count()
            })
            .OrderByDescending(x => x.UsageCount)
            .Take(5)
            .ToListAsync();

        var salaryItems = await vacancyQuery
            .Where(x => x.SalaryFrom > 0 && x.SalaryTo > 0)
            .Select(x => new { x.SalaryFrom, x.SalaryTo })
            .ToListAsync();

        var averageSalary = salaryItems.Count == 0
            ? 0m
            : salaryItems.Average(x => (x.SalaryFrom + x.SalaryTo) / 2m);

        var applicationQuery = _context.Applications
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin && !string.IsNullOrWhiteSpace(userId))
        {
            applicationQuery = applicationQuery.Where(x => x.Vacancy != null && x.Vacancy.EmployerProfile != null && x.Vacancy.EmployerProfile.UserId == userId);
        }

        var resumeQuery = _context.Resumes
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin && !string.IsNullOrWhiteSpace(userId))
        {
            var employerResumeIds = await applicationQuery
                .Select(x => x.ResumeId)
                .Distinct()
                .ToListAsync();

            resumeQuery = resumeQuery.Where(x => employerResumeIds.Contains(x.Id));
        }

        var resumeExperienceItems = await resumeQuery
            .GroupBy(x => x.ExperienceLevel)
            .Select(x => new
            {
                ExperienceLevel = x.Key,
                Count = x.Count()
            })
            .ToListAsync();

        var resumeExperienceDistribution = resumeExperienceItems
            .Select(x => new ExperienceStatisticViewModel
            {
                ExperienceLevelName = x.ExperienceLevel.GetDisplayName(),
                Count = x.Count
            })
            .OrderBy(x => x.ExperienceLevelName)
            .ToList();

        var averageMatchPercentage = await applicationQuery
            .Select(x => (decimal?)x.MatchingPercent)
            .AverageAsync() ?? 0m;

        var candidateProfileQuery = _context.CandidateProfiles
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin && !string.IsNullOrWhiteSpace(userId))
        {
            candidateProfileQuery = candidateProfileQuery.Where(x =>
                x.Resumes.Any(r => r.Applications.Any(a =>
                    a.Vacancy != null &&
                    a.Vacancy.EmployerProfile != null &&
                    a.Vacancy.EmployerProfile.UserId == userId)));
        }

        var candidateExperienceItems = await candidateProfileQuery
            .GroupBy(x => x.ExperienceLevel)
            .Select(x => new
            {
                ExperienceLevel = x.Key,
                Count = x.Count()
            })
            .ToListAsync();

        var candidateExperienceDistribution = candidateExperienceItems
            .Select(x => new ExperienceStatisticViewModel
            {
                ExperienceLevelName = x.ExperienceLevel.GetDisplayName(),
                Count = x.Count
            })
            .OrderBy(x => x.ExperienceLevelName)
            .ToList();

        return new AnalyticsDashboardViewModel
        {
            VacancyCount = await vacancyQuery.CountAsync(),
            ResumeCount = await resumeQuery.CountAsync(),
            ApplicationCount = await applicationQuery.CountAsync(),
            UserCount = isAdmin
                ? await _context.Users.CountAsync()
                : await applicationQuery.Select(x => x.CandidateUserId).Distinct().CountAsync(),
            AverageSalary = Math.Round(averageSalary, 2),
            AverageMatchPercentage = Math.Round(averageMatchPercentage, 2),
            ActiveVacancyCount = await vacancyQuery.CountAsync(x => x.IsActive && x.Status == VacancyStatus.Published),
            ClosedVacancyCount = await vacancyQuery.CountAsync(x => x.Status == VacancyStatus.Closed),
            CandidateCount = isAdmin
                ? await _context.CandidateProfiles.CountAsync()
                : await applicationQuery.Select(x => x.CandidateUserId).Distinct().CountAsync(),
            EmployerCount = isAdmin ? await _context.EmployerProfiles.CountAsync() : 1,
            IsAdminView = isAdmin,
            VacanciesByCategory = vacanciesByCategory,
            CandidateExperienceDistribution = candidateExperienceDistribution,
            ResumeExperienceDistribution = resumeExperienceDistribution,
            VacancyEmploymentTypeDistribution = vacancyEmploymentTypeDistribution,
            PopularSkills = popularSkills
        };
    }
}
