namespace AisVacanciesAndResumes.Services;

public interface IMatchingService
{
    Task<int> CalculateMatchPercentageAsync(int resumeId, int vacancyId);
}
