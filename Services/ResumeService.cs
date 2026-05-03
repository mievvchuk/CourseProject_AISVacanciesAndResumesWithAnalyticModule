using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.Resumes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Services;

public class ResumeService : IResumeService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ResumeService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<bool> HasCandidateProfileAsync(string userId)
    {
        return await _context.CandidateProfiles.AnyAsync(x => x.UserId == userId);
    }

    public async Task<List<ResumeListItemViewModel>> GetUserResumesAsync(string userId)
    {
        return await _context.Resumes
            .AsNoTracking()
            .Where(x => x.CandidateProfile != null && x.CandidateProfile.UserId == userId)
            .Include(x => x.Category)
            .Include(x => x.CandidateProfile)
            .Include(x => x.ResumeSkills)
            .ThenInclude(x => x.Skill)
            .Select(x => new ResumeListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                DesiredPosition = x.DesiredPosition,
                City = x.CandidateProfile != null ? x.CandidateProfile.City : string.Empty,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                EmploymentType = x.EmploymentType,
                ExperienceYears = x.ExperienceYears,
                ExperienceLevel = x.ExperienceLevel,
                EducationLevel = x.EducationLevel,
                DesiredSalary = x.DesiredSalary,
                Status = x.Status,
                FilePath = x.FilePath,
                OriginalFileName = x.OriginalFileName,
                UpdatedAt = x.UpdatedAt,
                Skills = x.ResumeSkills.Select(rs => rs.Skill != null ? rs.Skill.Name : string.Empty).ToList()
            })
            .OrderByDescending(x => x.Status == ResumeStatus.Published)
            .ThenByDescending(x => x.Status == ResumeStatus.UnderModeration)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetResumeOptionsAsync(string userId)
    {
        return await _context.Resumes
            .AsNoTracking()
            .Where(x => x.CandidateProfile != null &&
                x.CandidateProfile.UserId == userId &&
                x.IsPublished &&
                x.Status == ResumeStatus.Published)
            .OrderBy(x => x.Title)
            .Select(x => new SelectListItem(x.Title, x.Id.ToString()))
            .ToListAsync();
    }

    public async Task<ResumeSearchViewModel> SearchPublishedResumesAsync(ResumeSearchFilterViewModel filter)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var query = _context.Resumes
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.CandidateProfile)
            .ThenInclude(x => x!.User)
            .Include(x => x.ResumeSkills)
            .ThenInclude(x => x.Skill)
            .Where(x => x.CandidateProfile != null && x.IsPublished && x.Status == ResumeStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.DesiredPosition))
        {
            var desiredPosition = filter.DesiredPosition.Trim().ToLower();
            query = query.Where(x =>
                x.DesiredPosition.ToLower().Contains(desiredPosition) ||
                x.Title.ToLower().Contains(desiredPosition));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim().ToLower();
            query = query.Where(x => x.CandidateProfile != null && x.CandidateProfile.City.ToLower().Contains(city));
        }

        if (filter.EmploymentType.HasValue)
        {
            query = query.Where(x => x.EmploymentType == filter.EmploymentType.Value);
        }

        if (filter.ExperienceLevel.HasValue)
        {
            query = query.Where(x => x.ExperienceLevel == filter.ExperienceLevel.Value);
        }

        if (filter.EducationLevel.HasValue)
        {
            query = query.Where(x => x.EducationLevel == filter.EducationLevel.Value);
        }

        if (filter.DesiredSalaryFrom.HasValue)
        {
            query = query.Where(x => x.DesiredSalary.HasValue && x.DesiredSalary.Value >= filter.DesiredSalaryFrom.Value);
        }

        if (filter.DesiredSalaryTo.HasValue)
        {
            query = query.Where(x => x.DesiredSalary.HasValue && x.DesiredSalary.Value <= filter.DesiredSalaryTo.Value);
        }

        var resumes = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

        var skillTerms = SplitSkillTerms(filter.Skills);
        if (skillTerms.Count > 0)
        {
            resumes = resumes
                .Where(x => skillTerms.All(term =>
                    x.ResumeSkills.Any(rs => rs.Skill != null && rs.Skill.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(x.SkillsDescription) && x.SkillsDescription.Contains(term, StringComparison.OrdinalIgnoreCase))))
                .ToList();
        }

        var totalItems = resumes.Count;
        var items = resumes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ResumeSearchListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                DesiredPosition = x.DesiredPosition,
                FullName = x.CandidateProfile?.User?.FullName ?? string.Empty,
                City = x.CandidateProfile?.City ?? string.Empty,
                CategoryName = x.Category?.Name ?? string.Empty,
                EmploymentType = x.EmploymentType,
                ExperienceLevel = x.ExperienceLevel,
                EducationLevel = x.EducationLevel,
                DesiredSalary = x.DesiredSalary,
                Skills = x.ResumeSkills
                    .Select(rs => rs.Skill != null ? rs.Skill.Name : string.Empty)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList()
            })
            .ToList();

        return new ResumeSearchViewModel
        {
            Filter = new ResumeSearchFilterViewModel
            {
                DesiredPosition = filter.DesiredPosition,
                CategoryId = filter.CategoryId,
                City = filter.City,
                EmploymentType = filter.EmploymentType,
                ExperienceLevel = filter.ExperienceLevel,
                EducationLevel = filter.EducationLevel,
                DesiredSalaryFrom = filter.DesiredSalaryFrom,
                DesiredSalaryTo = filter.DesiredSalaryTo,
                Skills = filter.Skills,
                Page = page,
                PageSize = pageSize
            },
            Items = items,
            CategoryOptions = await GetCategoriesAsync(),
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<ResumeFormViewModel> GetCreateModelAsync(string userId)
    {
        var candidateProfile = await GetCandidateProfileAsync(userId);
        var categories = await GetCategoriesAsync();

        return new ResumeFormViewModel
        {
            CandidateProfileId = candidateProfile.Id,
            CategoryId = int.TryParse(categories.FirstOrDefault()?.Value, out var categoryId) ? categoryId : 0,
            CategoryOptions = categories
        };
    }

    public async Task<ResumeFormViewModel?> GetEditModelAsync(string userId, int id)
    {
        var resume = await _context.Resumes
            .Include(x => x.CandidateProfile)
            .Include(x => x.ResumeSkills)
            .FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        if (resume is null)
        {
            return null;
        }

        return new ResumeFormViewModel
        {
            Id = resume.Id,
            CandidateProfileId = resume.CandidateProfileId,
            CategoryId = resume.CategoryId,
            Title = resume.Title,
            DesiredPosition = resume.DesiredPosition,
            Summary = resume.Summary,
            Education = resume.Education,
            Experience = resume.Experience,
            SkillsDescription = resume.SkillsDescription,
            EmploymentType = resume.EmploymentType,
            ExperienceYears = resume.ExperienceYears,
            ExperienceLevel = resume.ExperienceLevel,
            EducationLevel = resume.EducationLevel,
            DesiredSalary = resume.DesiredSalary,
            IsPublished = resume.Status is ResumeStatus.Published or ResumeStatus.UnderModeration,
            Status = resume.Status,
            FilePath = resume.FilePath,
            OriginalFileName = resume.OriginalFileName,
            ContentType = resume.ContentType,
            FileSize = resume.FileSize,
            UploadedAt = resume.UploadedAt,
            ParsedSkillNames = resume.ResumeSkills
                .Select(x => x.Skill != null ? x.Skill.Name : string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList(),
            CategoryOptions = await GetCategoriesAsync(),
        };
    }

    public async Task<ResumeDetailsViewModel?> GetDetailsModelAsync(string userId, int id)
    {
        return await GetResumeDetailsModelAsync(userId, id);
    }

    public async Task<ResumeDetailsViewModel?> GetPublishedDetailsModelAsync(int id)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(x => x.CandidateProfile)
            .ThenInclude(x => x!.User)
            .Include(x => x.Category)
            .Include(x => x.ResumeSkills)
            .ThenInclude(x => x.Skill)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsPublished && x.Status == ResumeStatus.Published);

        if (resume is null)
        {
            return null;
        }

        return MapDetailsModel(resume, resume.CandidateProfile?.User?.FullName ?? string.Empty);
    }

    public async Task<ResumeDetailsViewModel?> GetEmployerCandidateDetailsModelAsync(int id)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(x => x.CandidateProfile)
            .ThenInclude(x => x!.User)
            .Include(x => x.Category)
            .Include(x => x.ResumeSkills)
            .ThenInclude(x => x.Skill)
            .FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfile != null && x.IsPublished && x.Status == ResumeStatus.Published);

        if (resume is null)
        {
            return null;
        }

        return MapDetailsModel(resume, resume.CandidateProfile?.User?.FullName ?? string.Empty);
    }

    public async Task<ResumeFormViewModel?> GetDeleteModelAsync(string userId, int id)
    {
        return await GetResumeModelAsync(userId, id);
    }

    public async Task CreateAsync(string userId, ResumeFormViewModel model)
    {
        var candidateProfile = await GetCandidateProfileAsync(userId);

        var resume = new Resume
        {
            CandidateProfileId = candidateProfile.Id,
            CategoryId = model.CategoryId,
            Title = model.Title,
            DesiredPosition = model.DesiredPosition,
            Summary = model.Summary,
            Education = model.Education,
            Experience = model.Experience,
            SkillsDescription = model.SkillsDescription,
            EmploymentType = model.EmploymentType,
            ExperienceYears = model.ExperienceYears,
            ExperienceLevel = model.ExperienceLevel,
            EducationLevel = model.EducationLevel,
            DesiredSalary = model.DesiredSalary,
            IsPublished = false,
            Status = model.IsPublished ? ResumeStatus.UnderModeration : ResumeStatus.Draft
        };

        await SaveResumeFileAsync(resume, model.ResumeFile);

        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync();

        await SyncSkillsAsync(resume, model.SkillsDescription);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(string userId, ResumeFormViewModel model)
    {
        var resume = await _context.Resumes
            .Include(x => x.CandidateProfile)
            .Include(x => x.ResumeSkills)
            .FirstAsync(x => x.Id == model.Id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        resume.CategoryId = model.CategoryId;
        resume.Title = model.Title;
        resume.DesiredPosition = model.DesiredPosition;
        resume.Summary = model.Summary;
        resume.Education = model.Education;
        resume.Experience = model.Experience;
        resume.SkillsDescription = model.SkillsDescription;
        resume.EmploymentType = model.EmploymentType;
        resume.ExperienceYears = model.ExperienceYears;
        resume.ExperienceLevel = model.ExperienceLevel;
        resume.EducationLevel = model.EducationLevel;
        resume.DesiredSalary = model.DesiredSalary;
        resume.IsPublished = false;
        resume.Status = model.IsPublished ? ResumeStatus.UnderModeration : ResumeStatus.Draft;
        resume.UpdatedAt = DateTime.UtcNow;

        await SaveResumeFileAsync(resume, model.ResumeFile);

        await SyncSkillsAsync(resume, model.SkillsDescription);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string userId, int id)
    {
        var resume = await _context.Resumes
            .Include(x => x.CandidateProfile)
            .FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        if (resume is null)
        {
            return;
        }

        resume.Status = ResumeStatus.Archived;
        resume.IsPublished = false;
        resume.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<SelectListItem>> GetCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }

    private async Task<CandidateProfile> GetCandidateProfileAsync(string userId)
    {
        var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        return profile ?? throw new InvalidOperationException("Candidate profile was not found.");
    }

    private async Task SyncSkillsAsync(Resume resume, string skillsDescription)
    {
        var existingItems = await _context.ResumeSkills
            .Where(x => x.ResumeId == resume.Id)
            .ToListAsync();

        _context.ResumeSkills.RemoveRange(existingItems);

        var normalizedDescription = skillsDescription ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedDescription))
        {
            return;
        }

        var availableSkills = await _context.Skills
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        var matchedSkillIds = availableSkills
            .Where(x => ContainsSkill(normalizedDescription, x.Name))
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        foreach (var skillId in matchedSkillIds)
        {
            _context.ResumeSkills.Add(new ResumeSkill
            {
                ResumeId = resume.Id,
                SkillId = skillId
            });
        }
    }

    private async Task<ResumeFormViewModel?> GetResumeModelAsync(string userId, int id)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(x => x.CandidateProfile)
            .Include(x => x.Category)
            .Include(x => x.ResumeSkills)
            .ThenInclude(x => x.Skill)
            .FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        if (resume is null)
        {
            return null;
        }

        var owner = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        return new ResumeFormViewModel
        {
            Id = resume.Id,
            CandidateProfileId = resume.CandidateProfileId,
            CategoryId = resume.CategoryId,
            Title = resume.Title,
            DesiredPosition = resume.DesiredPosition,
            Summary = resume.Summary,
            Education = resume.Education,
            Experience = resume.Experience,
            SkillsDescription = resume.SkillsDescription,
            EmploymentType = resume.EmploymentType,
            ExperienceYears = resume.ExperienceYears,
            ExperienceLevel = resume.ExperienceLevel,
            EducationLevel = resume.EducationLevel,
            DesiredSalary = resume.DesiredSalary,
            IsPublished = resume.IsPublished,
            Status = resume.Status,
            FilePath = resume.FilePath,
            OriginalFileName = resume.OriginalFileName,
            ContentType = resume.ContentType,
            FileSize = resume.FileSize,
            UploadedAt = resume.UploadedAt,
            ParsedSkillNames = resume.ResumeSkills
                .Select(x => x.Skill != null ? x.Skill.Name : string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList(),
            CategoryName = resume.Category != null ? resume.Category.Name : string.Empty,
            FullName = owner?.FullName ?? string.Empty,
            CategoryOptions = await GetCategoriesAsync()
        };
    }

    private async Task<ResumeDetailsViewModel?> GetResumeDetailsModelAsync(string userId, int id)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(x => x.CandidateProfile)
            .Include(x => x.Category)
            .Include(x => x.ResumeSkills)
            .ThenInclude(x => x.Skill)
            .FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfile != null && x.CandidateProfile.UserId == userId);

        if (resume is null)
        {
            return null;
        }

        var owner = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        return MapDetailsModel(resume, owner?.FullName ?? string.Empty);
    }

    private static List<string> SplitSkillTerms(string? skills)
    {
        return (skills ?? string.Empty)
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static ResumeDetailsViewModel MapDetailsModel(Resume resume, string fullName)
    {
        return new ResumeDetailsViewModel
        {
            Id = resume.Id,
            Title = resume.Title,
            DesiredPosition = resume.DesiredPosition,
            City = resume.CandidateProfile?.City ?? string.Empty,
            Summary = resume.Summary,
            Education = resume.Education,
            Experience = resume.Experience,
            SkillsDescription = resume.SkillsDescription,
            FullName = fullName,
            CandidateUserId = resume.CandidateProfile?.UserId ?? string.Empty,
            CategoryName = resume.Category?.Name ?? string.Empty,
            EmploymentType = resume.EmploymentType,
            ExperienceYears = resume.ExperienceYears,
            ExperienceLevel = resume.ExperienceLevel,
            EducationLevel = resume.EducationLevel,
            DesiredSalary = resume.DesiredSalary,
            Status = resume.Status,
            IsPublished = resume.IsPublished,
            FilePath = resume.FilePath,
            OriginalFileName = resume.OriginalFileName,
            ContentType = resume.ContentType,
            FileSize = resume.FileSize,
            UploadedAt = resume.UploadedAt,
            SkillNames = resume.ResumeSkills
                .Select(x => x.Skill != null ? x.Skill.Name : string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList()
        };
    }

    private static bool ContainsSkill(string source, string skillName)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(skillName))
        {
            return false;
        }

        var escapedName = Regex.Escape(skillName.Trim());
        return Regex.IsMatch(source, $@"(?<!\w){escapedName}(?!\w)", RegexOptions.IgnoreCase);
    }

    private async Task SaveResumeFileAsync(Resume resume, Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".pdf", ".docx" };

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only PDF and DOCX files are allowed.");
        }

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "resumes");
        Directory.CreateDirectory(uploadsRoot);

        DeleteResumeFile(resume.FilePath);

        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsRoot, storedFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        resume.FilePath = $"/uploads/resumes/{storedFileName}";
        resume.OriginalFileName = file.FileName;
        resume.ContentType = file.ContentType;
        resume.FileSize = file.Length;
        resume.UploadedAt = DateTime.UtcNow;
        resume.UpdatedAt = DateTime.UtcNow;
    }

    private void DeleteResumeFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var relativePath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
