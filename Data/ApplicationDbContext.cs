using AisVacanciesAndResumes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Application> Applications => Set<Application>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ModerationLog> ModerationLogs => Set<ModerationLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<ResumeSkill> ResumeSkills => Set<ResumeSkill>();
    public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<VacancySkill> VacancySkills => Set<VacancySkill>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(200);
            entity.Property(x => x.StatusComment).HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        builder.Entity<Application>(entity =>
        {
            entity.Property(x => x.AppliedAt).HasDefaultValueSql("NOW()");
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        });

        builder.Entity<CandidateProfile>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<CandidateProfile>().Property(x => x.DesiredEmploymentType).HasDefaultValue(Enums.EmploymentType.FullTime);
        builder.Entity<CandidateProfile>().HasOne(x => x.User).WithOne(x => x.CandidateProfile).HasForeignKey<CandidateProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<CandidateProfile>().HasMany(x => x.Resumes).WithOne(x => x.CandidateProfile).HasForeignKey(x => x.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<CandidateProfile>().HasMany(x => x.PortfolioItems).WithOne(x => x.CandidateProfile).HasForeignKey(x => x.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EmployerProfile>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<EmployerProfile>().HasOne(x => x.User).WithOne(x => x.EmployerProfile).HasForeignKey<EmployerProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<EmployerProfile>().HasMany(x => x.Vacancies).WithOne(x => x.EmployerProfile).HasForeignKey(x => x.EmployerProfileId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Category>().HasMany(x => x.Vacancies).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Category>().HasMany(x => x.Resumes).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Resume>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        builder.Entity<Vacancy>(entity =>
        {
            entity.Property(x => x.PublishedAt).HasDefaultValueSql("NOW()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        builder.Entity<Skill>().Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Entity<SavedSearch>().Property(x => x.SearchType).HasDefaultValue(Enums.SearchType.Vacancies);

        builder.Entity<ResumeSkill>().HasKey(x => new { x.ResumeId, x.SkillId });
        builder.Entity<VacancySkill>().HasKey(x => new { x.VacancyId, x.SkillId });
        builder.Entity<ResumeSkill>().HasOne(x => x.Resume).WithMany(x => x.ResumeSkills).HasForeignKey(x => x.ResumeId);
        builder.Entity<ResumeSkill>().HasOne(x => x.Skill).WithMany(x => x.ResumeSkills).HasForeignKey(x => x.SkillId);
        builder.Entity<VacancySkill>().HasOne(x => x.Vacancy).WithMany(x => x.VacancySkills).HasForeignKey(x => x.VacancyId);
        builder.Entity<VacancySkill>().HasOne(x => x.Skill).WithMany(x => x.VacancySkills).HasForeignKey(x => x.SkillId);

        builder.Entity<Application>().HasOne(x => x.Resume).WithMany(x => x.Applications).HasForeignKey(x => x.ResumeId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Application>().HasOne(x => x.Vacancy).WithMany(x => x.Applications).HasForeignKey(x => x.VacancyId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Application>().HasOne(x => x.CandidateUser).WithMany(x => x.Applications).HasForeignKey(x => x.CandidateUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Application>().HasIndex(x => new { x.VacancyId, x.CandidateUserId }).IsUnique();

        builder.Entity<Notification>().HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<SavedSearch>().HasOne(x => x.User).WithMany(x => x.SavedSearches).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<SavedSearch>().HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Message>().HasOne(x => x.Sender).WithMany(x => x.SentMessages).HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Message>().HasOne(x => x.Receiver).WithMany(x => x.ReceivedMessages).HasForeignKey(x => x.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ModerationLog>().HasOne(x => x.AdminUser).WithMany(x => x.ModerationLogs).HasForeignKey(x => x.AdminUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Resume>().HasOne(x => x.ModeratedByUser).WithMany().HasForeignKey(x => x.ModeratedByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Vacancy>().HasOne(x => x.ModeratedByUser).WithMany().HasForeignKey(x => x.ModeratedByUserId).OnDelete(DeleteBehavior.SetNull);

        // Reference categories and skills are initialized by DbInitializer to avoid
        // duplicate English/Ukrainian seed data in demonstrations.
    }
}
