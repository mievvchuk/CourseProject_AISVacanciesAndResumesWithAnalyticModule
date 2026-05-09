using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.CandidateProfiles;
using AisVacanciesAndResumes.ViewModels.Portfolio;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class CandidateProfileService : ICandidateProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CandidateProfileService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        return await _context.CandidateProfiles.AnyAsync(x => x.UserId == userId);
    }
    public async Task<bool> IsCompletedAsync(string userId)
    {
        return await _context.CandidateProfiles.AnyAsync(x =>
            x.UserId == userId &&
            !string.IsNullOrWhiteSpace(x.Headline) &&
            !string.IsNullOrWhiteSpace(x.Summary) &&
            !string.IsNullOrWhiteSpace(x.City));
    }
    public async Task<CandidateProfileFormViewModel> GetOrCreateFormAsync(string userId)
    {
        var profile = await EnsureProfileAsync(userId);

        return new CandidateProfileFormViewModel
        {
            Id = profile.Id,
            Headline = profile.Headline,
            Summary = profile.Summary,
            City = profile.City,
            PhotoPath = profile.PhotoPath,
            ExperienceYears = profile.ExperienceYears,
            ExperienceLevel = profile.ExperienceLevel,
            EducationLevel = profile.EducationLevel,
            DesiredEmploymentType = profile.DesiredEmploymentType,
            DesiredSalary = profile.DesiredSalary
        };
    }

    public async Task<CandidateProfileDetailsViewModel?> GetDetailsAsync(string userId, string fullName, string email)
    {
        var profile = await _context.CandidateProfiles
            .AsNoTracking()
            .Include(x => x.PortfolioItems)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return null;
        }

        return new CandidateProfileDetailsViewModel
        {
            Id = profile.Id,
            FullName = fullName,
            Email = email,
            Headline = profile.Headline,
            Summary = profile.Summary,
            City = profile.City,
            PhotoPath = profile.PhotoPath,
            ExperienceYears = profile.ExperienceYears,
            ExperienceLevel = profile.ExperienceLevel,
            EducationLevel = profile.EducationLevel,
            DesiredSalary = profile.DesiredSalary,
            PortfolioItems = profile.PortfolioItems
                .OrderByDescending(x => x.Id)
                .Select(x => new PortfolioItemListItemViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Url = x.Url,
                    ImagePath = x.ImagePath
                })
                .ToList()
        };
    }

    public async Task SaveAsync(string userId, CandidateProfileFormViewModel model)
    {
        var profile = await EnsureProfileAsync(userId);

        profile.Headline = model.Headline;
        profile.Summary = model.Summary;
        profile.City = model.City;
        profile.ExperienceYears = model.ExperienceYears;
        profile.ExperienceLevel = model.ExperienceLevel;
        profile.EducationLevel = model.EducationLevel;
        profile.DesiredEmploymentType = model.DesiredEmploymentType;
        profile.DesiredSalary = model.DesiredSalary;
        profile.PhotoPath = await SaveFileAsync(model.PhotoFile, profile.PhotoPath, "uploads", "profiles");

        await _context.SaveChangesAsync();
    }

    private async Task<CandidateProfile> EnsureProfileAsync(string userId)
    {
        var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile is not null)
        {
            return profile;
        }

        profile = new CandidateProfile
        {
            UserId = userId
        };

        _context.CandidateProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    private async Task<string?> SaveFileAsync(Microsoft.AspNetCore.Http.IFormFile? file, string? currentPath, params string[] segments)
    {
        if (file is null || file.Length == 0)
        {
            return currentPath;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Дозволені лише файли JPG, PNG або WEBP.");
        }

        DeleteFile(currentPath);

        var directoryPath = Path.Combine(new[] { _environment.WebRootPath }.Concat(segments).ToArray());
        Directory.CreateDirectory(directoryPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(directoryPath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return "/" + string.Join('/', segments) + "/" + fileName;
    }

    private void DeleteFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var fullPath = Path.Combine(_environment.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
