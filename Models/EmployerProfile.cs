namespace AisVacanciesAndResumes.Models;

public class EmployerProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanySize { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int? FoundedYear { get; set; }
    public string? LogoPath { get; set; }

    public User? User { get; set; }
    public ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
