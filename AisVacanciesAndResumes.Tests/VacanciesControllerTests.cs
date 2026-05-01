using AisVacanciesAndResumes.Controllers;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Resumes;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Tests;

public class VacanciesControllerTests
{
    [Fact]
    public async Task Index_ForCandidate_LoadsResumeOptions()
    {
        var vacancyService = new FakeVacancyService();
        var resumeService = new FakeResumeService();
        var controller = new VacanciesController(vacancyService, resumeService);
        controller.ControllerContext = CreateControllerContext(
            "candidate-1",
            new[] { new Claim(ClaimTypes.Role, "Candidate") });

        var result = await controller.Index(new VacancyFilterViewModel());

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<VacancyIndexViewModel>(viewResult.Model);
        Assert.Single(model.ResumeOptions);
        Assert.Equal("candidate-1", resumeService.RequestedUserId);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenVacancyDoesNotExist()
    {
        var controller = new VacanciesController(new FakeVacancyService(), new FakeResumeService());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.Details(99);

        Assert.IsType<NotFoundResult>(result);
    }

    private static ControllerContext CreateControllerContext(string userId, IEnumerable<Claim> additionalClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        claims.AddRange(additionalClaims);

        var identity = new ClaimsIdentity(claims, "TestAuth");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    private sealed class FakeVacancyService : IVacancyService
    {
        public Task<VacancyIndexViewModel> SearchAsync(VacancyFilterViewModel filter, string? userId = null, bool isEmployer = false)
        {
            return Task.FromResult(new VacancyIndexViewModel
            {
                Filter = filter,
                Items = []
            });
        }

        public Task<bool> HasEmployerProfileAsync(string userId) => Task.FromResult(true);
        public Task<VacancyIndexViewModel> GetEmployerVacanciesAsync(string userId) => Task.FromResult(new VacancyIndexViewModel());
        public Task<VacancyFormViewModel> GetCreateModelAsync(string userId) => Task.FromResult(new VacancyFormViewModel());
        public Task<VacancyFormViewModel?> GetEditModelAsync(string userId, int id) => Task.FromResult<VacancyFormViewModel?>(null);
        public Task<VacancyDetailsViewModel?> GetDetailsModelAsync(int id, string? userId, bool isEmployer, bool isAdmin = false) => Task.FromResult<VacancyDetailsViewModel?>(null);
        public Task CreateAsync(string userId, VacancyFormViewModel model) => Task.CompletedTask;
        public Task UpdateAsync(string userId, VacancyFormViewModel model) => Task.CompletedTask;
        public Task DeleteAsync(string userId, int id) => Task.CompletedTask;
        public Task CloseAsync(string userId, int id) => Task.CompletedTask;
        public Task<List<SelectListItem>> GetCategoriesAsync() => Task.FromResult(new List<SelectListItem>());
        public Task<List<SelectListItem>> GetSkillsAsync() => Task.FromResult(new List<SelectListItem>());
    }

    private sealed class FakeResumeService : IResumeService
    {
        public string? RequestedUserId { get; private set; }

        public Task<bool> HasCandidateProfileAsync(string userId) => Task.FromResult(true);
        public Task<List<ResumeListItemViewModel>> GetUserResumesAsync(string userId) => Task.FromResult(new List<ResumeListItemViewModel>());

        public Task<List<SelectListItem>> GetResumeOptionsAsync(string userId)
        {
            RequestedUserId = userId;
            return Task.FromResult(new List<SelectListItem>
            {
                new("Resume 1", "1")
            });
        }

        public Task<ResumeSearchViewModel> SearchPublishedResumesAsync(ResumeSearchFilterViewModel filter) => Task.FromResult(new ResumeSearchViewModel());
        public Task<ResumeFormViewModel> GetCreateModelAsync(string userId) => Task.FromResult(new ResumeFormViewModel());
        public Task<ResumeFormViewModel?> GetEditModelAsync(string userId, int id) => Task.FromResult<ResumeFormViewModel?>(null);
        public Task<ResumeDetailsViewModel?> GetDetailsModelAsync(string userId, int id) => Task.FromResult<ResumeDetailsViewModel?>(null);
        public Task<ResumeDetailsViewModel?> GetPublishedDetailsModelAsync(int id) => Task.FromResult<ResumeDetailsViewModel?>(null);
        public Task<ResumeDetailsViewModel?> GetEmployerCandidateDetailsModelAsync(int id) => Task.FromResult<ResumeDetailsViewModel?>(null);
        public Task<ResumeFormViewModel?> GetDeleteModelAsync(string userId, int id) => Task.FromResult<ResumeFormViewModel?>(null);
        public Task CreateAsync(string userId, ResumeFormViewModel model) => Task.CompletedTask;
        public Task UpdateAsync(string userId, ResumeFormViewModel model) => Task.CompletedTask;
        public Task DeleteAsync(string userId, int id) => Task.CompletedTask;
        public Task<List<SelectListItem>> GetCategoriesAsync() => Task.FromResult(new List<SelectListItem>());
    }
}
