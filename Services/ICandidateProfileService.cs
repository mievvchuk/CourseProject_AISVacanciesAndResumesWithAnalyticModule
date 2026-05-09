using AisVacanciesAndResumes.ViewModels.CandidateProfiles;

namespace AisVacanciesAndResumes.Services;

public interface ICandidateProfileService
{
    Task<bool> ExistsAsync(string userId);
    Task<CandidateProfileFormViewModel> GetOrCreateFormAsync(string userId);
    Task<CandidateProfileDetailsViewModel?> GetDetailsAsync(string userId, string fullName, string email);
    Task<bool> IsCompletedAsync(string userId);
    Task SaveAsync(string userId, CandidateProfileFormViewModel model);
}
