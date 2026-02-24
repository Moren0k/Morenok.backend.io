# Morenok.backend.io

A multi-tenant SaaS portfolio backend built with **.NET 8** and **Clean Architecture**.

Each registered user gets their own portfolio namespace (via a unique `portfolioSlug`), enabling multiple independent portfolios to be served from a single API instance.

---

## Features

- JWT-based authentication
- Project CRUD with Cloudinary media uploads
- Public portfolio endpoint by portfolio slug
- Technologies catalog
- Global error handling (ProblemDetails / RFC 7807)
- Rate limiting on public endpoints

---

## Project Structure

```
Morenok.backend.io/
├── Backend.Api            → HTTP layer (endpoints, middleware, configuration)
├── Backend.Application    → Use cases, interfaces, DTOs
├── Backend.Domain         → Entities, enums, base classes
├── Backend.Infrastructure → EF Core, PostgreSQL, Cloudinary, JWT
└── Docs/
    ├── SETUP.md           → Local development setup guide
    ├── API.md             → HTTP API reference
    └── CHANGELOG.md       → Version history
```

---

## Documentation

| Document | Purpose |
|----------|---------|
| [SETUP.md](Docs/SETUP.md) | Environment variables, migration commands, and how to run locally |
| [API.md](Docs/API.md) | Full HTTP endpoint reference with request/response examples |
| [CHANGELOG.md](Docs/CHANGELOG.md) | Version history following Keep a Changelog + SemVer |

---

## Quick Start

See [Docs/SETUP.md](Docs/SETUP.md) for full setup instructions.

```bash
dotnet restore
dotnet ef database update --project Backend.Infrastructure --startup-project Backend.Api
dotnet run --project Backend.Api
```
