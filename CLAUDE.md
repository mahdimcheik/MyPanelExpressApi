# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Current state

The API skeleton is in place: EF Core + PostgreSQL, ASP.NET Core Identity (single admin, no roles), JWT auth, Swagger, MinIO storage, and an SMTP/RazorLight mail path for the contact form. **No domain entities exist yet** — `Realisation`, `Service`, `Actualite`, and `Parametres` from the spec are still to be written, and there is no frontend.

Working endpoints: `POST /auth/login`, `GET /auth/me`, `POST /media/upload`, `GET /media/url`, `DELETE /media`, `POST /contact`.

## The spec: `MyPanelExpress/userstory.md`

A brochure site ("site vitrine") for a sign/large-format printing company. **The public site is in Arabic (RTL)**; the code, comments, and API messages are in French. The spec is the source of truth for scope and data model. Load-bearing points:

- **Public site** (anonymous): home, about, services, gallery of past work ("réalisations"), news ("actualités"), contact form. SEO and responsive layout are explicit requirements.
- **Admin back-office**: exactly **one** administrator account. No user registration, no roles, no multi-tenancy. No e-commerce or payment of any kind.
- **Four entities**: `Réalisation` (multi-image), `Service`, `Actualité`, `Paramètres` (singleton: company contact info, hours, social links). All content entities carry a `Visible` bool — admin hides content rather than deleting it, so **never filter by deletion; filter by `Visible`**, and the public endpoints must return only visible rows.
- Images live in **MinIO**, not on disk. The database stores the **object key**; the presigned URL is regenerated at read time via `MediaService` (presigned links expire after 24h, so never persist a URL).

## Commands

Run from the solution root (`C:\git\MyPanelExpress\MyPanelExpress`):

```powershell
docker compose up -d db minio mailpit   # infra must be up first
dotnet run --project MyPanelExpress     # http://localhost:5196 — Swagger UI at /
dotnet ef migrations add <Name> --project MyPanelExpress
```

Migrations are applied automatically at startup (`Database.MigrateAsync()`), the MinIO bucket is created if missing (`EnsureBucketAsync`), and the admin is seeded from `ADMIN_EMAIL`/`ADMIN_PASSWORD` on first boot only. Dev UIs: MinIO console `:9001`, Mailpit `:8025`, pgAdmin `:5050`. There is no test project yet.

## Configuration — env vars only

All config goes through `Utilities/EnvironmentVariables.cs`, a static class reading `Environment.GetEnvironmentVariable` with fallbacks. **There is no `IConfiguration`/`IOptions` binding, and `appsettings.json` holds only logging.** Services read `EnvironmentVariables.X` statically.

Dev values come from a git-ignored `.env` at the solution root (copy `.env.example`), loaded into the process by `DotEnv.Load()` — which **must stay the first statement in `Program.cs`**, because `dotnet ef` runs top-level statements up to `Build()` and needs the connection string. The same `.env` feeds docker-compose, so dev and prod share one set of names. The `UserSecretsId` in the csproj is unused; user-secrets are invisible to `Environment.GetEnvironmentVariable`.

`EnvironmentVariables.Validate()` throws at startup if `JWT_KEY`, `ADMIN_PASSWORD`, or `MINIO_SECRETKEY` are still at their dev defaults outside Development.

## Conventions

Ported from the BonProf reference project (`c:\git\BonProfAPI\BonProf`, attached as additional working directories — a different application; take its style, never its domain):

- **Services return a `Response<T>` envelope** (`{ Message, Status, Data, Count }`), never bare entities and never thrown exceptions across the boundary. Each public method wraps its body in try/catch and returns `Status = 500`. Controllers stay thin: `return StatusCode(response.Status, response);`.
- **Service class per aggregate**, primary-constructor DI: `public class RealisationService(MainContext context) : IRealisationService`. Interfaces in `Services/Interfaces/`. `MinioService` and `MailService` are the exceptions — concrete, injected by type.
- **DTOs live beside the entity** in `<Entity>DTO.cs`: `<Entity>Details` for reads (constructor takes the entity, maps navs defensively) and `Create<Entity>`/`Update<Entity>` for writes. Entities are never serialized directly.
- Entities derive from `BaseModel` (`Models/Interfaces/Interfaces.cs`): `Guid` id, `CreatedAt`/`UpdatedAt`/`ArchivedAt` as `DateTimeOffset` with `[Column(TypeName = "timestamp with time zone")]`.
- **Always `DateTimeOffset.UtcNow`, never `.Now`.** Npgsql refuses to write a `DateTimeOffset` whose offset isn't zero to `timestamp with time zone` — `.Now` carries the local offset and throws on every insert.
- Identity comes from the `ClaimsPrincipal`, never the request body. `Utilities/CheckUser.cs` resolves the user from the `NameIdentifier` claim.
- **User-facing `Message` strings and doc comments are in French.** Use the spec's vocabulary (réalisation, actualité, paramètres).

## Gotchas

- **Swashbuckle is pinned to 7.x on purpose.** It compiles against Microsoft.OpenApi 1.6.x. Re-adding `Microsoft.AspNetCore.OpenApi` pulls Microsoft.OpenApi 2.x, which reshapes `OpenApiSecurityScheme.Reference` and breaks `ConfigureSwagger` at compile time.
- Identity is registered with `AddIdentityCore` + `IdentityUserContext` (no roles ⇒ no `AspNetRoles` tables). Consequently there is **no `SignInManager`**, so `CheckPasswordAsync` does not touch lockout counters — `AuthService.Login` wires lockout by hand (`IsLockedOutAsync` / `AccessFailedAsync` / `ResetAccessFailedCountAsync`). Keep that if you touch the login path.
- Uploads go through `ImageProcessing.ToWebpAsync`, which re-encodes to webp. The MIME/extension check is a cheap pre-filter; the real guard is ImageSharp's decode throwing `UnknownImageFormatException` on a non-image.
