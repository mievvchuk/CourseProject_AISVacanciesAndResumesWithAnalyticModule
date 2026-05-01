using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Extensions;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.SavedSearches;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class SavedSearchService : ISavedSearchService
{
    private readonly ApplicationDbContext _context;

    public SavedSearchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<SavedSearchFormViewModel> GetCreateModelAsync(SavedSearchFormViewModel filter)
    {
        return Task.FromResult(new SavedSearchFormViewModel
        {
            Query = filter.Query,
            City = filter.City,
            CategoryId = filter.CategoryId,
            EmploymentType = filter.EmploymentType,
            ExperienceLevel = filter.ExperienceLevel
        });
    }

    public async Task<List<SavedSearchListItemViewModel>> GetUserSavedSearchesAsync(string userId)
    {
        var searches = await _context.SavedSearches
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return searches.Select(x => new SavedSearchListItemViewModel
        {
            Id = x.Id,
            Query = x.Query,
            City = x.City,
            CategoryId = x.CategoryId,
            CategoryName = x.Category != null ? x.Category.Name : string.Empty,
            EmploymentType = x.EmploymentType,
            ExperienceLevel = x.ExperienceLevel,
            CreatedAt = x.CreatedAt,
            Summary = BuildSummary(x)
        }).ToList();
    }

    public async Task CreateAsync(string userId, SavedSearchFormViewModel model)
    {
        var savedSearch = new SavedSearch
        {
            UserId = userId,
            Query = model.Query,
            City = model.City,
            CategoryId = model.CategoryId,
            EmploymentType = model.EmploymentType,
            ExperienceLevel = model.ExperienceLevel
        };

        _context.SavedSearches.Add(savedSearch);
        await _context.SaveChangesAsync();
    }

    public async Task<SavedSearchDeleteViewModel?> GetDeleteModelAsync(string userId, int id)
    {
        var savedSearch = await _context.SavedSearches
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (savedSearch is null)
        {
            return null;
        }

        return new SavedSearchDeleteViewModel
        {
            Id = savedSearch.Id,
            Query = savedSearch.Query,
            City = savedSearch.City,
            CategoryId = savedSearch.CategoryId,
            EmploymentType = savedSearch.EmploymentType,
            ExperienceLevel = savedSearch.ExperienceLevel,
            Summary = BuildSummary(savedSearch)
        };
    }

    public async Task DeleteAsync(string userId, int id)
    {
        var savedSearch = await _context.SavedSearches
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (savedSearch is null)
        {
            return;
        }

        _context.SavedSearches.Remove(savedSearch);
        await _context.SaveChangesAsync();
    }

    public async Task<SavedSearchFormViewModel?> GetSavedFilterAsync(string userId, int id)
    {
        return await _context.SavedSearches
            .AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new SavedSearchFormViewModel
            {
                Query = x.Query,
                City = x.City,
                CategoryId = x.CategoryId,
                EmploymentType = x.EmploymentType,
                ExperienceLevel = x.ExperienceLevel
            })
            .FirstOrDefaultAsync();
    }

    private static string BuildSummary(SavedSearch search)
    {
        return
            $"Назва: {(string.IsNullOrWhiteSpace(search.Query) ? "будь-яка" : search.Query)}, " +
            $"Місто: {(string.IsNullOrWhiteSpace(search.City) ? "будь-яке" : search.City)}, " +
            $"Категорія: {(search.Category != null ? search.Category.Name : "будь-яка")}, " +
            $"Зайнятість: {(search.EmploymentType.HasValue ? search.EmploymentType.Value.GetDisplayName() : "будь-яка")}, " +
            $"Досвід: {(search.ExperienceLevel.HasValue ? search.ExperienceLevel.Value.GetDisplayName() : "будь-який")}";
    }
}
