using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.EmployerProfiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class EmployerProfileService : IEmployerProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EmployerProfileService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }
    public async Task<bool> IsCompletedAsync(string userId)
    {
        return await _context.EmployerProfiles.AnyAsync(x =>
            x.UserId == userId &&
            !string.IsNullOrWhiteSpace(x.CompanyName) &&
            !string.IsNullOrWhiteSpace(x.Description) &&
            !string.IsNullOrWhiteSpace(x.City));
    }
    public async Task<bool> ExistsAsync(string userId)
    {
        return await _context.EmployerProfiles.AnyAsync(x => x.UserId == userId);
    }
    private string? GetExistingPublicFilePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        var fullPath = Path.Combine(
            _environment.WebRootPath,
            path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        return File.Exists(fullPath) ? path : null;
    }
    public async Task<EmployerProfileFormViewModel> GetOrCreateFormAsync(string userId)
    {
        var profile = await EnsureProfileAsync(userId);

        return new EmployerProfileFormViewModel
        {
            Id = profile.Id,
            CompanyName = profile.CompanyName,
            Industry = profile.Industry,
            Description = profile.Description,
            CompanySize = profile.CompanySize,
            Website = profile.Website,
            City = profile.City,
            Location = profile.Location,
            FoundedYear = profile.FoundedYear,
            LogoPath = GetExistingPublicFilePath(profile.LogoPath)
        };
    }

    public async Task<EmployerProfileDetailsViewModel?> GetDetailsAsync(string userId, string fullName, string email)
    {
        var profile = await _context.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return null;
        }

        return new EmployerProfileDetailsViewModel
        {
            Id = profile.Id,
            FullName = fullName,
            Email = email,
            CompanyName = profile.CompanyName,
            Description = profile.Description,
            CompanySize = profile.CompanySize,
            Website = profile.Website,
            City = profile.City,
            LogoPath = GetExistingPublicFilePath(profile.LogoPath)
        };
    }

    public async Task SaveAsync(string userId, EmployerProfileFormViewModel model)
    {
        var profile = await EnsureProfileAsync(userId);

        profile.CompanyName = model.CompanyName;
        profile.Industry = model.Industry;
        profile.Description = model.Description;
        profile.CompanySize = model.CompanySize;
        profile.Website = model.Website;
        profile.City = model.City;
        profile.Location = model.Location;
        profile.FoundedYear = model.FoundedYear;
        profile.LogoPath = await SaveFileAsync(model.LogoFile, profile.LogoPath, "uploads", "logos");

        await _context.SaveChangesAsync();
    }

    private async Task<EmployerProfile> EnsureProfileAsync(string userId)
    {
        var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile is not null)
        {
            return profile;
        }

        profile = new EmployerProfile
        {
            UserId = userId
        };

        _context.EmployerProfiles.Add(profile);
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
