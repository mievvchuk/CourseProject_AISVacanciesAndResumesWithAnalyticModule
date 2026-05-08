using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AisVacanciesAndResumes.Tests;

public class FullProjectSmokeTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly IntegrationTestWebApplicationFactory _factory;

    public FullProjectSmokeTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public static TheoryData<string> GuestPages => new()
    {
        "/",
        "/Home/Privacy",
        "/Account/Login",
        "/Account/Register",
        "/Vacancies",
        "/Vacancies/Details/1",
        "/Export/Vacancies",
        "/Export/VacanciesPdf",
        "/api/vacancies",
        "/api/vacancies/1",
        "/api/resumes/1",
        "/api/analytics/summary"
    };

    public static TheoryData<string> CandidatePages => new()
    {
        "/",
        "/CandidateProfiles/Details",
        "/CandidateProfiles/Edit",
        "/Resumes",
        "/Resumes/Create",
        "/Resumes/Details/1",
        "/Resumes/Edit/1",
        "/Resumes/Delete/1",
        "/Applications/MyApplications",
        "/Applications/Details/1",
        "/SavedSearches",
        "/SavedSearches/Create",
        "/SavedSearches/Open/1",
        "/SavedSearches/Delete/1",
        "/Portfolio",
        "/Portfolio/Create",
        "/Portfolio/Edit/1",
        "/Portfolio/Delete/1",
        "/Notifications",
        "/Notifications/Details/1",
        "/Messages/Inbox",
        "/Messages/Sent",
        "/Messages/Create",
        "/Messages/Details/1",
        "/Recommendations",
        "/api/applications/1",
        "/api/recommendations"
    };

    public static TheoryData<string> EmployerPages => new()
    {
        "/",
        "/EmployerProfiles/Details",
        "/EmployerProfiles/Edit",
        "/Vacancies/My",
        "/Vacancies/Create",
        "/Vacancies/Edit/1",
        "/Applications/EmployerApplications",
        "/Applications/VacancyApplications?vacancyId=1",
        "/Applications/Details/1",
        "/EmployerCandidates/Search",
        "/EmployerCandidates/Details/1",
        "/Analytics",
        "/Export/Analytics",
        "/Export/AnalyticsPdf",
        "/Notifications",
        "/Notifications/Details/2",
        "/Messages/Inbox",
        "/Messages/Sent",
        "/Messages/Create",
        "/Messages/Details/1",
        "/api/applications/1"
    };

    public static TheoryData<string> AdminPages => new()
    {
        "/",
        "/Admin",
        "/Admin/Dashboard",
        "/Admin/Users",
        "/Admin/Vacancies",
        "/Admin/Resumes",
        "/Admin/ResumeDetails/1",
        "/Admin/ModerationLog",
        "/Analytics",
        "/Export/Analytics",
        "/Export/AnalyticsPdf",
        "/EmployerCandidates/Search",
        "/EmployerCandidates/Details/1",
        "/Notifications",
        "/Notifications/Details/3",
        "/Messages/Inbox",
        "/Messages/Sent",
        "/Messages/Create",
        "/api/applications/1"
    };

    public static TheoryData<string> ProtectedPages => new()
    {
        "/Admin/Dashboard",
        "/Resumes",
        "/Applications/MyApplications",
        "/Applications/EmployerApplications",
        "/EmployerProfiles/Edit",
        "/CandidateProfiles/Edit",
        "/Analytics",
        "/Messages/Inbox",
        "/Notifications"
    };

    public static TheoryData<string, string[]> RoleDeniedPages => new()
    {
        { "candidate.integration@example.com", ["/Admin/Dashboard", "/Vacancies/My", "/EmployerProfiles/Edit", "/Applications/EmployerApplications", "/EmployerCandidates/Search", "/Analytics"] },
        { "employer.integration@example.com", ["/Admin/Dashboard", "/Resumes", "/CandidateProfiles/Edit", "/Applications/MyApplications", "/Portfolio", "/Recommendations"] },
        { "admin.integration@example.com", ["/Resumes", "/CandidateProfiles/Edit", "/Vacancies/My", "/EmployerProfiles/Edit", "/Applications/MyApplications", "/Applications/EmployerApplications"] }
    };

    [Theory]
    [MemberData(nameof(GuestPages))]
    public async Task GuestPages_ReturnSuccess(string path)
    {
        using var client = CreateClient();

        var response = await client.GetAsync(path);

        Assert.True(response.IsSuccessStatusCode, $"{path} returned {(int)response.StatusCode} {response.StatusCode}");
    }

    [Theory]
    [MemberData(nameof(CandidatePages))]
    public async Task CandidatePages_ReturnExpectedResponse(string path)
    {
        using var client = await CreateLoggedInClientAsync("candidate.integration@example.com", "Candidate123");

        var response = await client.GetAsync(path);

        AssertAllowed(path, response);
    }

    [Theory]
    [MemberData(nameof(EmployerPages))]
    public async Task EmployerPages_ReturnExpectedResponse(string path)
    {
        using var client = await CreateLoggedInClientAsync("employer.integration@example.com", "Employer123");

        var response = await client.GetAsync(path);

        AssertAllowed(path, response);
    }

    [Theory]
    [MemberData(nameof(AdminPages))]
    public async Task AdminPages_ReturnExpectedResponse(string path)
    {
        using var client = await CreateLoggedInClientAsync("admin.integration@example.com", "Admin123");

        var response = await client.GetAsync(path);

        AssertAllowed(path, response);
    }

    [Theory]
    [MemberData(nameof(ProtectedPages))]
    public async Task Guest_IsRedirectedFromProtectedPages(string path)
    {
        using var client = CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", GetRedirectPath(response));
    }

    [Theory]
    [MemberData(nameof(RoleDeniedPages))]
    public async Task WrongRole_CannotOpenForbiddenPages(string email, string[] paths)
    {
        using var client = await CreateLoggedInClientAsync(email, PasswordFor(email));

        foreach (var path in paths)
        {
            var response = await client.GetAsync(path);
            AssertForbidden(path, response);
        }
    }

    [Theory]
    [InlineData("guest", "", "")]
    [InlineData("candidate", "candidate.integration@example.com", "Candidate123")]
    [InlineData("employer", "employer.integration@example.com", "Employer123")]
    [InlineData("admin", "admin.integration@example.com", "Admin123")]
    public async Task RenderedNavigationLinks_DoNotReturnServerErrorsOrBrokenLocalRoutes(string role, string email, string password)
    {
        using var client = string.IsNullOrWhiteSpace(email)
            ? CreateClient()
            : await CreateLoggedInClientAsync(email, password);

        var startPages = role switch
        {
            "candidate" => CandidateHtmlPages,
            "employer" => EmployerHtmlPages,
            "admin" => AdminHtmlPages,
            _ => GuestHtmlPages
        };

        var links = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var page in startPages)
        {
            var response = await client.GetAsync(page);
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            var html = await response.Content.ReadAsStringAsync();
            foreach (var href in ExtractLocalLinks(html))
            {
                links.Add(href);
            }
        }

        Assert.NotEmpty(links);
        foreach (var link in links.OrderBy(x => x).Take(80))
        {
            var response = await client.GetAsync(link);
            Assert.True(
                response.StatusCode is not HttpStatusCode.InternalServerError and not HttpStatusCode.NotFound,
                $"{role} rendered link {link} returned {(int)response.StatusCode} {response.StatusCode}");
        }
    }

    [Fact]
    public async Task FormsRenderWithExpectedLayoutClasses()
    {
        using var guest = CreateClient();
        using var candidate = await CreateLoggedInClientAsync("candidate.integration@example.com", "Candidate123");
        using var employer = await CreateLoggedInClientAsync("employer.integration@example.com", "Employer123");
        using var admin = await CreateLoggedInClientAsync("admin.integration@example.com", "Admin123");

        var checks = new (HttpClient Client, string Path, string ExpectedClass)[]
        {
            (guest, "/Account/Login", "justify-content-center"),
            (guest, "/Account/Register", "justify-content-center"),
            (candidate, "/Resumes/Create", "resume-form"),
            (candidate, "/CandidateProfiles/Edit", "profile-edit-card"),
            (employer, "/EmployerProfiles/Edit", "profile-edit-card"),
            (employer, "/Vacancies/Create", "justify-content-center"),
            (admin, "/Admin/Users", "align-items-end"),
            (admin, "/Admin/Vacancies", "align-items-end"),
            (admin, "/Admin/Resumes", "align-items-end")
        };

        foreach (var (client, path, expectedClass) in checks)
        {
            var response = await client.GetAsync(path);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("<form", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(expectedClass, html, StringComparison.OrdinalIgnoreCase);
        }
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private async Task<HttpClient> CreateLoggedInClientAsync(string email, string password)
    {
        var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        loginPage.EnsureSuccessStatusCode();

        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);
        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return client;
    }

    private static void AssertAllowed(string path, HttpResponseMessage response)
    {
        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
            $"{path} returned {(int)response.StatusCode} {response.StatusCode}");

        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            var redirectPath = GetRedirectPath(response);
            Assert.NotEqual("/Account/Login", redirectPath);
            Assert.NotEqual("/Account/AccessDenied", redirectPath);
        }
    }

    private static void AssertForbidden(string path, HttpResponseMessage response)
    {
        var location = response.Headers.Location?.OriginalString ?? string.Empty;
        var redirectPath = GetRedirectPath(response);
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            (response.StatusCode == HttpStatusCode.Redirect && string.Equals(redirectPath, "/Account/AccessDenied", StringComparison.OrdinalIgnoreCase)),
            $"{path} should be forbidden, but returned {(int)response.StatusCode} {response.StatusCode} -> {location}");
    }

    private static IEnumerable<string> ExtractLocalLinks(string html)
    {
        foreach (Match match in Regex.Matches(html, "href=\"([^\"]+)\"", RegexOptions.IgnoreCase))
        {
            var href = WebUtility.HtmlDecode(match.Groups[1].Value);
            if (string.IsNullOrWhiteSpace(href) ||
                href.StartsWith("#", StringComparison.Ordinal) ||
                href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                IsStaticAsset(href))
            {
                continue;
            }

            yield return href.StartsWith('/') ? href : "/" + href;
        }
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

    private static bool IsHtmlPage(string path)
    {
        return !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) &&
            !path.Contains("Export/", StringComparison.OrdinalIgnoreCase) &&
            !path.Contains("/Open/", StringComparison.OrdinalIgnoreCase);
    }

    private static string PasswordFor(string email)
    {
        if (email.StartsWith("candidate", StringComparison.OrdinalIgnoreCase))
        {
            return "Candidate123";
        }

        if (email.StartsWith("employer", StringComparison.OrdinalIgnoreCase))
        {
            return "Employer123";
        }

        return "Admin123";
    }

    private static string GetRedirectPath(HttpResponseMessage response)
    {
        var location = response.Headers.Location;
        if (location is null)
        {
            return string.Empty;
        }

        return location.IsAbsoluteUri ? location.AbsolutePath : location.OriginalString.Split('?', 2)[0];
    }

    private static bool IsStaticAsset(string href)
    {
        var path = href.Split('?', 2)[0];
        return path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".map", StringComparison.OrdinalIgnoreCase);
    }

    private static readonly string[] GuestHtmlPages =
    [
        "/",
        "/Home/Privacy",
        "/Account/Login",
        "/Account/Register",
        "/Vacancies",
        "/Vacancies/Details/1"
    ];

    private static readonly string[] CandidateHtmlPages =
    [
        "/",
        "/CandidateProfiles/Details",
        "/CandidateProfiles/Edit",
        "/Resumes",
        "/Resumes/Create",
        "/Applications/MyApplications",
        "/SavedSearches",
        "/Portfolio",
        "/Notifications",
        "/Messages/Inbox",
        "/Recommendations"
    ];

    private static readonly string[] EmployerHtmlPages =
    [
        "/",
        "/EmployerProfiles/Details",
        "/EmployerProfiles/Edit",
        "/Vacancies/My",
        "/Vacancies/Create",
        "/Applications/EmployerApplications",
        "/EmployerCandidates/Search",
        "/Analytics",
        "/Notifications",
        "/Messages/Inbox"
    ];

    private static readonly string[] AdminHtmlPages =
    [
        "/",
        "/Admin/Dashboard",
        "/Admin/Users",
        "/Admin/Vacancies",
        "/Admin/Resumes",
        "/Admin/ModerationLog",
        "/Analytics",
        "/EmployerCandidates/Search",
        "/Notifications",
        "/Messages/Inbox"
    ];
}
