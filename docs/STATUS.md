# Delivery status

The frontend and .NET API compile successfully in the authoring environment. The initial EF Core migration and an idempotent SQL script are committed. They have **not** been run against an Azure SQL database. The full admin to public end-to-end flow passes a local SQLite + Azurite test, including image upload. It has not passed against Azure SQL and live Blob Storage.

## Implemented

- Azure SQL EF Core model and migration for articles, movies, historical collection records, categories, media, comments and ASP.NET Identity
- Public article/movie/category/search APIs and admin create, update, archive/delete APIs; protected admin routes
- Blob image upload with validation and a media streaming route that works with a private container
- Responsive React frontend with homepage, article, movie history, search, article/movie editing and creation forms
- GitHub Actions build and manual deployment workflow; initial Azure Bicep infrastructure draft
- Basic sitemap and robots endpoints, article page title and description

## Still required before production

- End-to-end tests with Azure SQL and Blob Storage, including the create, publish, search and historical collection flow
- Complete collection edit/delete, category editing, media management, user management, comment moderation and analytics dashboard
- HTML rich text editor with server-side sanitization, image management, article preview and scheduling
- Proper SEO rendering for crawlers, canonical/Open Graph/Twitter/JSON-LD, sitemap under the public host
- Account registration policy, password reset and enforced admin initial password change; audit logs, rate limits, refined validation, error handling and secure token storage
- View deduplication and analytics aggregates, moderation UI, likes and search pagination across all result types
- SQL network restriction, least privilege identity, Key Vault, Blob managed identity, monitoring, backups and restore tests
- Domain DNS/certificates, public legal/editorial pages, content licensing, accessibility and performance testing

Do not deploy this as a public production news website yet.
