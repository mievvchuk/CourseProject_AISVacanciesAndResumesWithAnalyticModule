using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Analytics;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using System.Text;

namespace AisVacanciesAndResumes.Tests;

public class ExportServiceTests
{
    [Fact]
    public void GenerateVacanciesPdf_ReturnsPdfDocument()
    {
        var service = new ExportService();

        var result = service.GenerateVacanciesPdf([
            new VacancyListItemViewModel
            {
                Title = "Розробник",
                CompanyName = "AIS",
                City = "Київ",
                EmploymentType = EmploymentType.FullTime,
                ExperienceLevel = ExperienceLevel.Junior,
                Status = VacancyStatus.Published
            }
        ]);

        var prefix = Encoding.ASCII.GetString(result.Take(8).ToArray());
        Assert.StartsWith("%PDF-", prefix);
        Assert.DoesNotContain((byte)'?', result);
    }

    [Fact]
    public void GenerateAnalyticsPdf_ReturnsPdfDocument()
    {
        var service = new ExportService();

        var result = service.GenerateAnalyticsPdf(new AnalyticsDashboardViewModel
        {
            VacancyCount = 3,
            ResumeCount = 2,
            ApplicationCount = 1
        });

        var prefix = Encoding.ASCII.GetString(result.Take(8).ToArray());
        Assert.StartsWith("%PDF-", prefix);
        Assert.DoesNotContain((byte)'?', result);
    }

    [Fact]
    public void GenerateVacanciesCsv_UsesUkrainianHeadersAndEnumNames()
    {
        var service = new ExportService();

        var result = service.GenerateVacanciesCsv([
            new VacancyListItemViewModel
            {
                Title = "Розробник",
                CompanyName = "AIS",
                City = "Київ",
                EmploymentType = EmploymentType.FullTime,
                ExperienceLevel = ExperienceLevel.Junior,
                Status = VacancyStatus.Published
            }
        ]);

        var csv = Encoding.UTF8.GetString(result);
        Assert.Contains("Назва,Компанія,Категорія", csv);
        Assert.Contains("Повна зайнятість", csv);
        Assert.Contains("Активна", csv);
    }
}
