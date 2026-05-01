using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.Portfolio;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class PortfolioService : IPortfolioService
{
    private readonly ApplicationDbContext _context;

    public PortfolioService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PortfolioItemListItemViewModel>> GetUserPortfolioAsync(string userId)
    {
        return await _context.PortfolioItems
            .AsNoTracking()
            .Where(x => x.CandidateProfile != null && x.CandidateProfile.UserId == userId)
            .OrderByDescending(x => x.Id)
            .Select(x => new PortfolioItemListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Url = x.Url,
                ImagePath = x.ImagePath
            })
            .ToListAsync();
    }

    public async Task<PortfolioItemFormViewModel> GetCreateModelAsync(string userId)
    {
        await EnsureCandidateProfileAsync(userId);
        return new PortfolioItemFormViewModel();
    }

    public async Task<PortfolioItemFormViewModel?> GetEditModelAsync(string userId, int id)
    {
        return await _context.PortfolioItems
            .AsNoTracking()
            .Where(x => x.Id == id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId)
            .Select(x => new PortfolioItemFormViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Url = x.Url,
                ImagePath = x.ImagePath
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PortfolioItemDeleteViewModel?> GetDeleteModelAsync(string userId, int id)
    {
        return await _context.PortfolioItems
            .AsNoTracking()
            .Where(x => x.Id == id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId)
            .Select(x => new PortfolioItemDeleteViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Url = x.Url,
                ImagePath = x.ImagePath
            })
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(string userId, PortfolioItemFormViewModel model)
    {
        var candidateProfile = await EnsureCandidateProfileAsync(userId);

        var item = new PortfolioItem
        {
            CandidateProfileId = candidateProfile.Id,
            Title = model.Title,
            Description = model.Description,
            Url = model.Url,
            ImagePath = model.ImagePath
        };

        _context.PortfolioItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(string userId, PortfolioItemFormViewModel model)
    {
        var item = await _context.PortfolioItems
            .Include(x => x.CandidateProfile)
            .FirstAsync(x => x.Id == model.Id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        item.Title = model.Title;
        item.Description = model.Description;
        item.Url = model.Url;
        item.ImagePath = model.ImagePath;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string userId, int id)
    {
        var item = await _context.PortfolioItems
            .Include(x => x.CandidateProfile)
            .FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        if (item is null)
        {
            return;
        }

        _context.PortfolioItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    private async Task<CandidateProfile> EnsureCandidateProfileAsync(string userId)
    {
        var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is not null)
        {
            return profile;
        }

        profile = new CandidateProfile
        {
            UserId = userId,
            Headline = "Junior Specialist",
            Summary = "Tell employers about your experience and goals.",
            City = "Kyiv"
        };

        _context.CandidateProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return profile;
    }
}
