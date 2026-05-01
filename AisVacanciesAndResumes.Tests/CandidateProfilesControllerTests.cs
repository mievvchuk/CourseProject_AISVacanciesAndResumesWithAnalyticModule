using AisVacanciesAndResumes.Controllers;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.CandidateProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Tests;

public class CandidateProfilesControllerTests
{
    [Fact]
    public async Task Details_RedirectsToCreate_WhenProfileDoesNotExist()
    {
        var user = new User { Id = "candidate-1", UserName = "candidate@test.local", Email = "candidate@test.local", FullName = "Candidate" };
        using var userManager = CreateUserManager(user);
        var controller = new CandidateProfilesController(new FakeCandidateProfileService(false), userManager)
        {
            ControllerContext = CreateControllerContext(user.Id)
        };

        var result = await controller.Details();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Create", redirect.ActionName);
    }

    private static ControllerContext CreateControllerContext(string userId)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    private static UserManager<User> CreateUserManager(User user)
    {
        return new UserManager<User>(
            new FakeUserStore(user),
            Options.Create(new IdentityOptions()),
            new PasswordHasher<User>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new LoggerFactory().CreateLogger<UserManager<User>>());
    }

    private sealed class FakeCandidateProfileService : ICandidateProfileService
    {
        private readonly bool _exists;

        public FakeCandidateProfileService(bool exists)
        {
            _exists = exists;
        }

        public Task<bool> ExistsAsync(string userId) => Task.FromResult(_exists);
        public Task<CandidateProfileFormViewModel> GetOrCreateFormAsync(string userId) => Task.FromResult(new CandidateProfileFormViewModel());
        public Task<CandidateProfileDetailsViewModel?> GetDetailsAsync(string userId, string fullName, string email) => Task.FromResult<CandidateProfileDetailsViewModel?>(null);
        public Task SaveAsync(string userId, CandidateProfileFormViewModel model) => Task.CompletedTask;
    }

    private sealed class FakeUserStore : IUserStore<User>
    {
        private readonly User _user;

        public FakeUserStore(User user)
        {
            _user = user;
        }

        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public void Dispose() { }
        public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult(userId == _user.Id ? _user : null);
        public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<User?>(_user);
        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    }
}
