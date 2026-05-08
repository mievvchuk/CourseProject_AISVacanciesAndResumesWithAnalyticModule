using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using System.Text;

namespace AisVacanciesAndResumes.Tests;

public class ExportServiceTests
{
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
        Assert.Contains("Розробник", csv);
        Assert.Contains("Повна зайнятість", csv);
        Assert.Contains("Активна", csv);
    }
}
