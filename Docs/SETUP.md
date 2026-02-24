# Setup Guide

## Requirements

| Requirement | Version |
|-------------|---------|
| .NET SDK | 8.x (LTS) |
| PostgreSQL | 14+ |
| Cloudinary account | Any plan |

---

## Environment Variables

The application is configured via `appsettings.json` or environment variable overrides. For local development, override the sensitive values using environment variables or a `.env`-compatible mechanism (e.g., `launchSettings.json` or shell exports).

### Database

```
ConnectionStrings__DefaultConnection=Host=localhost;Database=morenok_db;Username=your_user;Password=your_password
```

> Uses EF Core + Npgsql provider. The key must be `DefaultConnection` under `ConnectionStrings`.

### JWT Authentication

```
Jwt__Key=<your-secret-signing-key-min-32-chars>
Jwt__Issuer=Morenok.backend.io
Jwt__Audience=Morenok.frontend.io
```

> `Jwt__Key` must be a strong secret (32+ characters). Do **not** commit real keys to version control.

### Cloudinary (Media Uploads)

```
Cloudinary__CloudName=your_cloud_name
Cloudinary__ApiKey=your_api_key
Cloudinary__ApiSecret=your_api_secret
Cloudinary__Url=cloudinary://your_api_key:your_api_secret@your_cloud_name
```

> All four Cloudinary fields are required for asset upload/deletion to work correctly.

---

## Upload Body Size Limits

The API accepts multipart uploads up to **60 MB** per request. This is configured at the Kestrel and form-options level in `Program.cs`. No additional server-side configuration is required for local development.

---

## Running Locally

### 1. Restore Dependencies

```bash
dotnet restore
```

### 2. Apply Database Migrations

```bash
dotnet ef database update --project Backend.Infrastructure --startup-project Backend.Api
```

> Requires `dotnet-ef` CLI tool. Install it if needed:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

### 3. Run the API

```bash
dotnet run --project Backend.Api
```

The API will be available at:
- HTTP: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger` *(Development only)*

---

## EF Core Migration Commands

| Operation | Command |
|-----------|---------|
| Add a migration | `dotnet ef migrations add <Name> --project Backend.Infrastructure --startup-project Backend.Api` |
| Apply migrations | `dotnet ef database update --project Backend.Infrastructure --startup-project Backend.Api` |
| Revert last migration | `dotnet ef migrations remove --project Backend.Infrastructure --startup-project Backend.Api` |

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `Connection refused` on DB | Verify PostgreSQL is running and `DefaultConnection` string is correct. |
| `401 Unauthorized` on protected endpoints | Ensure a valid Bearer token is sent in the `Authorization` header. |
| `413 Request Entity Too Large` | Verify body size is under 60 MB. |
| Cloudinary upload fails | Double-check all four `Cloudinary__*` environment variables are set. |
| JWT validation fails | Ensure `Jwt__Issuer` and `Jwt__Audience` match the values used when the token was issued. |
