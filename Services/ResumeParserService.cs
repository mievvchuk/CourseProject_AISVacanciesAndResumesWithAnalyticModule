using AisVacanciesAndResumes.Enums;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace AisVacanciesAndResumes.Services;

public class ResumeParserService : IResumeParserService
{
    private static readonly string[] DesiredPositionKeywords =
    [
        "desired position", "position", "job title", "objective", "target position",
        "посада", "позиція", "бажана посада", "цільова посада", "професійна мета"
    ];

    private static readonly string[] SummaryKeywords =
    [
        "summary", "profile", "about", "about me", "professional summary", "personal profile",
        "про себе", "коротко про себе", "профіль", "короткий опис"
    ];

    private static readonly string[] EducationKeywords =
    [
        "education", "academic background", "qualification",
        "освіта", "кваліфікація", "навчання"
    ];

    private static readonly string[] ExperienceKeywords =
    [
        "experience", "work experience", "employment history", "career history",
        "досвід", "досвід роботи", "професійний досвід", "кар'єра", "практика"
    ];

    private static readonly string[] SkillsKeywords =
    [
        "skills", "technical skills", "key skills", "core skills", "competencies",
        "навички", "технічні навички", "ключові навички", "компетенції", "технології", "інструменти"
    ];

    private static readonly string[] SectionKeywords =
    [
        "summary", "profile", "about", "about me", "education", "experience", "skills",
        "contacts", "contact", "phone", "email", "salary", "desired position", "position",
        "про себе", "профіль", "короткий опис", "освіта", "досвід", "досвід роботи",
        "навички", "контакти", "телефон", "зарплата", "бажана зарплата",
        "бажана посада", "посада", "позиція"
    ];

    public async Task<ResumeParseResult> ParseAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return new ResumeParseResult();
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var text = extension switch
        {
            ".docx" => await ExtractDocxTextAsync(memoryStream),
            ".pdf" => await ExtractPdfTextAsync(memoryStream),
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(text))
        {
            return new ResumeParseResult();
        }

        var normalizedText = NormalizeText(text);
        var compactText = CompactText(normalizedText);
        var summary = ExtractBlock(normalizedText, SummaryKeywords);
        var education = ExtractBlock(normalizedText, EducationKeywords);
        var experience = ExtractBlock(normalizedText, ExperienceKeywords);
        var skillsDescription = ExtractBlock(normalizedText, SkillsKeywords);
        var desiredPosition = ExtractSingleLineValue(normalizedText, DesiredPositionKeywords);
        if (string.IsNullOrWhiteSpace(desiredPosition))
        {
            desiredPosition = ExtractBlock(normalizedText, DesiredPositionKeywords);
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            summary = ExtractFallbackSummary(normalizedText);
        }

        if (string.IsNullOrWhiteSpace(education))
        {
            education = ExtractByKeywordWindow(
                compactText,
                ["education", "academic", "university", "college", "bachelor", "master", "освіта", "університет", "коледж", "бакалавр", "магістр"],
                280);
        }

        if (string.IsNullOrWhiteSpace(experience))
        {
            experience = ExtractByKeywordWindow(
                compactText,
                ["experience", "employment", "career", "worked", "company", "досвід", "роботи", "працював", "працювала", "компанія"],
                360);
        }

        if (string.IsNullOrWhiteSpace(skillsDescription))
        {
            skillsDescription = ExtractByKeywordWindow(
                compactText,
                ["skills", "competencies", "stack", "technologies", "tools", "навички", "компетенції", "технології", "інструменти"],
                260);
        }

        if (string.IsNullOrWhiteSpace(desiredPosition))
        {
            desiredPosition = ExtractByKeywordWindow(
                compactText,
                ["desired position", "position", "job title", "specialist", "manager", "developer", "analyst", "посада", "позиція", "спеціаліст", "фахівець", "менеджер"],
                140);
        }

        if (string.IsNullOrWhiteSpace(desiredPosition))
        {
            desiredPosition = ExtractLikelyTitle(compactText);
        }

        return new ResumeParseResult
        {
            ExtractedText = Limit(compactText, 4000),
            DesiredPosition = desiredPosition,
            Summary = summary,
            Education = education,
            Experience = experience,
            SkillsDescription = skillsDescription,
            ParsedSkillNames = ExtractSkillNames(normalizedText),
            Email = ExtractEmail(compactText),
            PhoneNumber = ExtractPhoneNumber(compactText),
            YearsOfExperience = ExtractYearsOfExperience(compactText),
            DesiredSalary = ExtractDesiredSalary(compactText),
            EducationLevel = ExtractEducationLevel(compactText),
            EmploymentType = ExtractEmploymentType(compactText)
        };
    }

    private static async Task<string> ExtractDocxTextAsync(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var entry = archive.GetEntry("word/document.xml");
        if (entry is null)
        {
            return string.Empty;
        }

        await using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream, Encoding.UTF8);
        var xml = await reader.ReadToEndAsync();
        var withBreaks = Regex.Replace(xml, @"</w:p>|</w:tr>|</w:tbl>", "\n", RegexOptions.IgnoreCase);
        var withoutTags = Regex.Replace(withBreaks, "<.*?>", " ");
        return WebUtility.HtmlDecode(withoutTags);
    }

    private static async Task<string> ExtractPdfTextAsync(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.Position = 0;
        await stream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        var pdfPigText = ExtractPdfTextWithPdfPig(bytes);
        if (!string.IsNullOrWhiteSpace(pdfPigText))
        {
            return pdfPigText;
        }

        var rawLatin = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
        var rawUtf8 = Encoding.UTF8.GetString(bytes);

        var operatorText = string.Join(' ', new[]
        {
            ExtractPdfTextOperators(rawLatin),
            ExtractPdfTextOperators(rawUtf8),
            ExtractPdfHexStrings(rawLatin)
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        var printableText = string.Join(' ', new[]
        {
            ExtractPrintablePdfText(rawUtf8),
            ExtractPrintablePdfText(rawLatin)
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        return $"{operatorText} {printableText}".Trim();
    }

    private static string ExtractPdfTextWithPdfPig(byte[] bytes)
    {
        try
        {
            using var document = PdfDocument.Open(bytes);
            var builder = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }

            return builder.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractPdfTextOperators(string rawText)
    {
        var matches = Regex.Matches(rawText, @"\((.*?)\)\s*Tj", RegexOptions.Singleline);
        if (matches.Count == 0)
        {
            matches = Regex.Matches(rawText, @"\[(.*?)\]\s*TJ", RegexOptions.Singleline);
        }

        var builder = new StringBuilder();
        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            var value = match.Groups[1].Value
                .Replace(@"\(", "(")
                .Replace(@"\)", ")")
                .Replace(@"\n", " ")
                .Replace(@"\r", " ")
                .Replace(@"\t", " ")
                .Replace("\\/", "/");

            builder.Append(' ').Append(value);
        }

        return builder.ToString();
    }

    private static string ExtractPdfHexStrings(string rawText)
    {
        var matches = Regex.Matches(rawText, @"<([0-9A-Fa-f]{8,})>\s*Tj", RegexOptions.Singleline);
        var builder = new StringBuilder();

        foreach (Match match in matches)
        {
            var hex = match.Groups[1].Value;
            if (hex.Length % 2 != 0)
            {
                continue;
            }

            try
            {
                var bytes = Convert.FromHexString(hex);
                builder.Append(' ').Append(Encoding.BigEndianUnicode.GetString(bytes));
            }
            catch
            {
            }
        }

        return builder.ToString();
    }

    private static string ExtractPrintablePdfText(string rawText)
    {
        var matches = Regex.Matches(
            rawText,
            @"[\p{L}\p{N}@+().,\-/:]{3,}(?:\s+[\p{L}\p{N}@+().,\-/:]{2,})*",
            RegexOptions.Multiline);

        return string.Join(' ', matches.Select(x => x.Value.Trim()).Where(x => x.Length >= 3));
    }

    private static string NormalizeText(string text)
    {
        var decoded = WebUtility.HtmlDecode(text)
            .Replace('\u00A0', ' ')
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        decoded = Regex.Replace(decoded, @"[ \t]+", " ");
        decoded = Regex.Replace(decoded, @"[ \t]*\n[ \t]*", "\n");
        decoded = Regex.Replace(decoded, @"\n{2,}", "\n");

        return decoded.Trim();
    }

    private static string CompactText(string text)
    {
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static string ExtractBlock(string text, IEnumerable<string> keywords)
    {
        var lines = GetMeaningfulLines(text);
        foreach (var keyword in keywords)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var inlineValue = ExtractInlineSectionValue(lines[i], keyword);
                if (!string.IsNullOrWhiteSpace(inlineValue))
                {
                    return Limit(CleanupBlock(inlineValue), 1200);
                }

                if (!IsSectionHeading(lines[i], keyword))
                {
                    continue;
                }

                var blockLines = new List<string>();
                for (var j = i + 1; j < lines.Count; j++)
                {
                    if (IsAnySectionHeading(lines[j]))
                    {
                        break;
                    }

                    blockLines.Add(lines[j]);
                }

                var value = CleanupBlock(string.Join(" ", blockLines));
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Limit(value, 1200);
                }
            }
        }

        var compact = CompactText(text);
        var lookahead = string.Join("|", SectionKeywords.Select(Regex.Escape));
        foreach (var keyword in keywords)
        {
            var pattern = $@"(?<!\p{{L}}){Regex.Escape(keyword)}[:\-\s]+(.+?)(?=((?<!\p{{L}})({lookahead})[:\-\s])|$)";
            var match = Regex.Match(compact, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!match.Success)
            {
                continue;
            }

            var value = CleanupBlock(match.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return Limit(value, 1200);
            }
        }

        return string.Empty;
    }

    private static string ExtractSingleLineValue(string text, IEnumerable<string> keywords)
    {
        foreach (var line in GetMeaningfulLines(text).Take(20))
        {
            foreach (var keyword in keywords)
            {
                var value = ExtractInlineSectionValue(line, keyword);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Limit(CleanupBlock(value), 160);
                }
            }
        }

        return string.Empty;
    }

    private static List<string> GetMeaningfulLines(string text)
    {
        return text.Split('\n')
            .Select(x => Regex.Replace(x, @"\s+", " ").Trim())
            .Where(x => x.Length > 0)
            .ToList();
    }

    private static bool IsAnySectionHeading(string line)
    {
        return SectionKeywords.Any(keyword => IsSectionHeading(line, keyword));
    }

    private static bool IsSectionHeading(string line, string keyword)
    {
        var normalized = line.Trim(' ', '-', ':', '.', ';').ToLowerInvariant();
        var normalizedKeyword = keyword.ToLowerInvariant();
        return normalized == normalizedKeyword
            || normalized.StartsWith($"{normalizedKeyword}:")
            || normalized.StartsWith($"{normalizedKeyword} -")
            || normalized.StartsWith($"{normalizedKeyword} —");
    }

    private static string ExtractInlineSectionValue(string line, string keyword)
    {
        var pattern = $@"^\s*{Regex.Escape(keyword)}\s*[:\-—]\s*(.+)$";
        var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string CleanupBlock(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim(' ', '-', ':', ';', ',');
    }

    private static string ExtractFallbackSummary(string text)
    {
        var compact = CompactText(text);
        var sentences = Regex.Split(compact, @"(?<=[.!?])\s+")
            .Select(x => x.Trim())
            .Where(x => x.Length > 20)
            .Take(3)
            .ToList();

        if (sentences.Count > 0)
        {
            return Limit(string.Join(" ", sentences).Trim(), 800);
        }

        return Limit(compact, 400);
    }

    private static string ExtractByKeywordWindow(string text, IEnumerable<string> keywords, int length)
    {
        foreach (var keyword in keywords)
        {
            var index = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                continue;
            }

            var size = Math.Min(length, text.Length - index);
            var value = text.Substring(index, size).Trim(' ', '.', ',', ';', ':', '-');
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string ExtractLikelyTitle(string text)
    {
        var titlePatterns = new[]
        {
            @"\b(senior|middle|junior|lead)?\s*(developer|engineer|manager|analyst|designer|teacher|accountant|assistant|specialist)\b",
            @"\b(розробник|менеджер|аналітик|дизайнер|вчитель|бухгалтер|асистент|спеціаліст|фахівець)\b"
        };

        foreach (var pattern in titlePatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Value.Trim();
            }
        }

        return string.Empty;
    }

    private static string ExtractEmail(string text)
    {
        var match = Regex.Match(text, @"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : string.Empty;
    }

    private static string ExtractPhoneNumber(string text)
    {
        var match = Regex.Match(text, @"(\+?\d[\d\-\s\(\)]{8,}\d)");
        return match.Success ? match.Value.Trim() : string.Empty;
    }

    private static List<string> ExtractSkillNames(string text)
    {
        var skillsBlock = ExtractBlock(text, SkillsKeywords);
        if (string.IsNullOrWhiteSpace(skillsBlock))
        {
            skillsBlock = ExtractByKeywordWindow(text, ["skills", "technologies", "tools", "competencies", "навички", "технології", "інструменти"], 240);
        }

        if (string.IsNullOrWhiteSpace(skillsBlock))
        {
            return new List<string>();
        }

        return Regex.Split(skillsBlock, @"[,;/|•]")
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeSkillToken)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();
    }

    private static string NormalizeSkillToken(string value)
    {
        var normalized = Regex.Replace(value, @"\s+", " ").Trim(' ', '.', '-', ':');
        return normalized.Length < 2 ? string.Empty : normalized;
    }

    private static int? ExtractYearsOfExperience(string text)
    {
        var match = Regex.Match(text, @"(\d{1,2})\+?\s*(years|year|роки|років|рік)", RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var years) ? years : null;
    }

    private static decimal? ExtractDesiredSalary(string text)
    {
        var labeledMatch = Regex.Match(text, @"(salary|desired salary|expected salary|зарплата|бажана зарплата|очікувана зарплата)[^\d]{0,20}(\d{4,7})", RegexOptions.IgnoreCase);
        if (labeledMatch.Success && decimal.TryParse(labeledMatch.Groups[2].Value, out var labeledSalary))
        {
            return labeledSalary;
        }

        var genericMatch = Regex.Match(text, @"(\d{4,7})\s*(uah|грн|\$|usd|eur|євро)", RegexOptions.IgnoreCase);
        return genericMatch.Success && decimal.TryParse(genericMatch.Groups[1].Value, out var genericSalary) ? genericSalary : null;
    }

    private static EducationLevel? ExtractEducationLevel(string text)
    {
        if (Regex.IsMatch(text, @"phd|doctor|доктор|аспірант", RegexOptions.IgnoreCase))
        {
            return EducationLevel.PhD;
        }

        if (Regex.IsMatch(text, @"master|магістр", RegexOptions.IgnoreCase))
        {
            return EducationLevel.Master;
        }

        if (Regex.IsMatch(text, @"bachelor|бакалавр", RegexOptions.IgnoreCase))
        {
            return EducationLevel.Bachelor;
        }

        if (Regex.IsMatch(text, @"secondary|school|середня", RegexOptions.IgnoreCase))
        {
            return EducationLevel.Secondary;
        }

        return null;
    }

    private static EmploymentType? ExtractEmploymentType(string text)
    {
        if (Regex.IsMatch(text, @"internship|intern|стажування", RegexOptions.IgnoreCase))
        {
            return EmploymentType.Internship;
        }

        if (Regex.IsMatch(text, @"part[\s-]?time|часткова зайнятість|неповна зайнятість", RegexOptions.IgnoreCase))
        {
            return EmploymentType.PartTime;
        }

        if (Regex.IsMatch(text, @"remote|віддалено|віддалена робота", RegexOptions.IgnoreCase))
        {
            return EmploymentType.Remote;
        }

        if (Regex.IsMatch(text, @"full[\s-]?time|повна зайнятість", RegexOptions.IgnoreCase))
        {
            return EmploymentType.FullTime;
        }

        return null;
    }

    private static string Limit(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength].Trim();
    }
}
