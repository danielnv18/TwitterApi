# Development Setup Guide

## Prerequisites

### Required Software

1. **.NET 9 SDK**
   ```bash
   # Check if installed
   dotnet --version
   # Should output: 9.0.x
   ```
   Download: https://dotnet.microsoft.com/download/dotnet/9.0

2. **Docker Desktop**
   - Windows: https://www.docker.com/products/docker-desktop
   - Mac: https://www.docker.com/products/docker-desktop
   - Linux: https://docs.docker.com/engine/install/
   
   ```bash
   # Verify installation
   docker --version
   docker-compose --version
   ```

3. **Git**
   ```bash
   git --version
   ```

4. **IDE (Choose One)**
   - **Visual Studio 2022** (Windows/Mac) - Recommended for beginners
   - **JetBrains Rider** (All platforms) - Best for productivity
   - **Visual Studio Code** (All platforms) - Lightweight

### Optional Tools

- **Postman** or **Insomnia**: API testing
- **DBeaver** or **DataGrip**: Alternative database client
- **Azure Data Studio**: Cross-platform database tool

---

## Initial Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd TwitterCloneApi
```

### 2. Start Docker Services

Start SQL Server, Mailpit, and Azure Data Studio:

```bash
docker-compose up -d
```

**Verify services are running**:
```bash
docker-compose ps
```

You should see:
- `twitter-clone-sqlserver` (healthy)
- `twitter-clone-mailpit` (running)

**Access Services**:
- **SQL Server**: `localhost:1433`
- **Mailpit UI**: http://localhost:8025

**Note**: Use Azure Data Studio or SQL Server Management Studio (SSMS) to manage the database.

### 3. Configure User Secrets

Set up sensitive configuration (development only):

```bash
cd src/API

# Initialize user secrets
dotnet user-secrets init

# Set JWT secret key (generate a strong random key)
dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-key-at-least-32-characters-long"

# Optional: Set connection string (if different from appsettings)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=TwitterCloneDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Generate Strong Secret Key** (PowerShell):
```powershell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | % {[char]$_})
```

**Generate Strong Secret Key** (Bash):
```bash
openssl rand -base64 64
```

### 4. Restore NuGet Packages

```bash
# From solution root
dotnet restore
```

### 5. Build Solution

```bash
dotnet build
```

### 6. Apply Database Migrations

```bash
# From solution root
dotnet ef database update --project src/Infrastructure --startup-project src/API

# Or if migrations don't exist yet, create initial migration:
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

**Verify database created**:
1. Download Azure Data Studio: https://docs.microsoft.com/sql/azure-data-studio/download
2. Or use SQL Server Management Studio (SSMS)
3. Connect to server:
   - Server: `localhost,1433`
   - Authentication: `SQL Login`
   - Username: `sa`
   - Password: `YourStrong!Passw0rd`
   - Trust Server Certificate: `Yes`
4. Verify database `TwitterCloneDb` exists

### 7. Run Application

**From solution root**:
```bash
dotnet run --project src/API/API.csproj
```

Or with hot reload (recommended for development):
```bash
dotnet watch --project src/API/API.csproj
```

**Application URLs**:
- **HTTP**: http://localhost:5038
- **Swagger UI**: http://localhost:5038/swagger
- **Hangfire Dashboard**: http://localhost:5038/hangfire

---

## Project Structure

Once set up, your solution should look like:

```
TwitterCloneApi/
├── src/
│   ├── Domain/                    # Entities, enums
│   ├── Application/               # Business logic, DTOs
│   ├── Infrastructure/            # EF Core, services
│   └── API/                       # Controllers, middleware
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── ArchitectureTests/
├── docs/                          # Documentation (this file!)
├── docker-compose.yml
└── TwitterCloneApi.sln
```

---

## IDE Configuration

### Visual Studio 2022

1. **Open Solution**: `File → Open → Project/Solution` → Select `TwitterCloneApi.sln`
2. **Set Startup Project**: Right-click `API` → Set as Startup Project
3. **Configure Launch**: Debug dropdown → `https` profile
4. **Run**: Press `F5` or click Start Debugging

**Recommended Extensions**:
- ReSharper (paid, but excellent)
- CodeMaid (free code cleanup)

### JetBrains Rider

1. **Open Solution**: `File → Open` → Select `TwitterCloneApi.sln`
2. **Run Configuration**: Should auto-detect `API`
3. **Run**: Click Run button or `Shift+F10`

**Rider auto-configures**:
- Database connections (detect from connection string)
- Docker Compose integration
- HTTP client (alternative to Postman)

### Visual Studio Code

1. **Open Folder**: `File → Open Folder` → Select `TwitterCloneApi/`
2. **Install Extensions**:
   - C# (Microsoft)
   - C# Dev Kit (Microsoft)
   - Docker (Microsoft)
   - REST Client (Huachao Mao) - optional

3. **Configure Launch** (`.vscode/launch.json`):
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/API/bin/Debug/net9.0/API.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/API",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    }
  ]
}
```

4. **Run**: `F5` or Run → Start Debugging

---

## Environment Variables

### Development (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TwitterCloneDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "EmailSettings": {
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "UseSsl": false
  }
}
```

### User Secrets (Sensitive Data)

Never commit secrets to Git. Use User Secrets:

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-here"
```

**List all secrets**:
```bash
dotnet user-secrets list
```

**Remove a secret**:
```bash
dotnet user-secrets remove "JwtSettings:SecretKey"
```

---

## Database Management

### EF Core Migrations

**Create Migration**:
```bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/API
```

**Apply Migration**:
```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/API
```

**Rollback to Specific Migration**:
```bash
dotnet ef database update <PreviousMigrationName> \
  --project src/Infrastructure \
  --startup-project src/API
```

**Remove Last Migration** (if not applied):
```bash
dotnet ef migrations remove \
  --project src/Infrastructure \
  --startup-project src/API
```

**Generate SQL Script** (for production):
```bash
dotnet ef migrations script \
  --project src/Infrastructure \
  --startup-project src/API \
  --output migration.sql
```

### Reset Database (Development Only)

```bash
# Drop database
dotnet ef database drop \
  --project src/Infrastructure \
  --startup-project src/API \
  --force

# Recreate
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/API
```

### Seed Test Data

Create a seeder class in `Infrastructure/Data/Seeders/`:

```csharp
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
            return; // Already seeded
            
        // Seed users, posts, etc.
    }
}
```

Call in `Program.cs`:
```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DatabaseSeeder.SeedAsync(context);
}
```

---

## Testing

### Run All Tests

```bash
# From solution root
dotnet test
```

### Run Specific Test Project

```bash
dotnet test tests/UnitTests
dotnet test tests/IntegrationTests
```

### Run Tests with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Watch Tests (Auto-run on change)

```bash
dotnet watch test --project tests/UnitTests
```

---

## Useful Commands

### Docker

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f sqlserver
docker-compose logs -f mailpit

# Restart service
docker-compose restart sqlserver

# Remove all data (fresh start)
docker-compose down -v
```

### .NET CLI

```bash
# Clean build artifacts
dotnet clean

# Full rebuild
dotnet clean && dotnet build

# Run with specific environment
dotnet run --environment Production

# List available runtimes
dotnet --list-runtimes

# List installed SDKs
dotnet --list-sdks
```

### NuGet Packages

```bash
# Add package
dotnet add package Serilog.AspNetCore

# Remove package
dotnet remove package Serilog.AspNetCore

# List packages
dotnet list package

# Check for updates
dotnet list package --outdated
```

---

## Troubleshooting

### Issue: Port Already in Use

**Symptoms**: `Failed to bind to address http://localhost:5038`

**Solution**:
1. Change port in `launchSettings.json`:
```json
"applicationUrl": "http://localhost:5039"
```

2. Or kill process using port:
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:5000 | xargs kill -9
```

### Issue: Cannot Connect to Database

**Symptoms**: `could not connect to server: Connection refused`

**Solution**:
1. Check Docker is running: `docker ps`
2. Check SQL Server is healthy: `docker-compose ps`
3. Restart SQL Server: `docker-compose restart sqlserver`
4. Check connection string in `appsettings.json`

### Issue: Migration Fails

**Symptoms**: `Unable to create an object of type 'ApplicationDbContext'`

**Solution**:
1. Ensure connection string is correct
2. Rebuild solution: `dotnet build`
3. Add design-time factory in Infrastructure project:

```csharp
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=twitter_clone_dev;Username=twitter_user;Password=dev_password_2024");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
```

### Issue: User Secrets Not Working

**Symptoms**: JWT secret key is null or default

**Solution**:
1. Verify secrets initialized: `dotnet user-secrets list`
2. Ensure correct project: `cd src/API`
3. Check `csproj` has `<UserSecretsId>`:
```xml
<PropertyGroup>
  <UserSecretsId>aspnet-TwitterCloneApi-12345</UserSecretsId>
</PropertyGroup>
```

### Issue: Swagger Not Loading

**Symptoms**: 404 on `/swagger`

**Solution**:
1. Ensure in Development environment
2. Check `Program.cs` has:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### Issue: Emails Not Sending

**Symptoms**: Email verification not received

**Solution**:
1. Check Mailpit is running: `docker ps | grep mailpit`
2. Check Mailpit UI: http://localhost:8025
3. Verify SMTP settings in `appsettings.json`:
```json
"EmailSettings": {
  "SmtpHost": "localhost",
  "SmtpPort": 1025,
  "UseSsl": false
}
```

---

## Next Steps

After setup is complete:

1. **Explore Swagger**: http://localhost:5038/swagger
2. **Test Registration**: `POST /api/auth/register`
3. **Check Email in Mailpit**: http://localhost:8025
4. **View Database**: http://localhost:5050
5. **Monitor Background Jobs**: http://localhost:5038/hangfire

---

## Development Workflow

### Daily Development

1. **Start Docker**:
   ```bash
   docker-compose up -d
   ```

2. **Run App**:
   ```bash
   cd src/API
   dotnet watch run
   ```

3. **Make Changes**: Edit code, save, app auto-restarts

4. **Test Endpoint**: Use Swagger or Postman

5. **Check Logs**: Console output or Serilog files

6. **End of Day**:
   ```bash
   # Optional: Stop Docker services to save resources
   docker-compose down
   ```

### Adding New Feature

1. **Create Branch**:
   ```bash
   git checkout -b feature/new-feature
   ```

2. **Implement Domain Entity** (if needed)

3. **Create Migration**:
   ```bash
   dotnet ef migrations add AddNewFeature
   dotnet ef database update
   ```

4. **Implement Application Layer** (Command/Query)

5. **Implement Infrastructure** (if needed)

6. **Create Controller Endpoint**

7. **Write Tests**

8. **Test Manually** (Swagger)

9. **Commit**:
   ```bash
   git add .
   git commit -m "feat: add new feature"
   ```

---

## Performance Tips

### Development Mode Optimizations

1. **Disable Detailed Errors** in production
2. **Use Development Database**: Smaller dataset
3. **Enable Hot Reload**: `dotnet watch`
4. **Use In-Memory Caching**: For frequently accessed data

### Database Tips

1. **Create Indexes** for frequent queries
2. **Use `.AsNoTracking()`** for read-only queries
3. **Eager Load** related entities: `.Include()`
4. **Batch Operations**: Use `AddRange()` instead of multiple `Add()`

---

## Useful Resources

### Documentation
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core)
- [SQL Server Docs](https://www.sqlserverql.org/docs/)

### Learning
- [Microsoft Learn - .NET](https://learn.microsoft.com/dotnet)
- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com)
- [EF Core Best Practices](https://learn.microsoft.com/ef/core/performance/)

### Community
- [r/dotnet Reddit](https://reddit.com/r/dotnet)
- [C# Discord](https://discord.gg/csharp)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/.net)

---

## Summary

✅ **Setup Complete!**

You now have:
- .NET 9 API running
- SQL Server database with migrations
- Mailpit for email testing
- Azure Data Studio for database management
- Swagger for API documentation
- Hot reload for fast development

**Start coding!** 🚀
