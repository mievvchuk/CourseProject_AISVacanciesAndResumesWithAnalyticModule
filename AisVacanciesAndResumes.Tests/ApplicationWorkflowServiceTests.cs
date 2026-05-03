using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Tests;

public class ApplicationWorkflowServiceTests
{
    [Fact]
    public async Task ApplyAsync_CreatesApplicationAndNotification()
    {
        await using var context = TestDbContextFactory.Create();
        context.CandidateProfiles.Add(new CandidateProfile { Id = 1, UserId = "candidate-1" });
        context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "Company" });
        context.Resumes.Add(new Resume
        {
            Id = 1,
            CandidateProfileId = 1,
            CategoryId = 1,
            Title = "Resume",
            Status = ResumeStatus.Published,
            IsPublished = true
        });
        context.Vacancies.Add(new Vacancy { Id = 1, EmployerProfileId = 1, CategoryId = 1, Title = "Vacancy", SalaryFrom = 1000, SalaryTo = 2000, IsActive = true, Status = VacancyStatus.Published });
        await context.SaveChangesAsync();

        var service = new ApplicationWorkflowService(context, new StubMatchingService(75));

        await service.ApplyAsync(1, 1, "candidate-1", "My cover letter");

        var application = await context.Applications.SingleAsync();
        var notifications = await context.Notifications.ToListAsync();

        Assert.Equal(75, application.MatchingPercent);
        Assert.Equal(ApplicationStatus.New, application.Status);
        Assert.Contains(notifications, x => x.UserId == "candidate-1");
        Assert.Contains(notifications, x => x.UserId == "employer-1");
    }

    [Fact]
    public async Task ApplyAsync_RejectsResumeThatIsStillUnderModeration()
    {
        await using var context = TestDbContextFactory.Create();
        context.CandidateProfiles.Add(new CandidateProfile { Id = 1, UserId = "candidate-1" });
        context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "Company" });
        context.Resumes.Add(new Resume
        {
            Id = 1,
            CandidateProfileId = 1,
            CategoryId = 1,
            Title = "Resume",
            Status = ResumeStatus.UnderModeration,
            IsPublished = false
        });
        context.Vacancies.Add(new Vacancy
        {
            Id = 1,
            EmployerProfileId = 1,
            CategoryId = 1,
            Title = "Vacancy",
            SalaryFrom = 1000,
            SalaryTo = 2000,
            IsActive = true,
            Status = VacancyStatus.Published
        });
        await context.SaveChangesAsync();

        var service = new ApplicationWorkflowService(context, new StubMatchingService(75));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApplyAsync(1, 1, "candidate-1", "My cover letter"));
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesStatusAndCreatesNotification()
    {
        await using var context = TestDbContextFactory.Create();
        context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "Company" });
        context.Vacancies.Add(new Vacancy
        {
            Id = 1,
            EmployerProfileId = 1,
            CategoryId = 1,
            Title = "Vacancy",
            SalaryFrom = 1000,
            SalaryTo = 2000
        });

        context.Applications.Add(new Application
        {
            Id = 1,
            ResumeId = 1,
            VacancyId = 1,
            CandidateUserId = "candidate-1",
            Status = ApplicationStatus.New
        });

        await context.SaveChangesAsync();

        var service = new ApplicationWorkflowService(context, new StubMatchingService(60));

        await service.UpdateStatusAsync(1, ApplicationStatus.Accepted, "employer-1");

        var application = await context.Applications.SingleAsync();
        var notification = await context.Notifications.SingleAsync();
        var log = await context.ModerationLogs.SingleAsync();

        Assert.Equal(ApplicationStatus.Accepted, application.Status);
        Assert.Equal("candidate-1", notification.UserId);
        Assert.Equal("employer-1", log.AdminUserId);
    }

    private sealed class StubMatchingService : IMatchingService
    {
        private readonly int _result;

        public StubMatchingService(int result)
        {
            _result = result;
        }

        public Task<int> CalculateMatchPercentageAsync(int resumeId, int vacancyId)
        {
            return Task.FromResult(_result);
        }
    }
}
