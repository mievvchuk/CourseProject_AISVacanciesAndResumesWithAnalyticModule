using AisVacanciesAndResumes.ViewModels.Analytics;
using AisVacanciesAndResumes.ViewModels.Vacancies;

namespace AisVacanciesAndResumes.Services;

public interface IExportService
{
    byte[] GenerateVacanciesCsv(IEnumerable<VacancyListItemViewModel> vacancies);
    byte[] GenerateAnalyticsCsv(AnalyticsDashboardViewModel analytics);
    byte[] GenerateVacanciesPdf(IEnumerable<VacancyListItemViewModel> vacancies);
    byte[] GenerateAnalyticsPdf(AnalyticsDashboardViewModel analytics);
}
