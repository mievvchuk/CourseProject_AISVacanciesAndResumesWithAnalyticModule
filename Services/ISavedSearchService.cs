using AisVacanciesAndResumes.ViewModels.SavedSearches;

namespace AisVacanciesAndResumes.Services;

public interface ISavedSearchService
{
    Task<List<SavedSearchListItemViewModel>> GetUserSavedSearchesAsync(string userId);
    Task<SavedSearchFormViewModel> GetCreateModelAsync(SavedSearchFormViewModel filter);
    Task CreateAsync(string userId, SavedSearchFormViewModel model);
    Task<SavedSearchDeleteViewModel?> GetDeleteModelAsync(string userId, int id);
    Task DeleteAsync(string userId, int id);
    Task<SavedSearchFormViewModel?> GetSavedFilterAsync(string userId, int id);
}
