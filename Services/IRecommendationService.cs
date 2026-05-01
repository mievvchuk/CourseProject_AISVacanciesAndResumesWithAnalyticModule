using AisVacanciesAndResumes.ViewModels.Recommendations;

namespace AisVacanciesAndResumes.Services;

public interface IRecommendationService
{
    Task<RecommendationIndexViewModel> GetRecommendationsAsync(string userId, int take = 10);
}
