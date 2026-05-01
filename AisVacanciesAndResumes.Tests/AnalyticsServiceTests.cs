using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;

namespace AisVacanciesAndResumes.Tests;

public class AnalyticsServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_ForEmployer_ReturnsOnlyOwnVacancyAnalytics()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { Id = 1, Name = "Office" });
        context.EmployerProfiles.AddRange(
            new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "A", City = "Kyiv" },
            new EmployerProfile { Id = 2, UserId = "employer-2", CompanyName = "B", City = "Lviv" });
        context.CandidateProfiles.Add(new CandidateProfile { Id = 1, UserId = "candidate-1", Headline = "A", Summary = "A", City = "Kyiv" });
        context.Resumes.Add(new Resume
        {
            Id = 1,
            CandidateProfileId = 1,
            CategoryId = 1,
            Title = "Resume",
            Status = ResumeStatus.Published,
            IsPublished = true,
            ExperienceLevel = ExperienceLevel.Middle
        });
        context.Vacancies.AddRange(
            new Vacancy
            {
                Id = 1,
                EmployerProfileId = 1,
                CategoryId = 1,
                Title = "Own",
                SalaryFrom = 1000,
                SalaryTo = 2000,
                EmploymentType = EmploymentType.FullTime,
                Status = VacancyStatus.Published,
                IsActive = true
            },
            new Vacancy
            {
                Id = 2,
                EmployerProfileId = 2,
                CategoryId = 1,
                Title = "Other",
                SalaryFrom = 4000,
                SalaryTo = 6000,
                EmploymentType = EmploymentType.Remote,
                Status = VacancyStatus.Published,
                IsActive = true
            });
        context.Applications.Add(new Application
        {
            Id = 1,
            ResumeId = 1,
            VacancyId = 1,
            CandidateUserId = "candidate-1",
            MatchingPercent = 85,
            Status = ApplicationStatus.New
        });
        await context.SaveChangesAsync();

        var service = new AnalyticsService(context);

        var result = await service.GetDashboardAsync("employer-1", false);

        Assert.Equal(1, result.VacancyCount);
        Assert.Equal(1, result.ResumeCount);
        Assert.Equal(1, result.ApplicationCount);
        Assert.Single(result.VacancyEmploymentTypeDistribution);
        Assert.Equal("Повна зайнятість", result.VacancyEmploymentTypeDistribution[0].EmploymentTypeName);
        Assert.Equal("Middle", result.ResumeExperienceDistribution[0].ExperienceLevelName);
    }
}
