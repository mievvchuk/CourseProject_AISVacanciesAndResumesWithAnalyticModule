using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Data;

public static class DbInitializer
{
    private const string AdminEmail = "admin@example.com";
    private const string AdminPassword = "Admin123!";
    private const string CandidateEmail = "candidate@example.com";
    private const string CandidatePassword = "Candidate123!";
    private const string EmployerEmail = "employer@example.com";
    private const string EmployerPassword = "Employer123!";

    private static readonly string[] CategoryNames =
    [
        "Розробка ПЗ",
        "Дизайн",
        "Маркетинг",
        "Продажі",
        "Освіта",
        "Медицина",
        "Фінанси",
        "Адміністрування",
        "Логістика",
        "Підтримка клієнтів"
    ];

    private static readonly string[] SkillNames =
    [
        "C#",
        ".NET",
        "ASP.NET Core",
        "Entity Framework Core",
        "PostgreSQL",
        "SQL",
        "JavaScript",
        "TypeScript",
        "React",
        "HTML",
        "CSS",
        "Bootstrap",
        "Figma",
        "UI/UX",
        "Docker",
        "Git",
        "CRM",
        "Excel",
        "Комунікація",
        "Переговори",
        "Управління проєктами",
        "Клієнтська підтримка",
        "Англійська мова"
    ];

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

        await context.Database.MigrateAsync();
        await EnsureLegacySchemaAsync(context);
        await EnsureRolesAsync(roleManager);

        if (environment.IsDevelopment())
        {
            await ResetDevelopmentDemoDataAsync(context, userManager);
            await EnsureReferenceDataAsync(context, reset: true);
            await EnsureDevelopmentUsersAsync(userManager);
            await EnsureDevelopmentDemoContentAsync(context, userManager);
            return;
        }

        await EnsureReferenceDataAsync(context, reset: false);
        await EnsureProductionAdminAsync(userManager);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in Enum.GetNames<UserRoleType>())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task EnsureProductionAdminAsync(UserManager<User> userManager)
    {
        var admin = await EnsureUserAsync(userManager, AdminEmail, AdminPassword, "Адміністратор системи", UserRoleType.Admin, resetPassword: false);
        admin.IsActive = true;
        await userManager.UpdateAsync(admin);
    }

    private static async Task EnsureDevelopmentUsersAsync(UserManager<User> userManager)
    {
        await EnsureUserAsync(userManager, AdminEmail, AdminPassword, "Адміністратор системи", UserRoleType.Admin, resetPassword: true);
        await EnsureUserAsync(userManager, CandidateEmail, CandidatePassword, "Іван Петренко", UserRoleType.Candidate, resetPassword: true);
        await EnsureUserAsync(userManager, "olena.candidate@example.com", CandidatePassword, "Олена Коваль", UserRoleType.Candidate, resetPassword: true);
        await EnsureUserAsync(userManager, "dmytro.candidate@example.com", CandidatePassword, "Дмитро Савчук", UserRoleType.Candidate, resetPassword: true);
        await EnsureUserAsync(userManager, EmployerEmail, EmployerPassword, "Марія Бондар", UserRoleType.Employer, resetPassword: true);
        await EnsureUserAsync(userManager, "techcorp@example.com", EmployerPassword, "TechCorp Ukraine", UserRoleType.Employer, resetPassword: true);
        await EnsureUserAsync(userManager, "designhub@example.com", EmployerPassword, "DesignHub Studio", UserRoleType.Employer, resetPassword: true);
    }

    private static async Task<User> EnsureUserAsync(
        UserManager<User> userManager,
        string email,
        string password,
        string fullName,
        UserRoleType role,
        bool resetPassword)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                IsActive = true
            };

            await ThrowIfFailedAsync(userManager.CreateAsync(user, password));
        }

        user.UserName = email;
        user.Email = email;
        user.EmailConfirmed = true;
        user.FullName = fullName;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await ThrowIfFailedAsync(userManager.UpdateAsync(user));

        if (resetPassword)
        {
            if (await userManager.HasPasswordAsync(user))
            {
                await ThrowIfFailedAsync(userManager.RemovePasswordAsync(user));
            }

            await ThrowIfFailedAsync(userManager.AddPasswordAsync(user, password));
            await userManager.UpdateSecurityStampAsync(user);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var targetRole = role.ToString();
        var rolesToRemove = currentRoles.Where(x => !string.Equals(x, targetRole, StringComparison.OrdinalIgnoreCase)).ToList();
        if (rolesToRemove.Count > 0)
        {
            await ThrowIfFailedAsync(userManager.RemoveFromRolesAsync(user, rolesToRemove));
        }

        if (!await userManager.IsInRoleAsync(user, targetRole))
        {
            await ThrowIfFailedAsync(userManager.AddToRoleAsync(user, targetRole));
        }

        return user;
    }

    private static async Task ThrowIfFailedAsync(Task<IdentityResult> identityTask)
    {
        var result = await identityTask;
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException(errors);
        }
    }

    private static async Task ResetDevelopmentDemoDataAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        context.ResumeSkills.RemoveRange(context.ResumeSkills);
        context.VacancySkills.RemoveRange(context.VacancySkills);
        context.Applications.RemoveRange(context.Applications);
        context.Notifications.RemoveRange(context.Notifications);
        context.Messages.RemoveRange(context.Messages);
        context.ModerationLogs.RemoveRange(context.ModerationLogs);
        context.PortfolioItems.RemoveRange(context.PortfolioItems);
        context.SavedSearches.RemoveRange(context.SavedSearches);
        context.Resumes.RemoveRange(context.Resumes);
        context.Vacancies.RemoveRange(context.Vacancies);
        context.CandidateProfiles.RemoveRange(context.CandidateProfiles);
        context.EmployerProfiles.RemoveRange(context.EmployerProfiles);
        context.Skills.RemoveRange(context.Skills);
        context.Categories.RemoveRange(context.Categories);
        await context.SaveChangesAsync();

        var demoEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AdminEmail,
            CandidateEmail,
            "olena.candidate@example.com",
            "dmytro.candidate@example.com",
            EmployerEmail,
            "techcorp@example.com",
            "designhub@example.com"
        };

        var usersToDelete = await userManager.Users
            .Where(x => x.Email != null && !demoEmails.Contains(x.Email))
            .ToListAsync();

        foreach (var user in usersToDelete)
        {
            await ThrowIfFailedAsync(userManager.DeleteAsync(user));
        }
    }

    private static async Task EnsureReferenceDataAsync(ApplicationDbContext context, bool reset)
    {
        if (!reset)
        {
            await NormalizeCategoriesAsync(context);
            await NormalizeSkillsAsync(context);
            return;
        }

        foreach (var categoryName in CategoryNames)
        {
            context.Categories.Add(new Category { Name = categoryName });
        }

        foreach (var skillName in SkillNames)
        {
            context.Skills.Add(new Skill { Name = skillName, Category = GuessSkillCategory(skillName) });
        }

        await context.SaveChangesAsync();
    }

    private static async Task NormalizeCategoriesAsync(ApplicationDbContext context)
    {
        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Software Development"] = "Розробка ПЗ",
            ["Design"] = "Дизайн",
            ["Marketing"] = "Маркетинг",
            ["Sales"] = "Продажі",
            ["Education"] = "Освіта",
            ["Healthcare"] = "Медицина",
            ["Finance"] = "Фінанси",
            ["Administration"] = "Адміністрування",
            ["Logistics"] = "Логістика",
            ["Customer Support"] = "Підтримка клієнтів"
        };

        foreach (var categoryName in CategoryNames)
        {
            if (!await context.Categories.AnyAsync(x => x.Name == categoryName))
            {
                context.Categories.Add(new Category { Name = categoryName });
            }
        }

        await context.SaveChangesAsync();

        foreach (var (oldName, newName) in replacements)
        {
            var oldCategory = await context.Categories.FirstOrDefaultAsync(x => x.Name == oldName);
            if (oldCategory is null)
            {
                continue;
            }

            var targetCategory = await context.Categories.FirstAsync(x => x.Name == newName);
            await context.Vacancies.Where(x => x.CategoryId == oldCategory.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CategoryId, targetCategory.Id));
            await context.Resumes.Where(x => x.CategoryId == oldCategory.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CategoryId, targetCategory.Id));
            context.Categories.Remove(oldCategory);
            await context.SaveChangesAsync();
        }
    }

    private static async Task NormalizeSkillsAsync(ApplicationDbContext context)
    {
        foreach (var skillName in SkillNames)
        {
            if (!await context.Skills.AnyAsync(x => x.Name == skillName))
            {
                context.Skills.Add(new Skill { Name = skillName, Category = GuessSkillCategory(skillName) });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDevelopmentDemoContentAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        var categories = await context.Categories.ToDictionaryAsync(x => x.Name, x => x.Id);
        var skills = await context.Skills.ToDictionaryAsync(x => x.Name, x => x.Id);

        var adminUser = await GetDemoUserAsync(userManager, AdminEmail);
        var candidateUser = await GetDemoUserAsync(userManager, CandidateEmail);
        var olenaUser = await GetDemoUserAsync(userManager, "olena.candidate@example.com");
        var dmytroUser = await GetDemoUserAsync(userManager, "dmytro.candidate@example.com");
        var employerUser = await GetDemoUserAsync(userManager, EmployerEmail);
        var techCorpUser = await GetDemoUserAsync(userManager, "techcorp@example.com");
        var designHubUser = await GetDemoUserAsync(userManager, "designhub@example.com");

        var candidateProfile = AddCandidateProfile(context, candidateUser.Id, "Junior .NET Developer", "Початковий розробник з досвідом ASP.NET Core, SQL та Bootstrap.", "Київ", 1, ExperienceLevel.Junior, EducationLevel.Bachelor, 35000);
        var olenaProfile = AddCandidateProfile(context, olenaUser.Id, "UI/UX Designer", "Дизайнерка інтерфейсів із фокусом на Figma, прототипування та користувацькі сценарії.", "Львів", 3, ExperienceLevel.Middle, EducationLevel.Bachelor, 45000);
        var dmytroProfile = AddCandidateProfile(context, dmytroUser.Id, "Project Manager", "Менеджер проєктів з досвідом координації команд і роботи з клієнтами.", "Харків", 5, ExperienceLevel.Senior, EducationLevel.Master, 60000);

        var employerProfile = AddEmployerProfile(context, employerUser.Id, "AIC Tech", "Розробка ПЗ", "Компанія створює вебсистеми для малого та середнього бізнесу.", "51-200", "Київ");
        var techCorpProfile = AddEmployerProfile(context, techCorpUser.Id, "TechCorp Ukraine", "ІТ", "Продуктова компанія з фокусом на enterprise-рішення.", "201-500", "Львів");
        var designHubProfile = AddEmployerProfile(context, designHubUser.Id, "DesignHub Studio", "Дизайн", "Студія цифрового дизайну для українських та міжнародних клієнтів.", "11-50", "Одеса");
        await context.SaveChangesAsync();

        var resumes = new List<Resume>
        {
            AddResume(context, candidateProfile.Id, categories["Розробка ПЗ"], "Резюме Junior .NET Developer", "Junior .NET Developer", "Маю практичний досвід створення MVC-додатків, роботи з базами даних і Git.", "Бакалавр комп'ютерних наук", "Навчальні та pet-проєкти на ASP.NET Core MVC.", "C#, .NET, ASP.NET Core, SQL, HTML, CSS, Bootstrap", EmploymentType.FullTime, 1, ExperienceLevel.Junior, 35000, true),
            AddResume(context, candidateProfile.Id, categories["Розробка ПЗ"], "Резюме Frontend Developer", "Frontend Developer", "Створюю адаптивні інтерфейси та компоненти для вебзастосунків.", "Курси frontend-розробки", "Верстка сторінок, інтеграція з API, базовий React.", "JavaScript, TypeScript, React, HTML, CSS", EmploymentType.Remote, 2, ExperienceLevel.Junior, 40000, true),
            AddResume(context, olenaProfile.Id, categories["Дизайн"], "Резюме UI/UX Designer", "UI/UX Designer", "Проєктую зрозумілі інтерфейси та дизайн-системи.", "Бакалавр дизайну", "Комерційні лендинги, SaaS-кабінети, мобільні прототипи.", "Figma, UI/UX, Комунікація", EmploymentType.Hybrid, 3, ExperienceLevel.Middle, 45000, true),
            AddResume(context, olenaProfile.Id, categories["Маркетинг"], "Резюме Digital Designer", "Digital Designer", "Готую креативи для реклами, соцмереж і презентацій.", "Курси digital design", "Візуали для маркетингових кампаній.", "Figma, Комунікація, Англійська мова", EmploymentType.PartTime, 2, ExperienceLevel.Middle, 30000, false),
            AddResume(context, dmytroProfile.Id, categories["Адміністрування"], "Резюме Project Manager", "Project Manager", "Керую задачами, ризиками та комунікацією між командами.", "Магістр менеджменту", "5 років у продуктових та сервісних командах.", "Управління проєктами, Excel, Комунікація, Переговори", EmploymentType.FullTime, 5, ExperienceLevel.Senior, 60000, true),
            AddResume(context, dmytroProfile.Id, categories["Підтримка клієнтів"], "Резюме Customer Success Manager", "Customer Success Manager", "Допомагаю клієнтам успішно впроваджувати цифрові продукти.", "Бакалавр менеджменту", "Підтримка B2B-клієнтів, CRM, навчання користувачів.", "CRM, Клієнтська підтримка, Комунікація, Англійська мова", EmploymentType.FullTime, 4, ExperienceLevel.Middle, 50000, true)
        };

        await context.SaveChangesAsync();
        foreach (var resume in resumes)
        {
            AddResumeSkills(context, resume, skills);
        }

        var vacancies = new List<Vacancy>
        {
            AddVacancy(context, employerProfile.Id, categories["Розробка ПЗ"], "Junior .NET Developer", "Розробка та підтримка MVC-додатків для внутрішніх бізнес-процесів.", "C#, .NET, ASP.NET Core, SQL, Git, бажання навчатися.", 30000, 50000, EmploymentType.FullTime, ExperienceLevel.Junior, "Київ", VacancyStatus.Published),
            AddVacancy(context, employerProfile.Id, categories["Підтримка клієнтів"], "Customer Support Specialist", "Підтримка користувачів платформи, обробка звернень і робота з базою знань.", "CRM, Комунікація, Клієнтська підтримка, Англійська мова.", 25000, 40000, EmploymentType.FullTime, ExperienceLevel.Junior, "Київ", VacancyStatus.Published),
            AddVacancy(context, employerProfile.Id, categories["Адміністрування"], "Project Coordinator", "Координація задач команди, підготовка звітів і комунікація з клієнтами.", "Управління проєктами, Excel, Комунікація.", 35000, 55000, EmploymentType.Hybrid, ExperienceLevel.Middle, "Київ", VacancyStatus.UnderModeration),
            AddVacancy(context, techCorpProfile.Id, categories["Розробка ПЗ"], "Senior Frontend Developer", "Розробка клієнтської частини продукту на React.", "React, TypeScript, HTML, CSS, досвід з API.", 80000, 120000, EmploymentType.Hybrid, ExperienceLevel.Senior, "Львів", VacancyStatus.Published),
            AddVacancy(context, techCorpProfile.Id, categories["Розробка ПЗ"], "Backend Developer C#", "Розробка backend-сервісів та інтеграцій.", "C#, .NET, PostgreSQL, Entity Framework Core, Docker.", 70000, 110000, EmploymentType.Remote, ExperienceLevel.Middle, "Львів", VacancyStatus.Published),
            AddVacancy(context, techCorpProfile.Id, categories["Фінанси"], "Data Analyst", "Підготовка фінансових звітів і аналітичних дашбордів.", "SQL, Excel, уважність до даних.", 45000, 70000, EmploymentType.FullTime, ExperienceLevel.Middle, "Львів", VacancyStatus.UnderModeration),
            AddVacancy(context, designHubProfile.Id, categories["Дизайн"], "UI/UX Designer", "Проєктування інтерфейсів для SaaS-продуктів.", "Figma, UI/UX, прототипування, комунікація з клієнтом.", 45000, 70000, EmploymentType.Hybrid, ExperienceLevel.Middle, "Одеса", VacancyStatus.Published),
            AddVacancy(context, designHubProfile.Id, categories["Маркетинг"], "Digital Marketing Specialist", "Запуск рекламних кампаній та аналіз результатів.", "Маркетинг, аналітика, Комунікація, Excel.", 35000, 55000, EmploymentType.FullTime, ExperienceLevel.Middle, "Одеса", VacancyStatus.Published),
            AddVacancy(context, designHubProfile.Id, categories["Продажі"], "Sales Manager B2B", "Продаж цифрових послуг і ведення клієнтів.", "CRM, Переговори, Комунікація.", 30000, 65000, EmploymentType.FullTime, ExperienceLevel.Middle, "Одеса", VacancyStatus.Published),
            AddVacancy(context, techCorpProfile.Id, categories["Розробка ПЗ"], "PHP Developer", "Підтримка застарілого вебпроєкту.", "PHP, SQL, відповідальність.", 30000, 50000, EmploymentType.Remote, ExperienceLevel.Middle, "Віддалено", VacancyStatus.Rejected)
        };

        await context.SaveChangesAsync();
        foreach (var vacancy in vacancies)
        {
            AddVacancySkills(context, vacancy, skills);
        }

        vacancies[9].ModerationComment = "Опис вакансії потребує уточнення вимог і умов роботи.";
        vacancies[9].ModeratedAt = DateTime.UtcNow.AddDays(-1);
        vacancies[9].ModeratedByUserId = adminUser.Id;

        AddApplication(context, resumes[0], vacancies[0], candidateUser.Id, 86, ApplicationStatus.Reviewed, "Зацікавлений у вакансії та готовий виконати тестове завдання.");
        AddApplication(context, resumes[2], vacancies[6], olenaUser.Id, 92, ApplicationStatus.Accepted, "Маю релевантні кейси у Figma.");
        AddApplication(context, resumes[4], vacancies[8], dmytroUser.Id, 74, ApplicationStatus.New, "Маю досвід B2B-комунікацій.");

        AddPortfolio(context, candidateProfile.Id, "Система управління навчанням", "Навчальний проєкт на ASP.NET Core MVC.", "https://github.com/example/lms");
        AddPortfolio(context, olenaProfile.Id, "Дизайн кабінету кандидата", "Прототип особистого кабінету у Figma.", "https://example.com/design");

        AddNotification(context, candidateUser.Id, "Заявку переглянуто", "Роботодавець переглянув вашу заявку на Junior .NET Developer.", NotificationType.Info);
        AddNotification(context, employerUser.Id, "Нова заявка кандидата", "Кандидат подав заявку на вакансію Junior .NET Developer.", NotificationType.Info);
        AddNotification(context, techCorpUser.Id, "Вакансія на модерації", "Вакансія Data Analyst очікує перевірки адміністратором.", NotificationType.Warning);
        AddNotification(context, adminUser.Id, "Є вакансії на модерації", "Перевірте нові вакансії роботодавців.", NotificationType.Info);

        context.ModerationLogs.Add(new ModerationLog
        {
            AdminUserId = adminUser.Id,
            EntityName = nameof(Vacancy),
            EntityId = vacancies[9].Id,
            ActionType = ModerationActionType.Rejected,
            Note = "Вакансію «PHP Developer» відхилено. Коментар: опис потребує уточнення."
        });

        context.Messages.Add(new Message
        {
            SenderId = employerUser.Id,
            ReceiverId = candidateUser.Id,
            Subject = "Тестове завдання",
            Content = "Дякуємо за заявку. Надсилаємо коротке тестове завдання.",
            IsRead = false
        });

        await context.SaveChangesAsync();
    }

    private static async Task<User> GetDemoUserAsync(UserManager<User> userManager, string email)
    {
        return await userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException($"Demo user {email} was not created.");
    }

    private static CandidateProfile AddCandidateProfile(ApplicationDbContext context, string userId, string headline, string summary, string city, int years, ExperienceLevel level, EducationLevel education, decimal salary)
    {
        var profile = new CandidateProfile
        {
            UserId = userId,
            Headline = headline,
            Summary = summary,
            City = city,
            ExperienceYears = years,
            ExperienceLevel = level,
            EducationLevel = education,
            DesiredEmploymentType = EmploymentType.FullTime,
            DesiredSalary = salary
        };
        context.CandidateProfiles.Add(profile);
        return profile;
    }

    private static EmployerProfile AddEmployerProfile(ApplicationDbContext context, string userId, string companyName, string industry, string description, string size, string city)
    {
        var profile = new EmployerProfile
        {
            UserId = userId,
            CompanyName = companyName,
            Industry = industry,
            Description = description,
            CompanySize = size,
            Website = "https://example.com",
            City = city,
            Location = city,
            FoundedYear = 2018
        };
        context.EmployerProfiles.Add(profile);
        return profile;
    }

    private static Resume AddResume(ApplicationDbContext context, int profileId, int categoryId, string title, string position, string summary, string education, string experience, string skills, EmploymentType employmentType, int years, ExperienceLevel level, decimal salary, bool published)
    {
        var resume = new Resume
        {
            CandidateProfileId = profileId,
            CategoryId = categoryId,
            Title = title,
            DesiredPosition = position,
            Summary = summary,
            Education = education,
            Experience = experience,
            SkillsDescription = skills,
            EmploymentType = employmentType,
            ExperienceYears = years,
            ExperienceLevel = level,
            EducationLevel = EducationLevel.Bachelor,
            DesiredSalary = salary,
            IsPublished = published,
            Status = published ? ResumeStatus.Published : ResumeStatus.Draft,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        context.Resumes.Add(resume);
        return resume;
    }

    private static Vacancy AddVacancy(ApplicationDbContext context, int profileId, int categoryId, string title, string description, string requirements, decimal salaryFrom, decimal salaryTo, EmploymentType employmentType, ExperienceLevel level, string location, VacancyStatus status)
    {
        var vacancy = new Vacancy
        {
            EmployerProfileId = profileId,
            CategoryId = categoryId,
            Title = title,
            Description = description,
            Requirements = requirements,
            SalaryFrom = salaryFrom,
            SalaryTo = salaryTo,
            EmploymentType = employmentType,
            ExperienceLevel = level,
            Location = location,
            Status = status,
            IsActive = status == VacancyStatus.Published,
            PublishedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 20)),
            UpdatedAt = DateTime.UtcNow
        };
        context.Vacancies.Add(vacancy);
        return vacancy;
    }

    private static void AddResumeSkills(ApplicationDbContext context, Resume resume, Dictionary<string, int> skills)
    {
        foreach (var skillName in SkillNames.Where(x => resume.SkillsDescription.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            context.ResumeSkills.Add(new ResumeSkill { ResumeId = resume.Id, SkillId = skills[skillName], SkillLevel = SkillLevel.Intermediate });
        }
    }

    private static void AddVacancySkills(ApplicationDbContext context, Vacancy vacancy, Dictionary<string, int> skills)
    {
        var text = $"{vacancy.Description} {vacancy.Requirements}";
        foreach (var skillName in SkillNames.Where(x => text.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            context.VacancySkills.Add(new VacancySkill { VacancyId = vacancy.Id, SkillId = skills[skillName], SkillLevel = SkillLevel.Intermediate });
        }
    }

    private static void AddApplication(ApplicationDbContext context, Resume resume, Vacancy vacancy, string candidateUserId, int matchingPercent, ApplicationStatus status, string coverLetter)
    {
        context.Applications.Add(new Application
        {
            ResumeId = resume.Id,
            VacancyId = vacancy.Id,
            CandidateUserId = candidateUserId,
            MatchingPercent = matchingPercent,
            Status = status,
            CoverLetter = coverLetter,
            AppliedAt = DateTime.UtcNow.AddDays(-2),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });
    }

    private static void AddPortfolio(ApplicationDbContext context, int profileId, string title, string description, string url)
    {
        context.PortfolioItems.Add(new PortfolioItem
        {
            CandidateProfileId = profileId,
            Title = title,
            Description = description,
            Url = url,
            ImagePath = string.Empty
        });
    }

    private static void AddNotification(ApplicationDbContext context, string userId, string title, string content, NotificationType type)
    {
        context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = type,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static string GuessSkillCategory(string skillName)
    {
        return skillName switch
        {
            "Figma" or "UI/UX" => "Дизайн",
            "CRM" or "Переговори" or "Клієнтська підтримка" or "Комунікація" or "Англійська мова" => "Комунікація",
            "Excel" or "Управління проєктами" => "Менеджмент",
            _ => "Технічні навички"
        };
    }

    private static async Task EnsureLegacySchemaAsync(ApplicationDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_name = 'PortfolioItems'
                      AND column_name = 'ImagePath'
                ) THEN
                    ALTER TABLE "PortfolioItems"
                    ADD COLUMN "ImagePath" text NOT NULL DEFAULT '';
                END IF;
            END
            $$;
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE "CandidateProfiles" ADD COLUMN IF NOT EXISTS "PhotoPath" text;
            ALTER TABLE "CandidateProfiles" ADD COLUMN IF NOT EXISTS "DesiredSalary" numeric;
            ALTER TABLE "CandidateProfiles" ADD COLUMN IF NOT EXISTS "ExperienceLevel" integer NOT NULL DEFAULT 2;
            ALTER TABLE "CandidateProfiles" ADD COLUMN IF NOT EXISTS "EducationLevel" integer NOT NULL DEFAULT 2;
            ALTER TABLE "EmployerProfiles" ADD COLUMN IF NOT EXISTS "LogoPath" text;
            ALTER TABLE "SavedSearches" ADD COLUMN IF NOT EXISTS "City" text NOT NULL DEFAULT '';
            ALTER TABLE "SavedSearches" ADD COLUMN IF NOT EXISTS "CategoryId" integer;
            ALTER TABLE "SavedSearches" ADD COLUMN IF NOT EXISTS "EmploymentType" integer;
            ALTER TABLE "SavedSearches" ADD COLUMN IF NOT EXISTS "ExperienceLevel" integer;
            ALTER TABLE "AspNetUsers" ADD COLUMN IF NOT EXISTS "FullName" character varying(200) NOT NULL DEFAULT '';
            ALTER TABLE "AspNetUsers" ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT TRUE;
            ALTER TABLE "AspNetUsers" ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "AspNetUsers" ADD COLUMN IF NOT EXISTS "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "DesiredPosition" text NOT NULL DEFAULT '';
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "Education" text NOT NULL DEFAULT '';
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "Experience" text NOT NULL DEFAULT '';
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "SkillsDescription" text NOT NULL DEFAULT '';
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "EmploymentType" integer NOT NULL DEFAULT 1;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "ExperienceLevel" integer NOT NULL DEFAULT 2;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "EducationLevel" integer NOT NULL DEFAULT 2;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "DesiredSalary" numeric;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "Status" integer NOT NULL DEFAULT 1;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "IsPublished" boolean NOT NULL DEFAULT FALSE;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "FilePath" text;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "OriginalFileName" text;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "ContentType" text;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "FileSize" bigint;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "UploadedAt" timestamp with time zone;
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "Resumes" ADD COLUMN IF NOT EXISTS "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "Vacancies" ADD COLUMN IF NOT EXISTS "EmploymentType" integer NOT NULL DEFAULT 1;
            ALTER TABLE "Vacancies" ADD COLUMN IF NOT EXISTS "ExperienceLevel" integer NOT NULL DEFAULT 2;
            ALTER TABLE "Vacancies" ADD COLUMN IF NOT EXISTS "Status" integer NOT NULL DEFAULT 2;
            ALTER TABLE "Vacancies" ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT TRUE;
            ALTER TABLE "Vacancies" ADD COLUMN IF NOT EXISTS "PublishedAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "Applications" ADD COLUMN IF NOT EXISTS "CoverLetter" text;
            ALTER TABLE "Applications" ADD COLUMN IF NOT EXISTS "MatchingPercent" integer NOT NULL DEFAULT 0;
            ALTER TABLE "Applications" ADD COLUMN IF NOT EXISTS "Status" integer NOT NULL DEFAULT 1;
            ALTER TABLE "Applications" ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "Messages" ADD COLUMN IF NOT EXISTS "Subject" text NOT NULL DEFAULT '';
            ALTER TABLE "Messages" ADD COLUMN IF NOT EXISTS "IsRead" boolean NOT NULL DEFAULT FALSE;
            ALTER TABLE "Messages" ADD COLUMN IF NOT EXISTS "SentAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "Notifications" ADD COLUMN IF NOT EXISTS "Title" text NOT NULL DEFAULT '';
            ALTER TABLE "Notifications" ADD COLUMN IF NOT EXISTS "Content" text NOT NULL DEFAULT '';
            ALTER TABLE "Notifications" ADD COLUMN IF NOT EXISTS "Type" integer NOT NULL DEFAULT 1;
            ALTER TABLE "Notifications" ADD COLUMN IF NOT EXISTS "IsRead" boolean NOT NULL DEFAULT FALSE;
            ALTER TABLE "Notifications" ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "ModerationLogs" ADD COLUMN IF NOT EXISTS "ActionType" integer NOT NULL DEFAULT 1;
            ALTER TABLE "ModerationLogs" ADD COLUMN IF NOT EXISTS "Note" text NOT NULL DEFAULT '';
            ALTER TABLE "ModerationLogs" ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
            ALTER TABLE "ResumeSkills" ADD COLUMN IF NOT EXISTS "SkillLevel" integer NOT NULL DEFAULT 2;
            ALTER TABLE "VacancySkills" ADD COLUMN IF NOT EXISTS "SkillLevel" integer NOT NULL DEFAULT 2;
            """);
    }
}
