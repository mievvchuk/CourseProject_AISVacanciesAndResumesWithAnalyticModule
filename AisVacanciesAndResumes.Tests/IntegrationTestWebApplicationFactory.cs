using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AisVacanciesAndResumes.Tests;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"integration-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Testing:DatabaseName"] = _databaseName
            });
        });

    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        SeedDatabase(scope.ServiceProvider);
        return host;
    }

    private static void SeedDatabase(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in Enum.GetNames<UserRoleType>())
        {
            if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
            }
        }

        var userManager = services.GetRequiredService<UserManager<User>>();
        var admin = CreateUser(userManager, "admin.integration@example.com", "Admin123", "Integration Admin", UserRoleType.Admin);
        var candidate = CreateUser(userManager, "candidate.integration@example.com", "Candidate123", "Integration Candidate", UserRoleType.Candidate);
        var employer = CreateUser(userManager, "employer.integration@example.com", "Employer123", "Integration Employer", UserRoleType.Employer);

        var category = new Category { Id = 1, Name = "Software Development" };
        var skill = new Skill { Id = 1, Name = "ASP.NET Core", Category = "Technical" };
        var candidateProfile = new CandidateProfile
        {
            Id = 1,
            UserId = candidate.Id,
            Headline = "QA candidate",
            Summary = "Tests the full application.",
            City = "Kyiv",
            ExperienceYears = 2,
            ExperienceLevel = ExperienceLevel.Middle,
            EducationLevel = EducationLevel.Bachelor,
            DesiredEmploymentType = EmploymentType.FullTime,
            DesiredSalary = 1800
        };
        var employerProfile = new EmployerProfile
        {
            Id = 1,
            UserId = employer.Id,
            CompanyName = "Integration Labs",
            Industry = "IT",
            Description = "Builds testable products.",
            CompanySize = "11-50",
            Website = "https://example.com",
            City = "Kyiv",
            Location = "Kyiv"
        };

        context.Categories.Add(category);
        context.Skills.Add(skill);
        context.CandidateProfiles.Add(candidateProfile);
        context.EmployerProfiles.Add(employerProfile);
        context.SaveChanges();

        var publishedVacancy = new Vacancy
        {
            Id = 1,
            EmployerProfileId = employerProfile.Id,
            CategoryId = category.Id,
            Title = "Integration QA Engineer",
            Description = "Owns integration and e2e coverage.",
            Requirements = "ASP.NET Core, HTTP testing",
            SalaryFrom = 1200,
            SalaryTo = 2400,
            EmploymentType = EmploymentType.FullTime,
            ExperienceLevel = ExperienceLevel.Middle,
            Status = VacancyStatus.Published,
            IsActive = true,
            Location = "Kyiv",
            PublishedAt = DateTime.UtcNow
        };

        var hiddenVacancy = new Vacancy
        {
            Id = 2,
            EmployerProfileId = employerProfile.Id,
            CategoryId = category.Id,
            Title = "Hidden Draft Vacancy",
            Description = "Should not be returned by public API.",
            Requirements = "Private",
            SalaryFrom = 100,
            SalaryTo = 200,
            EmploymentType = EmploymentType.Remote,
            ExperienceLevel = ExperienceLevel.Junior,
            Status = VacancyStatus.Draft,
            IsActive = false,
            Location = "Remote",
            PublishedAt = DateTime.UtcNow.AddDays(-1)
        };

        context.Vacancies.AddRange(publishedVacancy, hiddenVacancy);
        context.SaveChanges();

        var resume = new Resume
        {
            Id = 1,
            CandidateProfileId = candidateProfile.Id,
            CategoryId = category.Id,
            Title = "Integration Resume",
            DesiredPosition = "QA Engineer",
            Summary = "Covers integration scenarios.",
            Education = "Bachelor",
            Experience = "Two years of QA work.",
            SkillsDescription = "ASP.NET Core",
            EmploymentType = EmploymentType.FullTime,
            ExperienceYears = 2,
            ExperienceLevel = ExperienceLevel.Middle,
            EducationLevel = EducationLevel.Bachelor,
            DesiredSalary = 2000,
            IsPublished = true,
            Status = ResumeStatus.Published
        };

        context.Resumes.Add(resume);
        context.SaveChanges();

        context.VacancySkills.Add(new VacancySkill
        {
            VacancyId = publishedVacancy.Id,
            SkillId = skill.Id,
            SkillLevel = SkillLevel.Intermediate
        });
        context.ResumeSkills.Add(new ResumeSkill
        {
            ResumeId = resume.Id,
            SkillId = skill.Id,
            SkillLevel = SkillLevel.Intermediate
        });
        context.Applications.Add(new Application
        {
            Id = 1,
            ResumeId = resume.Id,
            VacancyId = publishedVacancy.Id,
            CandidateUserId = candidate.Id,
            CoverLetter = "I can test this application.",
            MatchingPercent = 88,
            Status = ApplicationStatus.New
        });
        context.SavedSearches.Add(new SavedSearch
        {
            Id = 1,
            UserId = candidate.Id,
            SearchType = SearchType.Vacancies,
            Query = "QA",
            City = "Kyiv",
            CategoryId = category.Id,
            EmploymentType = EmploymentType.FullTime,
            ExperienceLevel = ExperienceLevel.Middle
        });
        context.PortfolioItems.Add(new PortfolioItem
        {
            Id = 1,
            CandidateProfileId = candidateProfile.Id,
            Title = "Integration Portfolio",
            Description = "Portfolio item used in smoke tests.",
            Url = "https://example.com",
            ImagePath = string.Empty
        });
        context.Notifications.AddRange(
            new Notification { Id = 1, UserId = candidate.Id, Title = "Candidate notification", Content = "Smoke notification", Type = NotificationType.Info },
            new Notification { Id = 2, UserId = employer.Id, Title = "Employer notification", Content = "Smoke notification", Type = NotificationType.Info },
            new Notification { Id = 3, UserId = admin.Id, Title = "Admin notification", Content = "Smoke notification", Type = NotificationType.Info });
        context.Messages.Add(new Message
        {
            Id = 1,
            SenderId = employer.Id,
            ReceiverId = candidate.Id,
            Subject = "Smoke message",
            Content = "This message verifies inbox and details pages."
        });
        context.ModerationLogs.Add(new ModerationLog
        {
            Id = 1,
            AdminUserId = admin.Id,
            EntityName = nameof(Vacancy),
            EntityId = publishedVacancy.Id,
            ActionType = ModerationActionType.Approved,
            Note = "Seeded moderation log."
        });
        context.SaveChanges();
    }

    private static User CreateUser(
        UserManager<User> userManager,
        string email,
        string password,
        string fullName,
        UserRoleType role)
    {
        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            IsActive = true
        };

        var createResult = userManager.CreateAsync(user, password).GetAwaiter().GetResult();
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(x => x.Description)));
        }

        var roleResult = userManager.AddToRoleAsync(user, role.ToString()).GetAwaiter().GetResult();
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(x => x.Description)));
        }

        return user;
    }
}
