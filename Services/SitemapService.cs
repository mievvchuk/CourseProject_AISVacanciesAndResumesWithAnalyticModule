using System.Xml.Linq;
using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AisVacanciesAndResumes.Services;

public class SitemapService
{
    private static readonly XNamespace SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private readonly ApplicationDbContext _context;
    private readonly SiteSettings _settings;

    public SitemapService(ApplicationDbContext context, IOptions<SiteSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = NormalizeBaseUrl(_settings.BaseUrl);
        var today = DateTime.UtcNow.Date;

        var urls = new List<SitemapUrl>
        {
            new(BuildAbsoluteUrl(baseUrl, "/"), today, "daily", "1.0"),
            new(BuildAbsoluteUrl(baseUrl, "/Vacancies"), today, "daily", "0.9")
        };

        var vacancies = await _context.Vacancies
            .AsNoTracking()
            .Where(x => x.IsActive && x.Status == VacancyStatus.Published)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.Id,
                LastModified = x.UpdatedAt > x.PublishedAt ? x.UpdatedAt : x.PublishedAt
            })
            .ToListAsync(cancellationToken);

        urls.AddRange(vacancies.Select(vacancy =>
            new SitemapUrl(
                BuildAbsoluteUrl(baseUrl, $"/Vacancies/Details/{vacancy.Id}"),
                vacancy.LastModified.Date,
                "weekly",
                "0.8")));

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(SitemapNamespace + "urlset",
                urls.Select(url => new XElement(SitemapNamespace + "url",
                    new XElement(SitemapNamespace + "loc", url.Location),
                    new XElement(SitemapNamespace + "lastmod", url.LastModified.ToString("yyyy-MM-dd")),
                    new XElement(SitemapNamespace + "changefreq", url.ChangeFrequency),
                    new XElement(SitemapNamespace + "priority", url.Priority)))));

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static string NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return "https://courseproject-fr3i.onrender.com";
        }

        return baseUrl.Trim().TrimEnd('/');
    }

    private static string BuildAbsoluteUrl(string baseUrl, string path)
    {
        return $"{baseUrl}/{path.TrimStart('/')}";
    }

    private sealed record SitemapUrl(
        string Location,
        DateTime LastModified,
        string ChangeFrequency,
        string Priority);
}
