using AisVacanciesAndResumes.ViewModels.Resumes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AisVacanciesAndResumes.Services;

public interface IResumeService
{
    Task<bool> HasCandidateProfileAsync(string userId);
    Task<List<ResumeListItemViewModel>> GetUserResumesAsync(string userId);
    Task<List<SelectListItem>> GetResumeOptionsAsync(string userId);
    Task<ResumeSearchViewModel> SearchPublishedResumesAsync(ResumeSearchFilterViewModel filter);
    Task<ResumeFormViewModel> GetCreateModelAsync(string userId);
    Task<ResumeFormViewModel?> GetEditModelAsync(string userId, int id);
    Task<ResumeDetailsViewModel?> GetDetailsModelAsync(string userId, int id);
    Task<ResumeDetailsViewModel?> GetPublishedDetailsModelAsync(int id);
    Task<ResumeDetailsViewModel?> GetEmployerCandidateDetailsModelAsync(int id);
    Task<ResumeFormViewModel?> GetDeleteModelAsync(string userId, int id);
    Task CreateAsync(string userId, ResumeFormViewModel model);
    Task UpdateAsync(string userId, ResumeFormViewModel model);
    Task DeleteAsync(string userId, int id);
    Task<List<SelectListItem>> GetCategoriesAsync();
    Task<bool> HasAnyResumeAsync(string userId);
}
