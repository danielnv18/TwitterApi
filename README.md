# TwitterCloneApi

A Twitter/X clone REST API built with .NET 9, demonstrating Clean Architecture and modern C# practices.

## Tech Stack

- **.NET 9** - Latest LTS framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 9** - ORM
- **PostgreSQL 16** - Database
- **Clean Architecture** - Separation of concerns
- **CQRS** - MediatR pattern
- **Hangfire** - Background jobs
- **ImageSharp** - Image processing
- **MailKit** - Email
- **xUnit** - Testing

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

## Quick Start

### 1. Clone Repository
```bash
git clone <repository-url>
cd TwitterCloneApi
```

### 2. Start Docker Services
```bash
docker-compose up -d
```

This starts:
- PostgreSQL (port 5432)
- Mailpit (port 8025 - email testing UI)
- PgAdmin (port 5050 - optional)

### 3. Configure User Secrets
```bash
cd src/API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-key-at-least-32-characters-long"
```

### 4. Run Migrations
```bash
cd ../..
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

### 5. Run Application
```bash
cd src/API
dotnet run
```

The API will be available at:
- **HTTPS**: https://localhost:7000
- **HTTP**: http://localhost:5000
- **Swagger UI**: https://localhost:7000/swagger

### 6. Test Email (Optional)
View sent emails at: http://localhost:8025

## 📖 Documentation

Comprehensive documentation available in the `docs/` folder:

- [Development Setup Guide](docs/10-development-setup.md)
- [API Specification](docs/06-api-specification.md)
- [System Architecture](docs/02-system-architecture.md)
- [Database Design](docs/04-database-design.md)

## Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/UnitTests

# Integration tests only
dotnet test tests/IntegrationTests
```

## Project Structure

```
src/
├── Domain/              # Entities, enums (no dependencies)
├── Application/         # Business logic, CQRS
├── Infrastructure/      # EF Core, services
└── API/                 # Controllers, middleware
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with .NET 9** | For detailed documentation, see the `docs/` folder
