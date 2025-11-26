# TwitterCloneApp

A Twitter/X clone application with a .NET 9 API and Vue.js frontend, demonstrating Clean Architecture and modern practices.

## Tech Stack

### Backend
- **.NET 9** - Latest LTS framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 9** - ORM
- **SQL Server 2022** - Database
- **Clean Architecture** - Separation of concerns
- **CQRS** - MediatR pattern
- **Hangfire** - Background jobs
- **ImageSharp** - Image processing
- **MailKit** - Email
- **xUnit** - Testing

### Frontend
- **Vue.js 3** - Progressive JavaScript Framework
- **TypeScript** - Typed JavaScript
- **Vite** - Build tool
- **Pinia** - State management
- **Vue Router** - Routing

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (LTS recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

## Quick Start

### 1. Clone Repository
```bash
git clone <repository-url>
cd TwitterCloneApp
```

### 2. Start Docker Services
```bash
docker-compose up -d
```

This starts:
- SQL Server 2022 (port 1433)
- Mailpit (port 8025 - email testing UI)

### 3. Backend Setup

#### Configure User Secrets
```bash
cd src/API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-key-at-least-32-characters-long"
```

#### Run Migrations
```bash
cd ../..
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

#### Run API
```bash
dotnet run --project src/API/API.csproj
```

The API will be available at:
- **HTTP**: http://localhost:5038
- **Swagger UI**: http://localhost:5038/swagger

### 4. Frontend Setup

#### Install Dependencies
```bash
cd src/Client
npm install
```

#### Run Client
```bash
npm run dev
```

The client will be available at http://localhost:5173 (default Vite port).

### 5. Test Email (Optional)
View sent emails at: http://localhost:8025

## 📖 Documentation

Comprehensive documentation available in the `docs/` folder:

- [Development Setup Guide](docs/10-development-setup.md)
- [API Specification](docs/06-api-specification.md)
- [System Architecture](docs/02-system-architecture.md)
- [Database Design](docs/04-database-design.md)

## Running Tests

### Backend
```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/UnitTests

# Integration tests only
dotnet test tests/IntegrationTests
```

### Frontend
```bash
cd src/Client
npm run test:unit
```

## Project Structure

```
TwitterCloneApp/
├── src/                 # Source code
│   ├── Domain/          # Entities, enums (no dependencies)
│   ├── Application/     # Business logic, CQRS
│   ├── Infrastructure/  # EF Core, services
│   ├── API/             # Controllers, middleware
│   └── Client/          # Vue.js frontend application
├── tests/               # Backend tests
└── docs/                # Documentation
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with .NET 9 & Vue.js** | For detailed documentation, see the `docs/` folder
