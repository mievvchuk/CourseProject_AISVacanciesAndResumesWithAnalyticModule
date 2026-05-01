using AisVacanciesAndResumes.Extensions;
using AisVacanciesAndResumes.ViewModels.Analytics;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using System.Text;

namespace AisVacanciesAndResumes.Services;

public class ExportService : IExportService
{
    private const string PdfDateFormat = "yyyy-MM-dd HH:mm";

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

    public byte[] GenerateVacanciesPdf(IEnumerable<VacancyListItemViewModel> vacancies)
    {
        var lines = new List<string>
        {
            "Звіт за вакансіями",
            $"Сформовано: {DateTime.Now.ToString(PdfDateFormat)}",
            string.Empty,
            "Назва | Компанія | Місто | Зайнятість | Досвід | Зарплата | Статус"
        };

        lines.AddRange(vacancies.Select(v =>
            $"{v.Title} | {v.CompanyName} | {v.City} | {v.EmploymentType.GetDisplayName()} | {v.ExperienceLevel.GetDisplayName()} | {v.SalaryFrom}-{v.SalaryTo} | {v.Status.GetDisplayName()}"));

        return BuildSimplePdf(lines);
    }

    public byte[] GenerateAnalyticsPdf(AnalyticsDashboardViewModel analytics)
    {
        var lines = new List<string>
        {
            "Аналітичний звіт",
            $"Сформовано: {DateTime.Now.ToString(PdfDateFormat)}",
            string.Empty,
            "Загальні показники",
            $"Вакансії: {analytics.VacancyCount}",
            $"Резюме: {analytics.ResumeCount}",
            $"Заявки: {analytics.ApplicationCount}",
            $"Кандидати: {analytics.CandidateCount}",
            $"Роботодавці: {analytics.EmployerCount}",
            $"Середня зарплата: {analytics.AverageSalary}",
            $"Середня відповідність: {analytics.AverageMatchPercentage}",
            $"Активні вакансії: {analytics.ActiveVacancyCount}",
            $"Закриті вакансії: {analytics.ClosedVacancyCount}",
            string.Empty,
            "Вакансії за категоріями"
        };

        lines.AddRange(analytics.VacanciesByCategory.Select(x => $"{x.CategoryName}: {x.Count}"));
        lines.Add(string.Empty);
        lines.Add("Популярні навички");
        lines.AddRange(analytics.PopularSkills.Select(x => $"{x.SkillName}: {x.UsageCount}"));
        lines.Add(string.Empty);
        lines.Add("Типи зайнятості вакансій");
        lines.AddRange(analytics.VacancyEmploymentTypeDistribution.Select(x => $"{x.EmploymentTypeName}: {x.Count}"));
        lines.Add(string.Empty);
        lines.Add("Рівні досвіду в резюме");
        lines.AddRange(analytics.ResumeExperienceDistribution.Select(x => $"{x.ExperienceLevelName}: {x.Count}"));

        return BuildSimplePdf(lines);
    }

    private static string Escape(string? value)
    {
        var safeValue = value ?? string.Empty;
        if (safeValue.Contains(',') || safeValue.Contains('"') || safeValue.Contains('\n') || safeValue.Contains('\r'))
        {
            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }

        return safeValue;
    }

    private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
    {
        var pages = Paginate(lines, 42);
        var objects = new List<string>();
        var contentObjectNumbers = new List<int>();

        objects.Add("<< /Type /Catalog /Pages 2 0 R >>");

        var kids = new StringBuilder();
        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var pageObjectNumber = 3 + pageIndex * 2;
            contentObjectNumbers.Add(pageObjectNumber + 1);
            kids.Append($"{pageObjectNumber} 0 R ");
        }

        objects.Add($"<< /Type /Pages /Count {pages.Count} /Kids [{kids.ToString().TrimEnd()}] >>");

        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var content = BuildPageContent(pages[pageIndex]);
            var contentLength = Encoding.UTF8.GetByteCount(content);
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >> /Contents {contentObjectNumbers[pageIndex]} 0 R >>");
            objects.Add($"<< /Length {contentLength} >>\nstream\n{content}\nendstream");
        }

        var builder = new StringBuilder();
        builder.AppendLine("%PDF-1.4");

        var offsets = new List<int> { 0 };
        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(Encoding.UTF8.GetByteCount(builder.ToString()));
            builder.AppendLine($"{index + 1} 0 obj");
            builder.AppendLine(objects[index]);
            builder.AppendLine("endobj");
        }

        var xrefPosition = Encoding.UTF8.GetByteCount(builder.ToString());
        builder.AppendLine("xref");
        builder.AppendLine($"0 {objects.Count + 1}");
        builder.AppendLine("0000000000 65535 f ");

        for (var index = 1; index < offsets.Count; index++)
        {
            builder.AppendLine($"{offsets[index]:0000000000} 00000 n ");
        }

        builder.AppendLine("trailer");
        builder.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        builder.AppendLine("startxref");
        builder.AppendLine(xrefPosition.ToString());
        builder.Append("%%EOF");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static List<List<string>> Paginate(IReadOnlyList<string> lines, int linesPerPage)
    {
        var pages = new List<List<string>>();
        for (var index = 0; index < lines.Count; index += linesPerPage)
        {
            pages.Add(lines.Skip(index).Take(linesPerPage).ToList());
        }

        if (pages.Count == 0)
        {
            pages.Add(new List<string> { "No data" });
        }

        return pages;
    }

    private static string BuildPageContent(IEnumerable<string> lines)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BT");
        builder.AppendLine("/F1 10 Tf");
        builder.AppendLine("50 790 Td");
        builder.AppendLine("14 TL");

        foreach (var line in lines)
        {
            builder.AppendLine($"<{ToPdfUnicodeHex(line)}> Tj");
            builder.AppendLine("T*");
        }

        builder.Append("ET");
        return builder.ToString();
    }

    private static string ToPdfUnicodeHex(string value)
    {
        var bytes = Encoding.BigEndianUnicode.GetBytes('\uFEFF' + value);
        return Convert.ToHexString(bytes);
    }
}
