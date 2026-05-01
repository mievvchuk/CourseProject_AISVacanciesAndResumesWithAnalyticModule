using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.SavedSearches;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Tests;

public class SavedSearchServiceTests
{
    [Fact]
    public async Task CreateAsync_SavesSearchWithFilters()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new SavedSearchService(context);

        var model = new SavedSearchFormViewModel
        {
            Query = ".NET",
            City = "Kyiv",
            EmploymentType = EmploymentType.Remote,
            ExperienceLevel = ExperienceLevel.Middle
        };

        await service.CreateAsync("candidate-1", model);

        var savedSearch = await context.SavedSearches.SingleAsync();
        Assert.Equal(".NET", savedSearch.Query);
        Assert.Equal("Kyiv", savedSearch.City);
        Assert.Equal(EmploymentType.Remote, savedSearch.EmploymentType);
        Assert.Equal(ExperienceLevel.Middle, savedSearch.ExperienceLevel);
    }

    [Fact]
    public async Task GetSavedFilterAsync_ReturnsOnlyCurrentUserFilter()
    {
        await using var context = TestDbContextFactory.Create();
        context.SavedSearches.AddRange(
            new SavedSearch { Id = 1, UserId = "candidate-1", Query = ".NET", City = "Kyiv" },
            new SavedSearch { Id = 2, UserId = "candidate-2", Query = "QA", City = "Lviv" });
        await context.SaveChangesAsync();

        var service = new SavedSearchService(context);

        var result = await service.GetSavedFilterAsync("candidate-1", 2);

        Assert.Null(result);
    }

}
