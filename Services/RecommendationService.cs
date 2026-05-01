using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.Recommendations;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMatchingService _matchingService;

    public RecommendationService(ApplicationDbContext context, IMatchingService matchingService)
    {
        _context = context;
        _matchingService = matchingService;
    }

    public async Task<RecommendationIndexViewModel> GetRecommendationsAsync(string userId, int take = 10)
    {
        var resumes = await _context.Resumes
            .AsNoTracking()
            .Where(x => x.CandidateProfile != null && x.CandidateProfile.UserId == userId)
            .OrderByDescending(x => x.IsPublished)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync();

        var model = new RecommendationIndexViewModel
        {
            HasResumes = resumes.Count > 0,
            ResumeCount = resumes.Count
        };

        if (resumes.Count == 0)
        {
            return model;
        }

        var vacancies = await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.EmployerProfile)
            .Include(x => x.Applications)
            .Where(x => x.IsActive && x.Status == VacancyStatus.Published)
            .ToListAsync();

        var recommendations = new List<RecommendedVacancyViewModel>();

        foreach (var vacancy in vacancies)
        {
            var bestResume = await FindBestResumeAsync(resumes, vacancy);
            if (bestResume is null)
            {
                continue;
            }

            recommendations.Add(new RecommendedVacancyViewModel
            {
                VacancyId = vacancy.Id,
                ResumeId = bestResume.ResumeId,
                ResumeTitle = bestResume.ResumeTitle,
                VacancyTitle = vacancy.Title,
                CompanyName = vacancy.EmployerProfile?.CompanyName ?? string.Empty,
                CategoryName = vacancy.Category?.Name ?? string.Empty,
                City = vacancy.EmployerProfile?.City ?? string.Empty,
                SalaryFrom = vacancy.SalaryFrom,
                SalaryTo = vacancy.SalaryTo,
                EmploymentType = vacancy.EmploymentType,
                ExperienceLevel = vacancy.ExperienceLevel,
                MatchPercentage = bestResume.MatchPercentage,
                HasApplied = vacancy.Applications.Any(x => x.CandidateUserId == userId)
            });
        }

        model.Items = recommendations
            .OrderByDescending(x => x.MatchPercentage)
            .ThenByDescending(x => x.SalaryTo)
            .ThenBy(x => x.VacancyTitle)
            .Take(take)
            .ToList();

        return model;
    }

    private async Task<ResumeMatchResult?> FindBestResumeAsync(IEnumerable<Resume> resumes, Vacancy vacancy)
    {
        ResumeMatchResult? bestResult = null;

        foreach (var resume in resumes)
        {
            var matchPercentage = await _matchingService.CalculateMatchPercentageAsync(resume.Id, vacancy.Id);
            if (bestResult is null || matchPercentage > bestResult.MatchPercentage)
            {
                bestResult = new ResumeMatchResult
                {
                    ResumeId = resume.Id,
                    ResumeTitle = resume.Title,
                    MatchPercentage = matchPercentage
                };
            }
        }

        return bestResult;
    }

    private sealed class ResumeMatchResult
    {
        public int ResumeId { get; set; }
        public string ResumeTitle { get; set; } = string.Empty;
        public int MatchPercentage { get; set; }
    }
}
