using AisVacanciesAndResumes.Extensions;
using AisVacanciesAndResumes.ViewModels.Analytics;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using System.Text;

namespace AisVacanciesAndResumes.Services;

public class ExportService : IExportService
{
    public byte[] GenerateVacanciesCsv(IEnumerable<VacancyListItemViewModel> vacancies)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Назва,Компанія,Категорія,Місто,Тип зайнятості,Рівень досвіду,Зарплата від,Зарплата до,Статус,Дата публікації,Навички");

        foreach (var vacancy in vacancies)
        {
            builder.AppendLine(string.Join(",",
                Escape(vacancy.Title),
                Escape(vacancy.CompanyName),
                Escape(vacancy.CategoryName),
                Escape(vacancy.City),
                Escape(vacancy.EmploymentType.GetDisplayName()),
                Escape(vacancy.ExperienceLevel.GetDisplayName()),
                vacancy.SalaryFrom,
                vacancy.SalaryTo,
                Escape(vacancy.Status.GetDisplayName()),
                Escape(vacancy.PublishedAt.ToString("yyyy-MM-dd")),
                Escape(string.Join("; ", vacancy.Skills))));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
    }

    public byte[] GenerateAnalyticsCsv(AnalyticsDashboardViewModel analytics)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Загальні показники");
        builder.AppendLine("Показник,Значення");
        builder.AppendLine($"Вакансії,{analytics.VacancyCount}");
        builder.AppendLine($"Резюме,{analytics.ResumeCount}");
        builder.AppendLine($"Заявки,{analytics.ApplicationCount}");
        builder.AppendLine($"Кандидати,{analytics.CandidateCount}");
        builder.AppendLine($"Роботодавці,{analytics.EmployerCount}");
        builder.AppendLine($"Середня зарплата,{analytics.AverageSalary}");
        builder.AppendLine($"Середня відповідність,{analytics.AverageMatchPercentage}");
        builder.AppendLine($"Активні вакансії,{analytics.ActiveVacancyCount}");
        builder.AppendLine($"Закриті вакансії,{analytics.ClosedVacancyCount}");
        builder.AppendLine();

        builder.AppendLine("Вакансії за категоріями");
        builder.AppendLine("Категорія,Кількість");
        foreach (var item in analytics.VacanciesByCategory)
        {
            builder.AppendLine($"{Escape(item.CategoryName)},{item.Count}");
        }
        builder.AppendLine();

        builder.AppendLine("Кандидати за досвідом");
        builder.AppendLine("Рівень досвіду,Кількість");
        foreach (var item in analytics.CandidateExperienceDistribution)
        {
            builder.AppendLine($"{Escape(item.ExperienceLevelName)},{item.Count}");
        }
        builder.AppendLine();

        builder.AppendLine("Вакансії за типом зайнятості");
        builder.AppendLine("Тип зайнятості,Кількість");
        foreach (var item in analytics.VacancyEmploymentTypeDistribution)
        {
            builder.AppendLine($"{Escape(item.EmploymentTypeName)},{item.Count}");
        }
        builder.AppendLine();

        builder.AppendLine("Резюме за досвідом");
        builder.AppendLine("Рівень досвіду,Кількість");
        foreach (var item in analytics.ResumeExperienceDistribution)
        {
            builder.AppendLine($"{Escape(item.ExperienceLevelName)},{item.Count}");
        }
        builder.AppendLine();

        builder.AppendLine("Популярні навички");
        builder.AppendLine("Навичка,Кількість використань");
        foreach (var item in analytics.PopularSkills)
        {
            builder.AppendLine($"{Escape(item.SkillName)},{item.UsageCount}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
    }

    private static string Escape(string? value)
    {
        var safeValue = value?.Trim() ?? string.Empty;
        if (safeValue.Contains(',') || safeValue.Contains('"') || safeValue.Contains('\n') || safeValue.Contains('\r'))
        {
            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }

        return safeValue;
    }
}
