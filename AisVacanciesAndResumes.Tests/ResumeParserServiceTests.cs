using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Services;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace AisVacanciesAndResumes.Tests;

public class ResumeParserServiceTests
{
    [Fact]
    public async Task ParseAsync_FillsFieldsFromUkrainianDocx()
    {
        var file = CreateDocxFormFile(
            "uk-resume.docx",
            """
            Євчук Михайло
            Бажана посада: Junior .NET Developer
            Телефон: +380 67 123 45 67
            Email: candidate@example.com
            Про себе
            Відповідальний розробник, працюю з ASP.NET Core та базами даних.
            Освіта
            Магістр комп'ютерних наук, 2024
            Досвід роботи
            3 роки комерційної розробки у продуктовій компанії.
            Навички
            C#, ASP.NET Core, PostgreSQL, Git
            Бажана зарплата: 30 000 - 50 000 грн
            Повна зайнятість
            """);

        var result = await new ResumeParserService().ParseAsync(file);

        Assert.Equal("Junior .NET Developer", result.DesiredPosition);
        Assert.Equal("Розробка ПЗ", result.CategoryName);
        Assert.DoesNotContain("candidate@example.com", result.DesiredPosition);
        Assert.DoesNotContain("+380", result.DesiredPosition);
        Assert.Contains("ASP.NET Core", result.Summary);
        Assert.Contains("Магістр", result.Education);
        Assert.Contains("3 роки", result.Experience);
        Assert.Contains("PostgreSQL", result.SkillsDescription);
        Assert.Equal(3, result.YearsOfExperience);
        Assert.Equal(40000, result.DesiredSalary);
        Assert.Equal(EducationLevel.Master, result.EducationLevel);
        Assert.Equal(ExperienceLevel.Junior, result.ExperienceLevel);
        Assert.Equal(EmploymentType.FullTime, result.EmploymentType);
        Assert.Equal("candidate@example.com", result.Email);
    }

    [Fact]
    public async Task ParseAsync_FillsFieldsFromEnglishDocx()
    {
        var file = CreateDocxFormFile(
            "en-resume.docx",
            """
            Desired position: Data Analyst
            Summary
            Analyst with Power BI, SQL and reporting experience.
            Education
            Bachelor of Economics
            Work experience
            2 years at a retail company.
            Skills
            SQL, Power BI, Excel, Python
            Expected salary: 30000 UAH
            Email: analyst@example.com
            Phone: +380 50 111 22 33
            Remote work
            """);

        var result = await new ResumeParserService().ParseAsync(file);

        Assert.Equal("Data Analyst", result.DesiredPosition);
        Assert.Equal("Аналітика даних", result.CategoryName);
        Assert.Contains("Power BI", result.Summary);
        Assert.Contains("Bachelor", result.Education);
        Assert.Contains("2 years", result.Experience);
        Assert.Contains("Python", result.SkillsDescription);
        Assert.Equal(2, result.YearsOfExperience);
        Assert.Equal(30000, result.DesiredSalary);
        Assert.Equal(EducationLevel.Bachelor, result.EducationLevel);
        Assert.Equal(ExperienceLevel.Junior, result.ExperienceLevel);
        Assert.Equal(EmploymentType.Remote, result.EmploymentType);
        Assert.Equal("analyst@example.com", result.Email);
    }

    [Fact]
    public async Task ParseAsync_CleansSectionLabelsAndStopsBeforeNextSection()
    {
        var file = CreateDocxFormFile(
            "clean-resume.docx",
            """
            Петренко Іван
            Junior Frontend Developer
            Email: frontend@example.com
            Телефон: +380 67 000 11 22
            Короткий опис: Frontend developer, створюю адаптивні інтерфейси та працюю з REST API.
            Освіта: Київський фаховий коледж інформаційних технологій, 2025
            Досвід роботи: 2 роки комерційної розробки SPA та підтримки UI-компонентів.
            Навички: HTML • CSS • JavaScript • React • Git
            Очікувана зарплата: 25 000 грн
            Віддалена робота
            """);

        var result = await new ResumeParserService().ParseAsync(file);

        Assert.Equal("Junior Frontend Developer", result.DesiredPosition);
        Assert.Equal("Розробка ПЗ", result.CategoryName);
        Assert.Equal("Frontend developer, створюю адаптивні інтерфейси та працюю з REST API", result.Summary);
        Assert.Equal("Київський фаховий коледж інформаційних технологій, 2025", result.Education);
        Assert.Equal("2 роки комерційної розробки SPA та підтримки UI-компонентів", result.Experience);
        Assert.Equal("HTML, CSS, JavaScript, React, Git", result.SkillsDescription);
        Assert.DoesNotContain("Очікувана зарплата", result.SkillsDescription);
        Assert.DoesNotContain("Петренко", result.Summary);
        Assert.Contains("JavaScript", result.ParsedSkillNames);
        Assert.Contains("React", result.ParsedSkillNames);
        Assert.Equal(25000, result.DesiredSalary);
        Assert.Equal(EmploymentType.Remote, result.EmploymentType);
    }

    private static IFormFile CreateDocxFormFile(string fileName, string text)
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var document = archive.CreateEntry("word/document.xml");
            using var writer = new StreamWriter(document.Open(), Encoding.UTF8);
            writer.Write("<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>");

            foreach (var line in text.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0))
            {
                writer.Write("<w:p><w:r><w:t>");
                writer.Write(WebUtility.HtmlEncode(line));
                writer.Write("</w:t></w:r></w:p>");
            }

            writer.Write("</w:body></w:document>");
        }

        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "ResumeFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
    }
}
