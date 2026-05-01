using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Resumes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace AisVacanciesAndResumes.Tests;

public class ResumeServiceTests
{
    [Fact]
    public async Task GetUserResumesAsync_ReturnsOnlyCurrentUserResumes()
    {
        await using var context = TestDbContextFactory.Create();
        context.CandidateProfiles.AddRange(
            new CandidateProfile { Id = 1, UserId = "candidate-1", Headline = "A", Summary = "A", City = "Kyiv" },
            new CandidateProfile { Id = 2, UserId = "candidate-2", Headline = "B", Summary = "B", City = "Lviv" });
        context.Categories.Add(new Category { Id = 1, Name = "Development" });
        context.Resumes.AddRange(
            new Resume { Id = 1, CandidateProfileId = 1, CategoryId = 1, Title = "Resume 1", Status = ResumeStatus.Published },
            new Resume { Id = 2, CandidateProfileId = 2, CategoryId = 1, Title = "Resume 2", Status = ResumeStatus.Draft });
        await context.SaveChangesAsync();

        var service = new ResumeService(context, CreateEnvironment());

        var result = await service.GetUserResumesAsync("candidate-1");

        Assert.Single(result);
        Assert.Equal("Resume 1", result[0].Title);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidFileExtension_ThrowsException()
    {
        await using var context = TestDbContextFactory.Create();
        context.CandidateProfiles.Add(new CandidateProfile
        {
            Id = 1,
            UserId = "candidate-1",
            Headline = "Candidate",
            Summary = "Summary",
            City = "Kyiv"
        });
        await context.SaveChangesAsync();

        var service = new ResumeService(context, CreateEnvironment());
        var formFile = CreateFormFile("resume.txt", "text/plain");

        var model = new ResumeFormViewModel
        {
            CategoryId = 1,
            Title = "My Resume",
            Summary = "Summary",
            ResumeFile = formFile
        };

        var action = () => service.CreateAsync("candidate-1", model);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task SearchPublishedResumesAsync_FiltersPublishedResumesForEmployer()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { Id = 1, Name = "Development" });
        context.Users.Add(new User { Id = "candidate-1", FullName = "Ivan Candidate", UserName = "candidate-1" });
        context.CandidateProfiles.Add(new CandidateProfile
        {
            Id = 1,
            UserId = "candidate-1",
            Headline = "Candidate",
            Summary = "Summary",
            City = "Kyiv"
        });
        context.Resumes.AddRange(
            new Resume
            {
                Id = 1,
                CandidateProfileId = 1,
                CategoryId = 1,
                Title = "Published Resume",
                DesiredPosition = "Analyst",
                Status = ResumeStatus.Published,
                IsPublished = true,
                EmploymentType = EmploymentType.FullTime,
                ExperienceLevel = ExperienceLevel.Middle,
                EducationLevel = EducationLevel.Bachelor,
                DesiredSalary = 30000
            },
            new Resume
            {
                Id = 2,
                CandidateProfileId = 1,
                CategoryId = 1,
                Title = "Draft Resume",
                DesiredPosition = "Manager",
                Status = ResumeStatus.Draft,
                IsPublished = false
            },
            new Resume
            {
                Id = 3,
                CandidateProfileId = 1,
                CategoryId = 1,
                Title = "Wrong Flag Resume",
                DesiredPosition = "Analyst",
                Status = ResumeStatus.Published,
                IsPublished = false
            },
            new Resume
            {
                Id = 4,
                CandidateProfileId = 1,
                CategoryId = 1,
                Title = "Archived Resume",
                DesiredPosition = "Analyst",
                Status = ResumeStatus.Archived,
                IsPublished = true
            });
        await context.SaveChangesAsync();

        var service = new ResumeService(context, CreateEnvironment());

        var result = await service.SearchPublishedResumesAsync(new ResumeSearchFilterViewModel
        {
            DesiredPosition = "Analyst"
        });

        Assert.Single(result.Items);
        Assert.Equal("Published Resume", result.Items[0].Title);
    }

    private static IWebHostEnvironment CreateEnvironment()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootPath);

        return new StubWebHostEnvironment
        {
            WebRootPath = rootPath
        };
    }

    private static IFormFile CreateFormFile(string fileName, string contentType)
    {
        var stream = new MemoryStream("test"u8.ToArray());
        return new FormFile(stream, 0, stream.Length, "ResumeFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
