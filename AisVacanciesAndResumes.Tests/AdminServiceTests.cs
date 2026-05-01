using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;

namespace AisVacanciesAndResumes.Tests;

public class AdminServiceTests
{
    [Fact]
    public async Task ApproveVacancyAsync_AllowsOnlyUnderModerationVacancies()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { Id = 1, Name = "Розробка ПЗ" });
        context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = "employer-1", CompanyName = "AIS", City = "Kyiv" });
        context.Vacancies.AddRange(
            new Vacancy
            {
                Id = 1,
                EmployerProfileId = 1,
                CategoryId = 1,
                Title = "На модерації",
                Status = VacancyStatus.UnderModeration,
                IsActive = false
            },
            new Vacancy
            {
                Id = 2,
                EmployerProfileId = 1,
                CategoryId = 1,
                Title = "Активна",
                Status = VacancyStatus.Published,
                IsActive = true
            });
        await context.SaveChangesAsync();

        var service = new AdminService(context, userManager: null!);

        await service.ApproveVacancyAsync("admin-1", 1, null);
        var approved = await context.Vacancies.FindAsync(1);

        Assert.Equal(VacancyStatus.Published, approved!.Status);
        Assert.True(approved.IsActive);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveVacancyAsync("admin-1", 2, null));
    }
}
