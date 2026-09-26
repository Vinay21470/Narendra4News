# NARENDRA4NEWS

A React and ASP.NET Core implementation in progress for a Telugu cinema news portal. The frontend and backend build and an initial EF migration is included, but the application is **not production complete**. See `docs/STATUS.md` for the remaining work.

## Structure

- `frontend`: React, TypeScript, Vite, React Router, Axios and TanStack Query
- `backend/Narendra4News.API`: .NET 9, EF Core SQL Server, ASP.NET Identity, Azure Blob Storage
- `docs`: architecture and implementation status

## Local setup

Requires Node 20+, .NET 9 SDK, SQL Server or Azure SQL, Azure Storage or Azurite. For a no-SQL local smoke test, Development supports SQLite with `DatabaseProvider=Sqlite` and `ConnectionStrings__Sqlite=Data Source=narendra4news-dev.db`; Production always uses SQL Server. No secrets are committed.

1. Create a SQL database. Set `ConnectionStrings__Sql` in the API process to its connection string.
2. Set `AzureStorage__ConnectionString` and optionally `AzureStorage__Container` (`media`). Use Azurite locally or a real Storage account.
3. Set `ADMIN_EMAIL` and `ADMIN_PASSWORD` (12+ characters) before the first API start. The admin is created only if missing. Change the password and remove these environment values afterward.
4. Apply the included EF migration with the SDK:

```sh
cd backend/Narendra4News.API
dotnet tool install --global dotnet-ef --version 9.0.0
dotnet ef database update
dotnet run --urls http://localhost:5000
```

5. Run the frontend in another terminal:

```sh
cd frontend
cp .env.example .env
npm install
npm run dev
```

Open `http://localhost:5173`. API Swagger appears at `http://localhost:5000/swagger` in Development. Browser requests use `VITE_API_BASE_URL`. For a local HTTP API, `UseHttpsRedirection` has no HTTPS port and leaves the request on HTTP.

## Admin flow

Visit `/admin`, sign in using the seeded account, create a movie, upload an image, enter daily collection figures, and publish an article linked to that movie. The homepage, article page, movie history, and search read from SQL through the API. The initial editor uses plain text. The admin UI supports article and movie creation/editing, article archiving, basic categories and dashboard counts. Collection editing, moderation and user management screens remain incomplete.

ASP.NET Identity maps its standard `/api/auth/login`, `/api/auth/register`, `/api/auth/refresh`, `/api/auth/manage/info` endpoints. `/register` currently allows user accounts, so configure a registration policy before launch. Public read endpoints serve only published articles.

## Azure deployment outline

Use `infra/main.bicep` as the initial resource template for Azure SQL, Storage with a private `media` container, and a .NET 9 App Service. Review and harden the template before public use. Configure SQL and Storage secrets as App Service settings or Key Vault references. Apply EF migrations as a controlled deployment step and set `AllowedOrigins__0` to the frontend origin. The `deploy.yml` workflow publishes the API and bundles the frontend into the same App Service. Its production API URL is relative (`/api`). Configure SPA fallback to `index.html`, HTTPS, `narendra4news.net` and `www.narendra4news.net` DNS and certificates. Use an Azure SQL identity or securely managed credentials. Set up database backups, logging, rate limiting and monitoring before accepting traffic.

The domain and Azure resources have **not** been provisioned by this repository. Do not treat this scaffold as a live deployment.

## Verification

The frontend and API build, and `tests/integration_flow.py` passes the admin to public flow on temporary SQLite and Azurite. Install Azurite (`npm install -g azurite`) and run `python3 tests/integration_flow.py` after a Release API build. The same test runs in GitHub Actions. This does **not** validate Azure SQL, live Storage, networking, DNS or custom domain.
