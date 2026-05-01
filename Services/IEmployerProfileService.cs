using AisVacanciesAndResumes.ViewModels.EmployerProfiles;

namespace AisVacanciesAndResumes.Services;

public interface IEmployerProfileService
{
    Task<bool> ExistsAsync(string userId);
    Task<EmployerProfileFormViewModel> GetOrCreateFormAsync(string userId);
    Task<EmployerProfileDetailsViewModel?> GetDetailsAsync(string userId, string fullName, string email);
    Task SaveAsync(string userId, EmployerProfileFormViewModel model);
}
