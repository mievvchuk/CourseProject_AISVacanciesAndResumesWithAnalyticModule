# AIS for Vacancies and Resumes with Analytics Module

Coursework ASP.NET Core MVC project for automating job search and recruitment workflows. The system supports candidate, employer, and administrator roles, and includes resumes, vacancies, applications, recommendations, notifications, messaging, moderation, and analytics.

## Key Features

- Registration, authentication, and role-based access for `Candidate`, `Employer`, and `Admin`.
- Candidate and employer profile management.
- Resume creation, editing, viewing, archiving, and deletion.
- PDF/DOCX resume upload with automatic parsing.
- Vacancy creation, editing, viewing, archiving, deletion, and moderation.
- Search and filtering for vacancies, resumes, and candidates.
- Saved search filters.
- Applications without duplicate submissions for the same vacancy.
- Resume-to-vacancy matching percentage calculation.
- Vacancy recommendations for candidates and candidate recommendations for employers.
- Analytics dashboard with statistics visualization.
- Notifications, private messages, and an administrative panel.
- CSV/PDF export for vacancies and analytics.

## Technology Stack

- ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL
- ASP.NET Identity
- Razor Views
- Bootstrap / AdminLTE
- xUnit

## Project Structure

```text
AisVacanciesAndResumes
|-- Controllers                  MVC and API request handlers
|-- Data                         ApplicationDbContext, migrations, seed data
|-- Enums                        System statuses, roles, and types
|-- Models                       Database entities
|-- Services                     Business logic and application workflows
|-- ViewModels                   Models for forms and pages
|-- Views                        Razor pages
|-- wwwroot                      CSS, JavaScript, libraries, and static files
|-- AisVacanciesAndResumes.Tests Automated tests
```

## Development Test Accounts

- Admin: `admin@example.com` / `Admin123!`
- Candidate: `candidate@example.com` / `Candidate123!`
- Employer: `employer@example.com` / `Employer123!`

## Run Locally

```bash
dotnet restore
dotnet build
dotnet run
```

Apply database migrations:

```bash
dotnet ef database update
```

## Verification

```bash
dotnet build
dotnet test
```

The current test suite covers key business scenarios: resumes, vacancies, applications, matching, recommendations, analytics, export, notifications, and admin workflows.

## Git Flow

The repository is organized according to Git Flow:

- `main` - stable state for demonstration.
- `develop` - integration branch.
- `feature/*` - separate feature branches.
- `release/pre-defense` - preparation branch before pre-defense.
- `v1.0.0-pre-defense` - release-state tag.

## Author

Mykhailo Yevchuk
