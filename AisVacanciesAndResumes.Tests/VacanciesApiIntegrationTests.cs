using System.Net;
using System.Net.Http.Json;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AisVacanciesAndResumes.Tests;

public class VacanciesApiIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VacanciesApiIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyPublishedActiveVacancies()
    {
        var vacancies = await _client.GetFromJsonAsync<List<VacancyApiDto>>("/api/vacancies");

        var vacancy = Assert.Single(vacancies!);
        Assert.Equal("Integration QA Engineer", vacancy.Title);
        Assert.Equal("Integration Labs", vacancy.CompanyName);
        Assert.Equal("Software Development", vacancy.Category);
        Assert.Contains("ASP.NET Core", vacancy.Skills);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForInactiveVacancy()
    {
        var response = await _client.GetAsync("/api/vacancies/2");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
