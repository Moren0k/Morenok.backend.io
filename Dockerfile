# ─── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching for restore)
COPY Backend.sln ./
COPY Backend.Api/Backend.Api.csproj                         Backend.Api/
COPY Backend.Application/Backend.Application.csproj         Backend.Application/
COPY Backend.Domain/Backend.Domain.csproj                   Backend.Domain/
COPY Backend.Infrastructure/Backend.Infrastructure.csproj   Backend.Infrastructure/

RUN dotnet restore

# Copy remaining source
COPY Backend.Api/            Backend.Api/
COPY Backend.Application/    Backend.Application/
COPY Backend.Domain/         Backend.Domain/
COPY Backend.Infrastructure/ Backend.Infrastructure/

# Publish the API project
RUN dotnet publish Backend.Api/Backend.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Default environment — override at runtime if needed
ENV ASPNETCORE_ENVIRONMENT=Production

# Bind to all interfaces on port 8080 by default.
# Render injects PORT at runtime; the entrypoint script uses it if present.
EXPOSE 8080

# Use a shell entrypoint so that $PORT expansion works correctly at runtime.
# If Render sets PORT=10000, this will bind on that port automatically.
ENTRYPOINT ["/bin/sh", "-c", "dotnet Backend.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
