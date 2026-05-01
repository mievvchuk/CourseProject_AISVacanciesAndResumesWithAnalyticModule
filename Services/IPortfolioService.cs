using AisVacanciesAndResumes.ViewModels.Portfolio;

namespace AisVacanciesAndResumes.Services;

public interface IPortfolioService
{
    Task<List<PortfolioItemListItemViewModel>> GetUserPortfolioAsync(string userId);
    Task<PortfolioItemFormViewModel> GetCreateModelAsync(string userId);
    Task<PortfolioItemFormViewModel?> GetEditModelAsync(string userId, int id);
    Task<PortfolioItemDeleteViewModel?> GetDeleteModelAsync(string userId, int id);
    Task CreateAsync(string userId, PortfolioItemFormViewModel model);
    Task UpdateAsync(string userId, PortfolioItemFormViewModel model);
    Task DeleteAsync(string userId, int id);
}
