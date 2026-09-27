# Repository review: 27 September 2026

## Confirmed defects fixed

- The initial Azure Bicep template used semicolons as property separators. Bicep compilation failed, despite the application CI passing. The syntax is corrected and the template now compiles with no warnings. Storage names are derived from `uniqueString` to meet Azure naming constraints; the prefix has length constraints.
- The admin article list requested a protected API without its bearer token. The request now includes the current token.
- Editing an article discarded SEO keywords. The editor now retains them.
- Editing a movie through its short form discarded fields returned by the API but not displayed in that form. The existing record is retained when opening the form.
- Azure deployment could run without executing CI. It now depends on the reusable validation workflow and serializes production deployments.

## New automated checks

- Azure Bicep compilation on PRs and main.
- A disposable SQL Server 2022 service runs the committed EF migration and the same create/publish/search/history flow as the existing SQLite test.
- The integration flow verifies that anonymous admin reads are rejected and authenticated admin reads include the newly created article.

SQL Server testing verifies relational schema and provider behavior. It does not establish Azure subscription quota, networking, Azure RBAC, live Blob access, custom-domain DNS, or production readiness.

## Remaining release blockers

- API input validation and conflict handling are incomplete (especially update, duplicate slug and duplicate collection routes).
- Rich text, scheduling, complete collection/category/media/user management and moderation UI are unfinished.
- Identity password change is not enforced; password reset/email delivery, audit logging and rate limiting are unfinished.
- The template still uses a SQL administrator connection and the broad Azure-services firewall rule; move to least privilege access and restricted networking before public release.
- Startup applies migrations. Move schema deployment into an explicit controlled step before scaling the application.
- Reliable crawler rendering, complete SEO metadata, page-view deduplication and analytics are unfinished.

The public repository and passing builds should not be described as a production deployment.
