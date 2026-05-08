using System.Net;
using System.Text.RegularExpressions;
using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AisVacanciesAndResumes.Tests;

public class RegistrationE2ETests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RegistrationE2ETests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task CandidateCanRegisterAndIsRedirectedToProfileEditing()
    {
        var registerPage = await _client.GetAsync("/Account/Register");
        registerPage.EnsureSuccessStatusCode();

        var html = await registerPage.Content.ReadAsStringAsync();
        var antiforgeryToken = ExtractAntiforgeryToken(html);
        var email = $"candidate.{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiforgeryToken,
            ["FullName"] = "End To End Candidate",
            ["Email"] = email,
            ["Role"] = UserRoleType.Candidate.ToString(),
            ["Password"] = "Candidate123",
            ["ConfirmPassword"] = "Candidate123"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/CandidateProfiles/Edit", response.Headers.Location?.OriginalString);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await context.Users.SingleAsync(x => x.Email == email);
        Assert.True(user.IsActive);
        Assert.True(await context.UserRoles.AnyAsync(x => x.UserId == user.Id));
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);

        return match.Success
            ? WebUtility.HtmlDecode(match.Groups[1].Value)
            : throw new InvalidOperationException("Antiforgery token was not rendered.");
    }
}
