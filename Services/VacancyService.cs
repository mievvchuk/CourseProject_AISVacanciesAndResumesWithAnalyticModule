using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.ViewModels.Vacancies;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class VacancyService : IVacancyService
{
    private readonly ApplicationDbContext _context;

    public VacancyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VacancyIndexViewModel> SearchAsync(VacancyFilterViewModel filter, string? userId = null, bool isEmployer = false)
    {
        var query = _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.EmployerProfile)
            .Include(x => x.Applications)
            .Include(x => x.VacancySkills)
            .ThenInclude(x => x.Skill)
            .Where(x => x.IsActive && x.Status == VacancyStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            var title = filter.Title.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim().ToLower();
            query = query.Where(x => x.EmployerProfile != null && x.EmployerProfile.City.ToLower().Contains(city));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        if (filter.EmploymentType.HasValue)
        {
            query = query.Where(x => x.EmploymentType == filter.EmploymentType.Value);
        }

        if (filter.ExperienceLevel.HasValue)
        {
            query = query.Where(x => x.ExperienceLevel == filter.ExperienceLevel.Value);
        }

        var items = await query
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => new VacancyListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                CompanyName = x.EmployerProfile != null ? x.EmployerProfile.CompanyName : string.Empty,
                City = x.EmployerProfile != null ? x.EmployerProfile.City : string.Empty,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                EmploymentType = x.EmploymentType,
                ExperienceLevel = x.ExperienceLevel,
                Status = x.Status,
                PublishedAt = x.PublishedAt,
                CanManage = false,
                HasApplied = !isEmployer && !string.IsNullOrWhiteSpace(userId) && x.Applications.Any(a => a.CandidateUserId == userId),
                Skills = x.VacancySkills.Select(vs => vs.Skill != null ? vs.Skill.Name : string.Empty).ToList()
            })
            .ToListAsync();

        return new VacancyIndexViewModel
        {
            Filter = filter,
            Items = items,
            CategoryOptions = await GetCategoriesAsync()
        };
    }

    public async Task<VacancyIndexViewModel> GetEmployerVacanciesAsync(string userId)
    {
        var items = await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.EmployerProfile)
            .Include(x => x.VacancySkills)
            .ThenInclude(x => x.Skill)
            .Where(x => x.EmployerProfile != null && x.EmployerProfile.UserId == userId)
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => new VacancyListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                CompanyName = x.EmployerProfile != null ? x.EmployerProfile.CompanyName : string.Empty,
                City = x.EmployerProfile != null ? x.EmployerProfile.City : string.Empty,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                EmploymentType = x.EmploymentType,
                ExperienceLevel = x.ExperienceLevel,
                Status = x.Status,
                PublishedAt = x.PublishedAt,
                CanManage = true,
                Skills = x.VacancySkills.Select(vs => vs.Skill != null ? vs.Skill.Name : string.Empty).ToList()
            })
            .ToListAsync();

        return new VacancyIndexViewModel
        {
            Items = items,
            CategoryOptions = await GetCategoriesAsync()
        };
    }

    public async Task<bool> HasEmployerProfileAsync(string userId)
    {
        return await _context.EmployerProfiles.AnyAsync(x => x.UserId == userId);
    }

    public async Task<VacancyFormViewModel> GetCreateModelAsync(string userId)
    {
        var employerProfile = await GetEmployerProfileAsync(userId);
        var categories = await GetCategoriesAsync();

        return new VacancyFormViewModel
        {
            EmployerProfileId = employerProfile.Id,
            City = employerProfile.City,
            CategoryId = 0,
            CategoryOptions = categories,
            SkillOptions = await GetSkillsAsync()
        };
    }

    public async Task<VacancyFormViewModel?> GetEditModelAsync(string userId, int id)
    {
        var vacancy = await _context.Vacancies
            .Include(x => x.EmployerProfile)
            .Include(x => x.Category)
            .Include(x => x.VacancySkills)
            .ThenInclude(x => x.Skill)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmployerProfile != null && x.EmployerProfile.UserId == userId);

        if (vacancy is null)
        {
            return null;
        }

        return new VacancyFormViewModel
        {
            Id = vacancy.Id,
            EmployerProfileId = vacancy.EmployerProfileId,
            CategoryId = vacancy.CategoryId,
            CategoryName = null,
            Title = vacancy.Title,
            Description = vacancy.Description,
            Requirements = vacancy.Requirements,
            SalaryFrom = vacancy.SalaryFrom,
            SalaryTo = vacancy.SalaryTo,
            EmploymentType = vacancy.EmploymentType,
            ExperienceLevel = vacancy.ExperienceLevel,
            Status = vacancy.Status,
            IsActive = vacancy.IsActive,
            Location = vacancy.Location,
            ClosingDate = vacancy.ClosingDate,
            City = vacancy.EmployerProfile != null ? vacancy.EmployerProfile.City : string.Empty,
            SelectedSkillIds = vacancy.VacancySkills.Select(x => x.SkillId).ToList(),
            SkillsText = string.Join(", ", vacancy.VacancySkills
                .Select(x => x.Skill != null ? x.Skill.Name : string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))),
            CategoryOptions = await GetCategoriesAsync(),
            SkillOptions = await GetSkillsAsync()
        };
    }

    public async Task<VacancyDetailsViewModel?> GetDetailsModelAsync(int id, string? userId, bool isEmployer, bool isAdmin = false)
    {
        var query = _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.EmployerProfile)
            .Include(x => x.Applications)
            .Include(x => x.VacancySkills)
            .ThenInclude(x => x.Skill)
            .AsQueryable();

        query = query.Where(x => x.Id == id);

        var vacancy = await query.FirstOrDefaultAsync();
        if (vacancy is null)
        {
            return null;
        }

        var canManage = isEmployer &&
            !string.IsNullOrWhiteSpace(userId) &&
            vacancy.EmployerProfile != null &&
            vacancy.EmployerProfile.UserId == userId;
        var isPublished = vacancy.IsActive && vacancy.Status == VacancyStatus.Published;

        if (!isPublished && !canManage && !isAdmin)
        {
            return null;
        }

        return new VacancyDetailsViewModel
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            Description = vacancy.Description,
            Requirements = vacancy.Requirements,
            SalaryFrom = vacancy.SalaryFrom,
            SalaryTo = vacancy.SalaryTo,
            EmploymentType = vacancy.EmploymentType,
            ExperienceLevel = vacancy.ExperienceLevel,
            Status = vacancy.Status,
            IsActive = vacancy.IsActive,
            PublishedAt = vacancy.PublishedAt,
            Location = vacancy.Location,
            ClosingDate = vacancy.ClosingDate,
            CanManage = canManage,
            HasApplied = !isEmployer &&
                !string.IsNullOrWhiteSpace(userId) &&
                vacancy.Applications.Any(a => a.CandidateUserId == userId),
            CompanyName = vacancy.EmployerProfile != null ? vacancy.EmployerProfile.CompanyName : string.Empty,
            CategoryName = vacancy.Category != null ? vacancy.Category.Name : string.Empty,
            City = vacancy.EmployerProfile != null ? vacancy.EmployerProfile.City : string.Empty,
            SkillNames = vacancy.VacancySkills
                .Select(x => x.Skill != null ? x.Skill.Name : string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList()
        };
    }

    public async Task CreateAsync(string userId, VacancyFormViewModel model)
    {
        var employerProfile = await GetEmployerProfileAsync(userId);
        var categoryId = await ResolveCategoryIdAsync(model.CategoryName, model.CategoryId);

        var vacancy = new Models.Vacancy
        {
            EmployerProfileId = employerProfile.Id,
            CategoryId = categoryId,
            Title = model.Title,
            Description = model.Description,
            Requirements = model.Requirements ?? string.Empty,
            SalaryFrom = model.SalaryFrom,
            SalaryTo = model.SalaryTo,
            EmploymentType = model.EmploymentType,
            ExperienceLevel = model.ExperienceLevel,
            Status = VacancyStatus.UnderModeration,
            IsActive = false,
            Location = model.Location ?? string.Empty,
            ClosingDate = model.ClosingDate
        };

        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();

        await SyncSkillsAsync(vacancy.Id, model.SelectedSkillIds, model.SkillsText);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(string userId, VacancyFormViewModel model)
    {
        var vacancy = await _context.Vacancies
            .Include(x => x.EmployerProfile)
            .FirstAsync(x => x.Id == model.Id && x.EmployerProfile != null && x.EmployerProfile.UserId == userId);

        vacancy.CategoryId = await ResolveCategoryIdAsync(model.CategoryName, model.CategoryId);
        vacancy.Title = model.Title;
        vacancy.Description = model.Description;
        vacancy.Requirements = model.Requirements ?? string.Empty;
        vacancy.SalaryFrom = model.SalaryFrom;
        vacancy.SalaryTo = model.SalaryTo;
        vacancy.EmploymentType = model.EmploymentType;
        vacancy.ExperienceLevel = model.ExperienceLevel;
        vacancy.Status = VacancyStatus.UnderModeration;
        vacancy.IsActive = false;
        vacancy.Location = model.Location ?? string.Empty;
        vacancy.ClosingDate = model.ClosingDate;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await SyncSkillsAsync(vacancy.Id, model.SelectedSkillIds, model.SkillsText);
        await _context.SaveChangesAsync();
    }

    public async Task CloseAsync(string userId, int id)
    {
        var vacancy = await _context.Vacancies
            .Include(x => x.EmployerProfile)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmployerProfile != null && x.EmployerProfile.UserId == userId);

        if (vacancy is null)
        {
            return;
        }

        vacancy.Status = VacancyStatus.Closed;
        vacancy.IsActive = false;
        vacancy.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string userId, int id)
    {
        var vacancy = await _context.Vacancies
            .Include(x => x.EmployerProfile)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmployerProfile != null && x.EmployerProfile.UserId == userId);

        if (vacancy is null)
        {
            return;
        }

        vacancy.Status = VacancyStatus.Archived;
        vacancy.IsActive = false;
        vacancy.UpdatedAt = DateTime.UtcNow;
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

    public async Task<List<SelectListItem>> GetSkillsAsync()
    {
        return await _context.Skills
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }

    private async Task<Models.EmployerProfile> GetEmployerProfileAsync(string userId)
    {
        var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        return profile ?? throw new InvalidOperationException("Профіль роботодавця не знайдено.");
    }

    private async Task<int> ResolveCategoryIdAsync(string? categoryName, int fallbackCategoryId)
    {
        var normalizedName = NormalizeName(categoryName);
        if (string.IsNullOrWhiteSpace(normalizedName) && fallbackCategoryId > 0)
        {
            var fallbackCategory = await _context.Categories.FirstOrDefaultAsync(x => x.Id == fallbackCategoryId);
            if (fallbackCategory is not null)
            {
                return fallbackCategory.Id;
            }
        }

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            normalizedName = "Інше";
        }

        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedName.ToLower());

        if (existingCategory is not null)
        {
            return existingCategory.Id;
        }

        var category = new Models.Category { Name = normalizedName };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    private async Task SyncSkillsAsync(int vacancyId, List<int>? selectedSkillIds, string? skillsText)
    {
        var existingItems = await _context.VacancySkills
            .Where(x => x.VacancyId == vacancyId)
            .ToListAsync();

        _context.VacancySkills.RemoveRange(existingItems);

        var skillIds = (selectedSkillIds ?? new List<int>())
            .Where(x => x > 0)
            .Distinct()
            .ToList();
        var typedSkillNames = SplitSkillNames(skillsText);

        foreach (var skillName in typedSkillNames)
        {
            var normalizedSkillName = NormalizeName(skillName);
            if (string.IsNullOrWhiteSpace(normalizedSkillName))
            {
                continue;
            }

            var existingSkill = await _context.Skills
                .FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedSkillName.ToLower());

            if (existingSkill is null)
            {
                existingSkill = new Models.Skill { Name = normalizedSkillName };
                _context.Skills.Add(existingSkill);
                await _context.SaveChangesAsync();
            }

            skillIds.Add(existingSkill.Id);
        }

        foreach (var skillId in skillIds.Distinct())
        {
            _context.VacancySkills.Add(new Models.VacancySkill
            {
                VacancyId = vacancyId,
                SkillId = skillId
            });
        }
    }

    private static List<string> SplitSkillNames(string? skillsText)
    {
        return (skillsText ?? string.Empty)
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeName(string? value)
    {
        return string.Join(" ", (value ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}
