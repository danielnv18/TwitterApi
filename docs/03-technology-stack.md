# Technology Stack

## Overview

This document details all technologies, libraries, and tools used in the Twitter Clone project. Each choice is justified based on .NET best practices, community adoption, and learning value.

## Core Framework

### .NET 9.0
- **Version**: 9.0 (LTS release November 2024)
- **Justification**:
  - Latest long-term support version
  - Performance improvements over .NET 8
  - New C# 13 features
  - Native AOT improvements
  - Better cloud integration
  - Active support until November 2027

### C# 13
- **Features Used**:
  - Record types for immutable DTOs
  - Pattern matching
  - Nullable reference types
  - Primary constructors
  - Collection expressions
  - Init-only properties

## Web Framework

### ASP.NET Core 9.0
- **Purpose**: Web API framework
- **Features Used**:
  - Minimal APIs (for health checks)
  - Controller-based APIs (main endpoints)
  - Built-in dependency injection
  - Middleware pipeline
  - Model validation
  - OpenAPI/Swagger integration

**Package**:
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.*" />
```

## Database

### SQL Server 2022
- **Purpose**: Primary relational database
- **Justification**:
  - Industry-standard enterprise database
  - Excellent performance and scalability
  - ACID compliance
  - Advanced features (full-text search, JSON support, temporal tables)
  - Great EF Core support
  - Free Developer Edition
  - SQL Server-specific optimizations available
  - Native integration with Azure

**Docker Image**: `mcr.microsoft.com/mssql/server:2022-latest`

### Entity Framework Core 9.0
- **Purpose**: ORM (Object-Relational Mapper)
- **Justification**:
  - Official Microsoft ORM
  - Code-first migrations
  - LINQ query support
  - Change tracking
  - Database provider abstraction
  - Excellent tooling

**Packages**:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.*" />
```

**Features Used**:
- Fluent API for configuration
- Migrations for schema management
- Query optimization
- Lazy loading disabled (explicit loading)
- Change tracking for updates

**Database Provider Strategy**:
To support future SQL Server migration:
- Avoid SQL Server-specific features
- Use standard SQL types
- Use EF Core abstractions (not raw SQL)
- Test migrations on both databases (future)

**Switching to SQL Server** (future):
```xml
<!-- Replace Microsoft.Data.SqlClient package with: -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.*" />
```

## Authentication & Security

### JWT Bearer Authentication
- **Package**: `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.*)
- **Purpose**: Token-based authentication
- **Implementation**:
  - Access tokens (15 minutes)
  - Refresh tokens (30 days)
  - HS256 algorithm
  - Custom claims (userId, username)

### BCrypt.Net-Next
- **Package**: `BCrypt.Net-Next` (4.0.*)
- **Purpose**: Password hashing
- **Justification**:
  - Industry standard for password hashing
  - Built-in salting
  - Configurable work factor
  - Resistant to rainbow table attacks
  - Better than MD5, SHA-1, SHA-256 for passwords

**Alternative Considered**: Argon2 (more modern, but BCrypt is more widely adopted)

### AspNetCoreRateLimit
- **Package**: `AspNetCoreRateLimit` (5.0.*)
- **Purpose**: API rate limiting
- **Features**:
  - Per-user limits
  - Different limits per endpoint
  - Customizable time windows
  - IP-based and client-based limiting

**Configuration**:
```csharp
services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new() { Endpoint = "POST:/api/images", Period = "1h", Limit = 50 },
        new() { Endpoint = "POST:/api/posts", Period = "1h", Limit = 100 }
    };
});
```

## Validation

### FluentValidation
- **Package**: `FluentValidation.AspNetCore` (11.3.*)
- **Purpose**: Input validation for commands and queries
- **Justification**:
  - Clean, testable validation rules
  - Separation from domain models
  - Rich validation syntax
  - Async validation support
  - Better than Data Annotations for complex rules

**Example**:
```csharp
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(1, 100)
            .Matches("^[a-zA-Z0-9]+$")
            .WithMessage("Username must be alphanumeric");
    }
}
```

**Alternative Considered**: Data Annotations (simpler but less flexible)

## Background Jobs

### Hangfire
- **Packages**:
  ```xml
  <PackageReference Include="Hangfire.Core" Version="1.8.*" />
  <PackageReference Include="Hangfire.AspNetCore" Version="1.8.*" />
  <PackageReference Include="Hangfire.PostgreSql" Version="1.20.*" />
  ```
- **Purpose**: Background job processing
- **Use Cases**:
  - Image processing after upload
  - Sending emails (async)
  - Cleanup tasks (expired tokens)
  - Future: Feed generation, notifications

**Features**:
- Web dashboard (`/hangfire`)
- Retry logic
- Job scheduling
- Recurring jobs
- Job persistence (survives restarts)

**Why Hangfire?**
- Most popular .NET background job library
- Simple setup
- Great dashboard for monitoring
- SQL Server storage support
- Can scale to distributed (Redis backend)

**Alternatives Considered**:
- **Quartz.NET**: More complex, overkill for MVP
- **MassTransit**: Message queue focused, steeper learning curve
- **Built-in BackgroundService**: No persistence, no retries

## Image Processing

### SixLabors.ImageSharp
- **Packages**:
  ```xml
  <PackageReference Include="SixLabors.ImageSharp" Version="3.1.*" />
  <PackageReference Include="SixLabors.ImageSharp.Web" Version="3.1.*" />
  ```
- **Purpose**: Image manipulation and processing
- **Features Used**:
  - Resize images (maintain aspect ratio)
  - Crop images (square thumbnails)
  - Format conversion (JPEG/PNG → WebP)
  - EXIF data removal
  - Image optimization

**Why ImageSharp?**
- Pure .NET, cross-platform
- No native dependencies
- Modern, actively maintained
- Excellent performance
- Commercial-friendly license (Apache 2.0 with exceptions)

**Alternatives Considered**:
- **SkiaSharp**: Faster but native dependencies, larger size
- **System.Drawing**: Windows-only, deprecated for web apps
- **Magick.NET**: ImageMagick wrapper, more complex

**Image Variants Generated**:
```csharp
public static class ImageVariants
{
    public static readonly (string Name, int Width, int? Height)[] Variants =
    {
        ("thumbnail", 150, 150),  // Square crop
        ("small", 400, null),      // Maintain aspect ratio
        ("medium", 800, null),     // Maintain aspect ratio
        ("large", 1200, null)      // Maintain aspect ratio
    };
}
```

## Email

### MailKit
- **Packages**:
  ```xml
  <PackageReference Include="MailKit" Version="4.7.*" />
  <PackageReference Include="MimeKit" Version="4.7.*" />
  ```
- **Purpose**: Send emails (SMTP)
- **Use Cases**:
  - Email verification
  - Password reset
  - Future: Notifications, digests

**Why MailKit?**
- Industry standard for .NET email
- Full SMTP support
- Authentication support
- TLS/SSL support
- Actively maintained by Miguel de Icaza

**Alternatives Considered**:
- **System.Net.Mail.SmtpClient**: Obsolete, not recommended
- **SendGrid/Mailgun SDKs**: Vendor lock-in, costs money
- **FluentEmail**: Wrapper over MailKit, unnecessary abstraction

### Mailpit (Development)
- **Docker Image**: `axllent/mailpit:latest`
- **Purpose**: Local email testing
- **Features**:
  - SMTP server on port 1025
  - Web UI on port 8025
  - No emails actually sent
  - View/debug all emails
  - No configuration needed

**Why Mailpit?**
- Modern replacement for MailHog
- Better UI
- Faster
- Actively maintained
- Perfect for development

## Logging

### Serilog
- **Packages**:
  ```xml
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.*" />
  <PackageReference Include="Serilog.Sinks.Console" Version="6.0.*" />
  <PackageReference Include="Serilog.Sinks.File" Version="6.0.*" />
  <PackageReference Include="Serilog.Sinks.Seq" Version="8.0.*" />
  ```
- **Purpose**: Structured logging
- **Features**:
  - Structured (JSON) logging
  - Multiple sinks (console, file, Seq)
  - Contextual properties
  - Log levels
  - Performance optimized

**Configuration**:
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/twitter-clone-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

**Why Serilog?**
- Most popular .NET logging library
- Structured logging (queryable logs)
- Rich ecosystem of sinks
- Better than built-in ILogger (but compatible)

**Optional**: Seq for log viewing (http://localhost:5341)

## Object Mapping

### AutoMapper
- **Package**: `AutoMapper` (13.0.*)
- **Purpose**: Map domain entities to DTOs
- **Justification**:
  - Reduces boilerplate code
  - Centralized mapping configuration
  - Performance optimized
  - Convention-based mapping

**Example**:
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FollowerCount, opt => opt.MapFrom(src => src.Followers.Count));
            
        CreateMap<Post, PostDto>();
    }
}
```

**Alternative Considered**: Mapster (faster, but less mature)

## API Documentation

### Swashbuckle (Swagger)
- **Package**: `Swashbuckle.AspNetCore` (6.8.*)
- **Purpose**: OpenAPI/Swagger documentation
- **Features**:
  - Interactive API documentation
  - Try-it-out functionality
  - JSON/YAML schema export
  - Authentication support

**Access**: `http://localhost:5038/swagger`

**Why Swagger?**
- Industry standard API docs
- Great for frontend developers
- Automated from code annotations
- Free and open-source

## Testing

### xUnit
- **Package**: `xunit` (2.9.*)
- **Purpose**: Unit testing framework
- **Justification**:
  - Most popular .NET test framework
  - Used by Microsoft
  - Cleaner syntax than NUnit
  - Parallel test execution
  - Great Visual Studio integration

**Example**:
```csharp
public class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var handler = new RegisterCommandHandler(/* mocks */);
        var command = new RegisterCommand { /* ... */ };
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

### FluentAssertions
- **Package**: `FluentAssertions` (6.12.*)
- **Purpose**: Better test assertions
- **Features**:
  - Readable assertions
  - Rich comparison methods
  - Better error messages

**Example**:
```csharp
result.Should().NotBeNull();
user.Username.Should().Be("johndoe");
users.Should().HaveCount(5);
```

### Moq
- **Package**: `Moq` (4.20.*)
- **Purpose**: Mocking framework
- **Use Cases**:
  - Mock IApplicationDbContext
  - Mock IEmailService
  - Mock IJwtService
  - Isolate unit tests

**Example**:
```csharp
var mockEmailService = new Mock<IEmailService>();
mockEmailService
    .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
    .ReturnsAsync(true);
```

### Testcontainers
- **Package**: `Testcontainers.PostgreSql` (3.10.*)
- **Purpose**: Integration testing with real database
- **Features**:
  - Spin up SQL Server in Docker
  - Isolated test database
  - Automatic cleanup
  - Fast parallel execution

**Example**:
```csharp
public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().Build();
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
```

### Bogus
- **Package**: `Bogus` (35.6.*)
- **Purpose**: Fake data generation
- **Use Cases**:
  - Generate test users
  - Generate test posts
  - Seed test database

**Example**:
```csharp
var faker = new Faker<User>()
    .RuleFor(u => u.Username, f => f.Internet.UserName())
    .RuleFor(u => u.Email, f => f.Internet.Email())
    .RuleFor(u => u.DisplayName, f => f.Name.FullName());
    
var users = faker.Generate(100);
```

### NetArchTest
- **Package**: `NetArchTest.Rules` (1.3.*)
- **Purpose**: Architecture testing
- **Use Cases**:
  - Enforce dependency rules
  - Ensure Clean Architecture compliance
  - Prevent circular dependencies

**Example**:
```csharp
[Fact]
public void Domain_Should_Not_Depend_On_Other_Layers()
{
    var result = Types.InAssembly(DomainAssembly)
        .ShouldNot()
        .HaveDependencyOnAll("Application", "Infrastructure", "API")
        .GetResult();
        
    result.IsSuccessful.Should().BeTrue();
}
```

### WebApplicationFactory
- **Package**: `Microsoft.AspNetCore.Mvc.Testing` (9.0.*)
- **Purpose**: Integration testing for API
- **Features**:
  - In-memory API hosting
  - Real HTTP requests
  - Dependency injection override
  - Fast execution

## File Storage

### Local Filesystem (Development)
- **Implementation**: Custom `LocalFileStorage` class
- **Purpose**: Store uploaded images
- **Structure**:
  ```
  uploads/
    {userId}/
      {year}/
        {month}/
          {imageId}/
            thumbnail.webp
            small.webp
            medium.webp
            large.webp
            original.{ext}
  ```

### Future: Azure Blob Storage
- **Package**: `Azure.Storage.Blobs`
- **Migration Path**:
  1. Implement `IFileStorage` interface
  2. Register in DI
  3. Update configuration
  4. No business logic changes needed

## Development Tools

### Docker & Docker Compose
- **Version**: Latest stable
- **Purpose**: Local development environment
- **Services**:
  - SQL Server (database)
  - Mailpit (email testing)
  - PgAdmin (database management)

### Database Management

#### PgAdmin 4
- **Docker Image**: `dpage/pgadmin4:latest`
- **Purpose**: SQL Server GUI
- **Access**: http://localhost:5050
- **Features**:
  - Visual query builder
  - Schema browser
  - Data editor
  - Query execution

#### EF Core CLI Tools
- **Installation**: `dotnet tool install --global dotnet-ef`
- **Purpose**: Database migrations
- **Commands**:
  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  dotnet ef migrations remove
  ```

### IDE Support

#### Recommended IDEs
1. **Visual Studio 2022** (Windows/Mac)
   - Best tooling support
   - Integrated debugger
   - Database tools
   - Free Community Edition

2. **JetBrains Rider** (Windows/Mac/Linux)
   - Excellent performance
   - Better refactoring
   - Paid (free for students)

3. **Visual Studio Code** (All platforms)
   - Lightweight
   - Free
   - Requires C# extension

#### Required VS Code Extensions
```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "jongrant.csharpsortusings",
    "kreativ-software.csharpextensions"
  ]
}
```

## Optional Tools

### Seq (Structured Logs Viewer)
- **Docker Image**: `dphilion/seq:latest`
- **Purpose**: View structured logs
- **Access**: http://localhost:5341
- **Free**: Single-user license

### Redis (Future Caching)
- **Purpose**: Cache feeds, user profiles, counts
- **Migration**: Add when performance becomes an issue
- **Package**: `StackExchange.Redis`

### MediatR (CQRS Library)
- **Package**: `MediatR` (12.4.*)
- **Purpose**: Implement CQRS pattern
- **Features**:
  - Request/response pattern
  - Pipeline behaviors (validation, logging)
  - Decoupled handlers

**Example**:
```csharp
// Command
public record CreatePostCommand : IRequest<Result<PostDto>> { }

// Handler
public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result<PostDto>> { }

// Usage in controller
var result = await _mediator.Send(new CreatePostCommand());
```

## NuGet Package Summary

### Production Dependencies
```xml
<!-- Core -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.*" />

<!-- Database -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.*" />
<PackageReference Include="Microsoft.Data.SqlClient.EntityFrameworkCore.SQL Server" Version="9.0.*" />

<!-- Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.*" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.*" />

<!-- Background Jobs -->
<PackageReference Include="Hangfire.Core" Version="1.8.*" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.*" />
<PackageReference Include="Hangfire.PostgreSql" Version="1.20.*" />

<!-- Images -->
<PackageReference Include="SixLabors.ImageSharp" Version="3.1.*" />
<PackageReference Include="SixLabors.ImageSharp.Web" Version="3.1.*" />

<!-- Email -->
<PackageReference Include="MailKit" Version="4.7.*" />
<PackageReference Include="MimeKit" Version="4.7.*" />

<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.*" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.*" />

<!-- Mapping -->
<PackageReference Include="AutoMapper" Version="13.0.*" />

<!-- Rate Limiting -->
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.*" />

<!-- API Docs -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.*" />

<!-- CQRS (Optional but recommended) -->
<PackageReference Include="MediatR" Version="12.4.*" />
```

### Development/Testing Dependencies
```xml
<!-- Testing Framework -->
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.*" />

<!-- Assertions -->
<PackageReference Include="FluentAssertions" Version="6.12.*" />

<!-- Mocking -->
<PackageReference Include="Moq" Version="4.20.*" />

<!-- Integration Testing -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.10.*" />

<!-- Fake Data -->
<PackageReference Include="Bogus" Version="35.6.*" />

<!-- Architecture Tests -->
<PackageReference Include="NetArchTest.Rules" Version="1.3.*" />

<!-- EF Core Tools -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.*" />
```

## Version Management

### Semantic Versioning
All packages use semantic versioning: `Major.Minor.Patch`

**Package Version Strategies**:
- `9.0.*` - Allow patch updates only (safest)
- `9.*` - Allow minor updates (recommended for dev)
- `*` - Allow any updates (not recommended)

### Updating Packages
```bash
# Check for updates
dotnet list package --outdated

# Update specific package
dotnet add package Serilog.AspNetCore

# Update all packages (careful!)
dotnet outdated --upgrade
```

## Performance Considerations

### Optimizations Built-In
1. **EF Core**: Compiled queries for repeated operations
2. **Serilog**: Async logging to avoid blocking
3. **ImageSharp**: Parallel processing
4. **Hangfire**: Background processing prevents blocking API
5. **AutoMapper**: Compiled mapping expressions

### Future Optimizations
- **Redis**: Caching layer for feeds and counts
- **CDN**: Image delivery
- **Database Indexing**: Query optimization
- **Response Compression**: Gzip/Brotli
- **HTTP/2**: Multiplexing

## License Considerations

All libraries used are **commercially friendly**:
- MIT License: Most libraries (EF Core, xUnit, Serilog, etc.)
- Apache 2.0: ImageSharp, Swashbuckle
- BSD: SQL Server

**No GPL dependencies** (allows commercial use without open-sourcing your code).

## Summary

This stack represents modern .NET best practices:
- ✅ Latest .NET 9 LTS
- ✅ Industry-standard libraries
- ✅ Database-agnostic approach
- ✅ Cloud-ready architecture
- ✅ Comprehensive testing tools
- ✅ Production-ready logging and monitoring
- ✅ Active community support
- ✅ Commercial-friendly licenses

Total package count: ~30 production, ~10 development = **40 NuGet packages**

All choices optimize for:
1. **Learning value**: Teach industry patterns
2. **Maintainability**: Well-documented, actively maintained
3. **Performance**: Production-ready speed
4. **Flexibility**: Easy to swap implementations
