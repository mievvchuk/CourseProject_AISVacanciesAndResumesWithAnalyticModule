using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Tests;

public class MatchingServiceTests
{
    [Fact]
    public async Task CalculateMatchPercentageAsync_ReturnsExpectedScore()
    {
        await using var context = TestDbContextFactory.Create();

        context.CandidateProfiles.Add(new CandidateProfile { Id = 1, UserId = "candidate-1" });
        context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "Company" });
        context.Resumes.Add(new Resume
        {
            Id = 1,
            CandidateProfileId = 1,
            CategoryId = 1,
            Title = "Junior .NET Developer",
            DesiredPosition = "Junior .NET Developer",
            EmploymentType = EmploymentType.FullTime,
            ExperienceLevel = ExperienceLevel.Junior,
            DesiredSalary = 30000,
            ResumeSkills =
            [
                new ResumeSkill { ResumeId = 1, SkillId = 1 },
                new ResumeSkill { ResumeId = 1, SkillId = 2 }
            ]
        });

        context.Vacancies.Add(new Vacancy
        {
            Id = 1,
            EmployerProfileId = 1,
            CategoryId = 1,
            Title = ".NET Developer",
            SalaryFrom = 25000,
            SalaryTo = 40000,
            EmploymentType = EmploymentType.FullTime,
            ExperienceLevel = ExperienceLevel.Junior,
            VacancySkills =
            [
                new VacancySkill { VacancyId = 1, SkillId = 1 },
                new VacancySkill { VacancyId = 1, SkillId = 2 },
                new VacancySkill { VacancyId = 1, SkillId = 3 }
            ]
        });

        await context.SaveChangesAsync();

        var service = new MatchingService(context);

        var result = await service.CalculateMatchPercentageAsync(1, 1);

        Assert.Equal(87, result);
    }

}
