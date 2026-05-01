using AisVacanciesAndResumes.ViewModels.Vacancies;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AisVacanciesAndResumes.Services;

public interface IVacancyService
{
    Task<VacancyIndexViewModel> SearchAsync(VacancyFilterViewModel filter, string? userId = null, bool isEmployer = false);
    Task<VacancyIndexViewModel> GetEmployerVacanciesAsync(string userId);
    Task<bool> HasEmployerProfileAsync(string userId);
    Task<VacancyFormViewModel> GetCreateModelAsync(string userId);
    Task<VacancyFormViewModel?> GetEditModelAsync(string userId, int id);
    Task<VacancyDetailsViewModel?> GetDetailsModelAsync(int id, string? userId, bool isEmployer, bool isAdmin = false);
    Task CreateAsync(string userId, VacancyFormViewModel model);
    Task UpdateAsync(string userId, VacancyFormViewModel model);
    Task DeleteAsync(string userId, int id);
    Task CloseAsync(string userId, int id);
    Task<List<SelectListItem>> GetCategoriesAsync();
    Task<List<SelectListItem>> GetSkillsAsync();
}
