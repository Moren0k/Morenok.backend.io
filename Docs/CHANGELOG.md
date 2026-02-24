# Changelog

All notable changes to this project will be documented in this file.

This project adheres to Keep a Changelog and Semantic Versioning (SemVer).

---

## [Unreleased]

### Added

### Changed

### Fixed

### Removed

---

## [1.0.0] - 2026-02-24

### Added

- Clean Architecture project structure: Api, Application, Domain, Infrastructure.
- JWT Bearer authentication with full token validation (issuer, audience, lifetime, signing key).
- `POST /api/auth/register` — user registration with optional `PortfolioSlug`.
- `POST /api/auth/login` — credential-based login returning a signed JWT.
- `GET /api/me` — authenticated endpoint returning the current user's profile.
- `GET /api/portfolio/{portfolioSlug}/projects` — public portfolio endpoint resolving slug to owner and returning published projects.
- `PortfolioSlug` support on the `User` entity with deterministic normalization via `SlugHelper`.
- Technologies catalog: `GET /api/technologies` and `POST /api/technologies`.
- Project CRUD with full asset orchestration:
  - `GET /api/projects/admin` — admin list of all owner projects.
  - `POST /api/projects` — multipart upload with cover image and optional demo video.
  - `PUT /api/projects/{projectId}` — full update with asset replacement and compensation rollback.
  - `DELETE /api/projects/{projectId}` — deletion with DB-first asset cleanup.
- Cloudinary integration for image and video asset upload and deletion.
- Global exception middleware returning RFC 7807 `ProblemDetails` responses.
- Rate limiting on public portfolio endpoint (fixed window: 60 req/min).
- Multipart upload support up to 60 MB (Kestrel + FormOptions configured).

### Changed

- Domain entity mutation no longer relies on reflection; properties are set via explicit setters.
- Project deletion logic is DB-first: assets are resolved from the database before Cloudinary deletion.
- Hardening improvements: `OwnerId` is always derived from the JWT claim, never from the request body.

### Security

- All admin endpoints require a valid JWT Bearer token (`RequireAuthorization()`).
- `OwnerId` is extracted exclusively from the `owner_id` JWT claim; clients cannot supply it.
- Public portfolio endpoint is rate-limited to prevent enumeration and abuse.

---

## [0.0.1] - 2026-02-20

### Added

- .NET 8 solution bootstrap with Clean Architecture: Api, Application, Domain, Infrastructure.
- Configured cross-project references.
- Initial .gitignore and README.

---

<details>
<summary>Changelog Format & Rules</summary>

### Versioning

Format: Major.Minor.Patch (e.g., 1.2.3)

- Major → Breaking changes
- Minor → Backward-compatible features
- Patch → Backward-compatible bug fixes

### Change Types

- Added → New features
- Changed → Changes in existing functionality
- Fixed → Bug fixes
- Removed → Removed features
- Security → Security-related fixes

### Workflow

1. All ongoing work must be documented under **Unreleased**.
2. When publishing a release:
   - Create a new version block with number and date.
   - Move changes from **Unreleased** into that version.
3. New releases must be added above older ones.
4. Published versions must never be modified.

### Unreleased Section

    ## [Unreleased]

    ### Added

    ### Changed

    ### Fixed

    ### Removed

</details>

---
