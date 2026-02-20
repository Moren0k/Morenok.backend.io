# Setup Guide

## Project Overview
This repository contains a .NET 8 backend application implemented with Clean Architecture. It provides a structured approach to separating concerns across API, Application, Domain, and Infrastructure layers.

## Requirements
- .NET 8 SDK
- COMMING
- Git

## Environment Variables
The application requires the following environment variables to be set for database connectivity.

### ConnectionStrings__Default
Connection string for COMMING.

Example:
```
Host=your_host;Database=your_db;Username=your_user;Password=your_password
```

## Setup Guide

### Restore Dependencies
```bash
dotnet restore
```

### Build the Solution
```bash
dotnet build
```

### Run the API
```bash
dotnet run --project Backend.Api
```

## EF Core Migrations

### Add a Migration
```bash
dotnet ef migrations add [MigrationName] --project Backend.Infrastructure --startup-project Backend.Api
```

### Update the Database
```bash
dotnet ef database update --project Backend.Infrastructure --startup-project Backend.Api
```

## Local URL Configuration
- Swagger UI: `http://localhost:[PORT]/swagger`
- API Endpoint: `http://localhost:PORT`

## Production SSL Notes
Production deployments must use SSL/TLS. This can be achieved by configuring a reverse proxy such as Nginx or Apache to terminate SSL, or by configuring the .NET Kestrel server with a valid certificate. Ensure SSL certificates are properly configured in the production environment.
