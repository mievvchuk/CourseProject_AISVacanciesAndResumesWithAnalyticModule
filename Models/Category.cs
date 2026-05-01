namespace AisVacanciesAndResumes.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
