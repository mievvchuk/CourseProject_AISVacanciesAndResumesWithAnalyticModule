using Microsoft.AspNetCore.Http;

namespace AisVacanciesAndResumes.Services;

public interface IResumeParserService
{
    Task<ResumeParseResult> ParseAsync(IFormFile? file);
}
