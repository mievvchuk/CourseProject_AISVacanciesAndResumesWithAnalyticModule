using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AisVacanciesAndResumes.Tests;

public class ExportControllerIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExportControllerIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task VacanciesPdf_ReturnsPrintableHtml()
    {
        var response = await _client.GetAsync("/Export/VacanciesPdf");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Звіт за вакансіями", html);
        Assert.Contains("Зберегти як PDF", html);
    }
}
