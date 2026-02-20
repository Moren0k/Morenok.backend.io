# Backend API — Morenok.backend.io v0.0.1

REST API developed in .NET 8 (LTS) following a lightweight Clean Architecture structure, with clear separation of responsibilities to keep the code maintainable, scalable, and decoupled from the infrastructure.

---

## Project Structure

Morenok.backend.io/

- Backend.sln
- Backend.Api            → HTTP Layer (Controllers, configuration, Swagger)
- Backend.Application    → Use cases and contracts
- Backend.Domain         → Entities and business rules
- Backend.Infrastructure → Persistence and technical implementations
- README.md

---

## Architecture

Backend.Domain

- Entities
- Value Objects
- Rules and invariants
- Does not depend on any other project

Backend.Application

- Use cases (Commands / Queries)
- Interfaces (repositories, services)
- Depends only on Domain

Backend.Infrastructure

- Persistence implementation (EF Core + PostgreSQL)
- Concrete repositories
- Depends on Application and Domain

Backend.Api

- HTTP Endpoints
- DTOs
- Dependency configuration
- Depends on Application and Infrastructure

---

## Dependencies

Application → Domain  
Infrastructure → Application + Domain  
Api → Application + Infrastructure  

Domain has no dependencies.

---

## Requirements

- .NET SDK 8.x
- PostgreSQL (Render)

---

## Initial Objective

Build a REST API to manage dynamic content (e.g., projects for a landing page) with:

- PostgreSQL persistence
- EF Core
- Clean Architecture
- Ready for deployment on Render

---

## Documentation

- [Setup Guide](Docs/SETUP.md)  
  This document provides detailed instructions for setting up the local development environment, configuring environment variables, and running the application. It should be the first point of reference for any developer new to the project.

- [Changelog](Docs/CHANGELOG.md)  
  This file maintains a chronological record of all significant changes, including new features, improvements, and bug fixes. Developers should consult this to stay informed about the project's evolution and recent updates.

---

Current status: solution created, references configured, and compiling correctly.
