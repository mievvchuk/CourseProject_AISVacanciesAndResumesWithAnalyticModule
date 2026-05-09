using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Tests;

public class VacancyServiceTests
{
    [Fact]
    public async Task SearchAsync_FiltersByCityAndEmploymentType()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { Id = 1, Name = "Development" });
        context.EmployerProfiles.AddRange(
            new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "AIS", City = "Kyiv" },
            new EmployerProfile { Id = 2, UserId = "employer-2", CompanyName = "Tech", City = "Lviv" });
        context.Vacancies.AddRange(
            new Vacancy
            {
                Id = 1,
                EmployerProfileId = 1,
                CategoryId = 1,
                Title = ".NET Developer",
                SalaryFrom = 20000,
                SalaryTo = 40000,
                EmploymentType = EmploymentType.FullTime,
                ExperienceLevel = ExperienceLevel.Junior,
                Status = VacancyStatus.Published,
                IsActive = true
            },
            new Vacancy
            {
                Id = 2,
                EmployerProfileId = 2,
                CategoryId = 1,
                Title = "QA Engineer",
                SalaryFrom = 15000,
                SalaryTo = 25000,
                EmploymentType = EmploymentType.Remote,
                ExperienceLevel = ExperienceLevel.Junior,
                Status = VacancyStatus.Published,
                IsActive = true
            });
        await context.SaveChangesAsync();

        var service = new VacancyService(context);
        var filter = new VacancyFilterViewModel
        {
            City = "Kyiv",
            EmploymentType = EmploymentType.FullTime
        };

        var result = await service.SearchAsync(filter);

        Assert.Single(result.Items);
        Assert.Equal(".NET Developer", result.Items[0].Title);
    }

    [Fact]
    public async Task GetDetailsModelAsync_ForCandidate_DoesNotReturnClosedVacancy()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { Id = 1, Name = "Development" });
        context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "AIS", City = "Kyiv" });
        context.Vacancies.Add(new Vacancy
        {
            Id = 1,
            EmployerProfileId = 1,
            CategoryId = 1,
            Title = "Closed Vacancy",
            SalaryFrom = 20000,
            SalaryTo = 30000,
            Status = VacancyStatus.Closed,
            IsActive = false
        });
        await context.SaveChangesAsync();

        var service = new VacancyService(context);

        var result = await service.GetDetailsModelAsync(1, "candidate-1", false);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_SetsEmployerVacancyToUnderModeration()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { Id = 1, Name = "Розробка ПЗ" });
        context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "AIS", City = "Kyiv" });
        await context.SaveChangesAsync();

        var service = new VacancyService(context);

        await service.CreateAsync("employer-1", new VacancyFormViewModel
        {
            CategoryId = 1,
            Title = "Junior .NET Developer",
            Description = "Опис вакансії",
            Requirements = "C#, SQL",
            SalaryFrom = 20000,
            SalaryTo = 40000,
            EmploymentType = EmploymentType.FullTime,
            ExperienceLevel = ExperienceLevel.Junior,
            Location = "Kyiv",
            ClosingDate = new DateTime(2026, 6, 1)
        });

        var vacancy = await context.Vacancies.SingleAsync();
        Assert.Equal(VacancyStatus.UnderModeration, vacancy.Status);
        Assert.False(vacancy.IsActive);
        Assert.Equal(DateTimeKind.Utc, vacancy.ClosingDate!.Value.Kind);
        Assert.Equal(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), vacancy.ClosingDate);
    }

}
