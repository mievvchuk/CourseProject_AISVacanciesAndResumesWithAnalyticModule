# Railway Deploy

## 1. Push to GitHub

1. Commit the project files.
2. Push the repository to GitHub.
3. Keep secrets out of `appsettings.json`. Use Railway Variables for production secrets.

## 2. Create a Railway project

1. Open Railway and create a new project.
2. Choose Deploy from GitHub repo.
3. Select this repository.
4. Railway should detect the `Dockerfile` and build the ASP.NET Core app with Docker.

## 3. Add PostgreSQL

1. In the Railway project, add a PostgreSQL service.
2. Wait until PostgreSQL is provisioned.
3. Open the web service variables and add the production connection string.

## 4. Required web service variables

Add these variables to the Railway web service:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
```

If your PostgreSQL service has a different name, replace `Postgres` with the actual Railway service name or copy the generated `DATABASE_URL` value directly into `ConnectionStrings__DefaultConnection`.

The application also supports these fallback variables if you prefer to expose the database URL directly:

```text
DATABASE_PRIVATE_URL=${{Postgres.DATABASE_PRIVATE_URL}}
DATABASE_URL=${{Postgres.DATABASE_URL}}
POSTGRES_URL=${{Postgres.DATABASE_URL}}
POSTGRESQL_URL=${{Postgres.DATABASE_URL}}
```

Railway PostgreSQL URLs usually look like `postgresql://user:password@host:port/database`. The app converts that URL to the Npgsql connection string format automatically.

Railway sets `PORT` automatically. The app reads `PORT` and listens on that port. If `PORT` is missing, it uses `8080`.

## 5. Redeploy

1. After adding variables, open the web service.
2. Click Redeploy.
3. Check the deployment logs.

## 6. Generate a domain

1. Open the web service settings.
2. Go to Networking.
3. Generate a Railway domain.
4. Open the generated URL and verify the home page, login, registration, vacancies, and admin pages.

## 7. Database migrations and seed data

`Program.cs` calls `DbInitializer.InitializeAsync` outside the Testing environment. The initializer runs EF Core migrations with `Database.MigrateAsync()` and then creates required roles, reference data, and initial admin data.

Do not manually create tables in Railway PostgreSQL. Let EF Core migrations create and update the schema.

## 8. Typical Railway log issues

- `Connection string 'DefaultConnection' was not found.`  
  Add `ConnectionStrings__DefaultConnection` to the web service variables, or add `DATABASE_URL`/`DATABASE_PRIVATE_URL`.

- `The ConnectionString property has not been initialized.`  
  The variable exists but is empty or points to the wrong Railway service reference. Check that the Postgres service is named `Postgres`, or update the variable to use the actual service name.

- `password authentication failed` or `connection refused`  
  Check that the web service variable points to the Railway PostgreSQL connection string and that PostgreSQL is running.

- App starts but Railway shows no page  
  Check that the app listens on Railway `PORT`. This project configures `builder.WebHost.UseUrls(...)` from `PORT`.

- Migration errors on startup  
  Check the EF Core migration logs. Do not delete migrations or existing Railway database data unless you intentionally reset the environment.

- DataProtection key warnings  
  The app stores keys in `App_Data/DataProtectionKeys`. On ephemeral containers these keys may be recreated after redeploy, which can sign users out. For a coursework demo this is usually acceptable.

## 9. Local development

The local `appsettings.json` connection string is kept for development:

```json
"DefaultConnection": "Host=localhost;Port=5432;Database=AisVacanciesAndResumesDb;Username=postgres"
```

For local passwords, use `appsettings.Development.json`, user secrets, or environment variables. Do not commit real production passwords or tokens.
