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
    private static readonly string[] PositionHeadings =
    [
        "бажана посада", "посада", "позиція", "професійна мета", "цільова посада",
        "desired position", "position", "job title", "target position", "objective"
    ];

    private static readonly string[] SummaryHeadings =
    [
        "про себе", "короткий опис", "профіль", "професійний профіль", "summary",
        "profile", "about", "about me", "professional summary"
    ];

    private static readonly string[] EducationHeadings =
    [
        "освіта", "навчання", "кваліфікація", "education", "academic background", "qualification"
    ];

    private static readonly string[] ExperienceHeadings =
    [
        "досвід", "досвід роботи", "професійний досвід", "кар'єра", "практика",
        "experience", "work experience", "employment history", "career history"
    ];

    private static readonly string[] SkillsHeadings =
    [
        "навички", "технічні навички", "ключові навички", "компетенції", "технології", "інструменти",
        "skills", "technical skills", "key skills", "core skills", "competencies", "technologies", "tools", "stack", "tech stack"
    ];

    private static readonly string[] ContactHeadings =
    [
        "контакти", "телефон", "електронна пошта", "email", "phone", "contacts", "contact", "linkedin", "github"
    ];

    private static readonly string[] AllSectionHeadings =
        PositionHeadings
            .Concat(SummaryHeadings)
            .Concat(EducationHeadings)
            .Concat(ExperienceHeadings)
            .Concat(SkillsHeadings)
            .Concat(ContactHeadings)
            .Concat(["зарплата", "бажана зарплата", "очікувана зарплата", "salary", "expected salary"])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

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
        var lines = GetMeaningfulLines(normalizedText);

        var summary = ExtractSection(lines, SummaryHeadings);
        var education = ExtractSection(lines, EducationHeadings);
        var experience = ExtractSection(lines, ExperienceHeadings);
        var skillsDescription = ExtractSection(lines, SkillsHeadings);
        var desiredPosition = ExtractSingleLineValue(lines, PositionHeadings);

        if (string.IsNullOrWhiteSpace(desiredPosition))
        {
            desiredPosition = ExtractLikelyTitle(lines);
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            summary = ExtractFallbackSummary(lines);
        }

        if (string.IsNullOrWhiteSpace(education))
        {
            education = ExtractKeywordLines(lines, ["університет", "коледж", "бакалавр", "магістр", "education", "university", "college", "bachelor", "master"], 3);
        }

        if (string.IsNullOrWhiteSpace(experience))
        {
            experience = ExtractKeywordLines(lines, ["досвід", "працював", "працювала", "компанія", "experience", "worked", "company"], 4);
        }

        if (string.IsNullOrWhiteSpace(skillsDescription))
        {
            skillsDescription = ExtractKeywordLines(lines, ["навички", "технології", "skills", "technologies", "stack"], 3);
        }

        skillsDescription = CleanupBlock(skillsDescription);
        var parsedSkillNames = ExtractSkillNames(skillsDescription);
        if (parsedSkillNames.Count == 0)
        {
            parsedSkillNames = ExtractKnownSkillNames(compactText);
            skillsDescription = string.Join(", ", parsedSkillNames);
        }

        return new ResumeParseResult
        {
            ExtractedText = Limit(compactText, 4000),
            DesiredPosition = Limit(CleanupPosition(desiredPosition), 160),
            CategoryName = GuessCategory(desiredPosition, skillsDescription, compactText),
            Summary = Limit(CleanupBlock(summary), 1000),
            Education = Limit(CleanupBlock(education), 1000),
            Experience = Limit(CleanupBlock(experience), 1200),
            SkillsDescription = Limit(skillsDescription, 1000),
            ParsedSkillNames = parsedSkillNames,
            Email = ExtractEmail(compactText),
            PhoneNumber = ExtractPhoneNumber(compactText),
            YearsOfExperience = ExtractYearsOfExperience(compactText),
            DesiredSalary = ExtractDesiredSalary(compactText),
            EducationLevel = ExtractEducationLevel(compactText),
            ExperienceLevel = ExtractExperienceLevel(compactText),
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
        var withBreaks = Regex.Replace(xml, @"</w:p>|</w:tr>|</w:tbl>|<w:br\s*/?>", "\n", RegexOptions.IgnoreCase);
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

        return string.Join(' ', new[]
        {
            ExtractPdfTextOperators(rawLatin),
            ExtractPdfTextOperators(rawUtf8),
            ExtractPdfHexStrings(rawLatin),
            ExtractPrintablePdfText(rawUtf8),
            ExtractPrintablePdfText(rawLatin)
        }.Where(x => !string.IsNullOrWhiteSpace(x)));
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
        var matches = Regex.Matches(rawText, @"\((.*?)\)\s*Tj|\[(.*?)\]\s*TJ", RegexOptions.Singleline);
        var builder = new StringBuilder();

        foreach (Match match in matches)
        {
            var value = (match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value)
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
                builder.Append(' ').Append(Encoding.BigEndianUnicode.GetString(Convert.FromHexString(hex)));
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
            @"[\p{L}\p{N}@+().,\-/:#]{3,}(?:\s+[\p{L}\p{N}@+().,\-/:#]{2,})*",
            RegexOptions.Multiline);

        return string.Join(' ', matches.Select(x => x.Value.Trim()).Where(x => x.Length >= 3));
    }

    private static string NormalizeText(string text)
    {
        var decoded = WebUtility.HtmlDecode(text)
            .Replace('\u00A0', ' ')
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

    private static List<string> GetMeaningfulLines(string text)
    {
        return text.Split('\n')
            .Select(x => Regex.Replace(x, @"\s+", " ").Trim())
            .Where(x => x.Length > 0)
            .ToList();
    }

    private static string ExtractSection(List<string> lines, IEnumerable<string> headings)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var matchedHeading = FindHeading(lines[i], headings);
            if (string.IsNullOrWhiteSpace(matchedHeading))
            {
                continue;
            }

            var inlineValue = ExtractInlineValue(lines[i], matchedHeading);
            if (!string.IsNullOrWhiteSpace(inlineValue))
            {
                return CleanupBlock(inlineValue);
            }

            var block = new List<string>();
            for (var j = i + 1; j < lines.Count; j++)
            {
                if (IsAnyHeading(lines[j]))
                {
                    break;
                }

                if (IsContactLine(lines[j]) || IsLikelyPersonalName(lines[j]))
                {
                    continue;
                }

                block.Add(lines[j]);
            }

            var value = CleanupBlock(string.Join(" ", block));
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string ExtractSingleLineValue(List<string> lines, IEnumerable<string> headings)
    {
        foreach (var line in lines.Take(25))
        {
            foreach (var heading in headings)
            {
                var value = ExtractInlineValue(line, heading);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return string.Empty;
    }

    private static string ExtractInlineValue(string line, string heading)
    {
        var pattern = $@"^\s*{Regex.Escape(heading)}\s*[:\-–—]\s*(.+)$";
        var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? CleanupBlock(match.Groups[1].Value) : string.Empty;
    }

    private static string FindHeading(string line, IEnumerable<string> headings)
    {
        return headings
            .OrderByDescending(x => x.Length)
            .FirstOrDefault(heading => IsHeading(line, heading)) ?? string.Empty;
    }

    private static bool IsAnyHeading(string line)
    {
        return AllSectionHeadings.Any(heading => IsHeading(line, heading));
    }

    private static bool IsHeading(string line, string heading)
    {
        var normalized = NormalizeHeading(line);
        var normalizedHeading = NormalizeHeading(heading);

        return normalized == normalizedHeading ||
            normalized.StartsWith($"{normalizedHeading}:") ||
            normalized.StartsWith($"{normalizedHeading} -") ||
            normalized.StartsWith($"{normalizedHeading} –") ||
            normalized.StartsWith($"{normalizedHeading} —");
    }

    private static string NormalizeHeading(string value)
    {
        return value.Trim(' ', '-', ':', '.', ';', '–', '—').ToLowerInvariant();
    }

    private static string ExtractLikelyTitle(List<string> lines)
    {
        foreach (var line in lines.Take(12))
        {
            var candidate = CleanupPosition(line);
            if (candidate.Length < 3 ||
                candidate.Length > 90 ||
                IsAnyHeading(candidate) ||
                IsContactLine(candidate))
            {
                continue;
            }

            if (IsLikelyJobTitle(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    private static string ExtractFallbackSummary(List<string> lines)
    {
        var summaryLines = lines
            .Where(x => !IsAnyHeading(x) && !IsContactLine(x))
            .Where(x => !IsLikelyPersonalName(x) && !IsLikelyJobTitle(x))
            .Where(x => x.Length is >= 30 and <= 220)
            .Take(2)
            .ToList();

        return CleanupBlock(string.Join(" ", summaryLines));
    }

    private static string ExtractKeywordLines(List<string> lines, IEnumerable<string> keywords, int maxLines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (IsContactLine(lines[i]) ||
                !keywords.Any(keyword => lines[i].Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var block = new List<string>();
            for (var j = i; j < lines.Count && block.Count < maxLines; j++)
            {
                if (j > i && IsAnyHeading(lines[j]))
                {
                    break;
                }

                if (IsContactLine(lines[j]) || IsLikelyPersonalName(lines[j]))
                {
                    continue;
                }

                var value = CleanupBlock(lines[j]);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    block.Add(value);
                }
            }

            var result = CleanupBlock(string.Join(" ", block));
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }
        }

        return string.Empty;
    }

    private static string CleanupBlock(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var withoutContacts = RemoveContactFragments(value);
        withoutContacts = Regex.Replace(withoutContacts, @"[•●▪]+", ", ");
        withoutContacts = Regex.Replace(withoutContacts, @"\s+", " ");
        withoutContacts = Regex.Replace(withoutContacts, @"\s*,\s*", ", ");
        withoutContacts = StripSectionLabels(withoutContacts);
        return withoutContacts.Trim(' ', '-', ':', ';', ',', '.', '–', '—');
    }

    private static string CleanupPosition(string value)
    {
        var cleaned = RemoveContactFragments(value);
        cleaned = Regex.Replace(cleaned, @"\b(телефон|phone|email|електронна пошта|linkedin|github)\b.*", string.Empty, RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        return cleaned.Trim(' ', '-', ':', ';', ',', '.', '–', '—');
    }

    private static string StripSectionLabels(string value)
    {
        var result = value;
        foreach (var heading in AllSectionHeadings.OrderByDescending(x => x.Length))
        {
            result = Regex.Replace(result, $@"^\s*{Regex.Escape(heading)}\s*[:\-–—]?\s*", string.Empty, RegexOptions.IgnoreCase);

            var trailingMatch = Regex.Match(result, $@"\s+{Regex.Escape(heading)}\s*[:\-–—]", RegexOptions.IgnoreCase);
            if (trailingMatch.Success)
            {
                result = result[..trailingMatch.Index];
            }
        }

        return result;
    }

    private static string RemoveContactFragments(string value)
    {
        var result = Regex.Replace(value, @"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", string.Empty, RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\+?\d[\d\s().\-]{8,}\d", string.Empty);
        result = Regex.Replace(result, @"https?://\S+|www\.\S+", string.Empty, RegexOptions.IgnoreCase);
        return result;
    }

    private static bool IsContactLine(string line)
    {
        return Regex.IsMatch(line, @"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(line, @"\+?\d[\d\s().\-]{8,}\d") ||
            Regex.IsMatch(line, @"linkedin|github|телефон|email|електронна пошта", RegexOptions.IgnoreCase);
    }

    private static bool IsLikelyPersonalName(string line)
    {
        var value = CleanupPosition(line);
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return words.Length is >= 2 and <= 3 &&
            words.All(word => Regex.IsMatch(word, @"^\p{Lu}\p{Ll}{1,}['\-]?\p{Ll}*$")) &&
            !IsLikelyJobTitle(value);
    }

    private static bool IsLikelyJobTitle(string value)
    {
        return Regex.IsMatch(
            value,
            @"developer|engineer|manager|analyst|designer|specialist|розробник|інженер|менеджер|аналітик|дизайнер|спеціаліст|фахівець|програміст|розробниця",
            RegexOptions.IgnoreCase);
    }

    private static string ExtractEmail(string text)
    {
        var match = Regex.Match(text, @"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : string.Empty;
    }

    private static string ExtractPhoneNumber(string text)
    {
        var match = Regex.Match(text, @"(\+?\d[\d\s().\-]{8,}\d)");
        return match.Success ? match.Value.Trim() : string.Empty;
    }

    private static List<string> ExtractSkillNames(string skillsDescription)
    {
        if (string.IsNullOrWhiteSpace(skillsDescription))
        {
            return new List<string>();
        }

        var splitSkills = Regex.Split(skillsDescription, @"[,;/|•●▪\n]")
            .Select(x => Regex.Replace(x, @"\s+", " ").Trim(' ', '.', '-', ':'))
            .Where(x => x.Length >= 2 && x.Length <= 40)
            .Where(x => !IsAnyHeading(x) && !IsContactLine(x))
            .ToList();

        return splitSkills
            .Concat(ExtractKnownSkillNames(skillsDescription))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();
    }

    private static List<string> ExtractKnownSkillNames(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        var knownSkills = new[]
        {
            "C#", ".NET", "ASP.NET Core", "Entity Framework", "SQL", "PostgreSQL", "MS SQL", "MySQL",
            "SQLite", "Java", "Python", "JavaScript", "TypeScript", "HTML", "CSS", "Bootstrap",
            "React", "Angular", "Vue", "Node.js", "Git", "GitHub", "Docker", "Azure", "REST API",
            "Swagger", "Postman", "Power BI", "Excel", "Figma", "UI/UX"
        };

        return knownSkills
            .Where(skill => ContainsTerm(text, skill))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool ContainsTerm(string source, string term)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(term))
        {
            return false;
        }

        var escapedTerm = Regex.Escape(term.Trim());
        return Regex.IsMatch(source, $@"(?<!\w){escapedTerm}(?!\w)", RegexOptions.IgnoreCase);
    }

    private static string GuessCategory(string desiredPosition, string skillsDescription, string fullText)
    {
        var source = $"{desiredPosition} {skillsDescription} {fullText}";

        if (Regex.IsMatch(source, @"data|analytics|power bi|tableau|аналітик|аналітика", RegexOptions.IgnoreCase))
        {
            return "Аналітика даних";
        }

        if (Regex.IsMatch(source, @"c#|\.net|asp\.net|java\b|python|javascript|typescript|react|angular|sql|developer|розробник|програміст", RegexOptions.IgnoreCase))
        {
            return "Розробка ПЗ";
        }

        if (Regex.IsMatch(source, @"figma|ux|ui|designer|дизайн|дизайнер", RegexOptions.IgnoreCase))
        {
            return "Дизайн";
        }

        if (Regex.IsMatch(source, @"marketing|seo|smm|маркетинг", RegexOptions.IgnoreCase))
        {
            return "Маркетинг";
        }

        if (Regex.IsMatch(source, @"admin|адміністр|office|офіс", RegexOptions.IgnoreCase))
        {
            return "Адміністрування";
        }

        return string.Empty;
    }

    private static int? ExtractYearsOfExperience(string text)
    {
        var match = Regex.Match(text, @"(\d{1,2})\+?\s*(роки|років|рік|years|year)", RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var years) ? years : null;
    }

    private static decimal? ExtractDesiredSalary(string text)
    {
        var labeledMatch = Regex.Match(
            text,
            @"(зарплата|бажана зарплата|очікувана зарплата|salary|desired salary|expected salary)[^\d$€]{0,30}[$€]?\s*(\d[\d\s]{2,10})(?:\s*[-–—]\s*[$€]?\s*(\d[\d\s]{2,10}))?",
            RegexOptions.IgnoreCase);

        if (labeledMatch.Success)
        {
            return ParseSalaryRange(labeledMatch.Groups[2].Value, labeledMatch.Groups[3].Value);
        }

        var currencyAfterMatch = Regex.Match(text, @"(\d[\d\s]{2,10})(?:\s*[-–—]\s*(\d[\d\s]{2,10}))?\s*(грн|uah|usd|eur|євро)", RegexOptions.IgnoreCase);
        if (currencyAfterMatch.Success)
        {
            return ParseSalaryRange(currencyAfterMatch.Groups[1].Value, currencyAfterMatch.Groups[2].Value);
        }

        var currencyBeforeMatch = Regex.Match(text, @"[$€]\s*(\d[\d\s]{2,10})(?:\s*[-–—]\s*[$€]?\s*(\d[\d\s]{2,10}))?", RegexOptions.IgnoreCase);
        return currencyBeforeMatch.Success ? ParseSalaryRange(currencyBeforeMatch.Groups[1].Value, currencyBeforeMatch.Groups[2].Value) : null;
    }

    private static decimal? ParseSalaryRange(string from, string to)
    {
        var fromValue = ParseMoney(from);
        var toValue = ParseMoney(to);

        if (fromValue.HasValue && toValue.HasValue)
        {
            return Math.Round((fromValue.Value + toValue.Value) / 2, 0);
        }

        return fromValue ?? toValue;
    }

    private static decimal? ParseMoney(string value)
    {
        var digits = Regex.Replace(value, @"\s+", string.Empty);
        return decimal.TryParse(digits, out var result) ? result : null;
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

    private static ExperienceLevel? ExtractExperienceLevel(string text)
    {
        if (Regex.IsMatch(text, @"\b(senior|sr\.?)\b|сеньйор|старший", RegexOptions.IgnoreCase))
        {
            return ExperienceLevel.Senior;
        }

        if (Regex.IsMatch(text, @"\bmiddle\b|мідл|середній", RegexOptions.IgnoreCase))
        {
            return ExperienceLevel.Middle;
        }

        if (Regex.IsMatch(text, @"\b(junior|jr\.?)\b|джуніор|початковий", RegexOptions.IgnoreCase))
        {
            return ExperienceLevel.Junior;
        }

        if (Regex.IsMatch(text, @"без досвіду|no experience|trainee|стажер", RegexOptions.IgnoreCase))
        {
            return ExperienceLevel.NoExperience;
        }

        var years = ExtractYearsOfExperience(text);
        return years switch
        {
            null => null,
            <= 0 => ExperienceLevel.NoExperience,
            <= 2 => ExperienceLevel.Junior,
            <= 5 => ExperienceLevel.Middle,
            _ => ExperienceLevel.Senior
        };
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
