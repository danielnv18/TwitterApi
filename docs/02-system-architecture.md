# System Architecture

## Architectural Pattern: Clean Architecture

This project follows **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture) as popularized by Robert C. Martin (Uncle Bob). This pattern emphasizes separation of concerns and dependency inversion.

### Core Principles

1. **Independence from Frameworks**: Business logic doesn't depend on ASP.NET Core, EF Core, etc.
2. **Testability**: Business rules can be tested without UI, database, or external services
3. **Independence from UI**: The API could be replaced with a GraphQL API, gRPC, or any other interface
4. **Independence from Database**: PostgreSQL could be swapped for SQL Server, MongoDB, etc.
5. **Independence from External Agencies**: Business rules don't know about external services

### The Dependency Rule

**Dependencies only point inward.** Inner layers never depend on outer layers.

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│              (API, Controllers, Middleware)             │
│  - Depends on: Application                              │
│  - References: Application, Infrastructure              │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                 Infrastructure Layer                     │
│       (EF Core, Email, File Storage, Hangfire)          │
│  - Depends on: Application                              │
│  - Implements: Application interfaces                   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  Application Layer                      │
│     (Business Logic, DTOs, Commands, Queries)           │
│  - Depends on: Domain                                   │
│  - Defines: Interfaces for Infrastructure               │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                    Domain Layer                         │
│           (Entities, Value Objects, Enums)              │
│  - Depends on: Nothing                                  │
│  - Pure C# classes, no external dependencies            │
└─────────────────────────────────────────────────────────┘
```

## Project Structure

### Solution Layout

```
TwitterCloneApi/
├── src/
│   ├── Domain/                        # Core business entities
│   ├── Application/                   # Business logic, use cases
│   ├── Infrastructure/                # External concerns, implementations
│   └── API/                           # Web API, controllers, middleware
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── ArchitectureTests/
├── docker-compose.yml
└── TwitterCloneApi.sln
```

**Note**: Folder names do NOT have prefixes, but namespaces DO:
```csharp
// Folder: src/Domain/Entities/User.cs
namespace TwitterCloneApi.Domain.Entities;

// Folder: src/Application/Features/Auth/
namespace TwitterCloneApi.Application.Features.Auth;
```

## Layer Details

### 1. Domain Layer (Core)

**Purpose**: Contains enterprise business rules and domain entities.

**Characteristics**:
- No dependencies on any other layer or external libraries
- Pure C# classes
- Represents the business domain
- Most stable layer (changes least frequently)

**Contains**:
```
src/Domain/
├── Entities/
│   ├── User.cs                    # User aggregate root
│   ├── Post.cs                    # Post aggregate root
│   ├── Image.cs                   # Image entity
│   ├── Like.cs                    # Like entity
│   ├── Follow.cs                  # Follow relationship
│   ├── Bookmark.cs                # Bookmark entity
│   ├── Notification.cs            # Notification entity
│   ├── Hashtag.cs                 # Hashtag entity
│   ├── PostHashtag.cs             # Junction table
│   ├── PostImage.cs               # Junction table
│   ├── RefreshToken.cs            # Refresh token entity
│   ├── EmailVerificationToken.cs  # Verification token
│   └── PasswordResetToken.cs      # Reset token
├── Enums/
│   ├── NotificationType.cs        # Follower, Reply
│   └── ImageProcessingStatus.cs   # Pending, Processing, Complete, Failed
├── Common/
│   ├── BaseEntity.cs              # Common properties (Id, CreatedAt)
│   └── IAggregateRoot.cs          # Marker interface
└── Domain.csproj
```

**Example Entity**:
```csharp
namespace TwitterCloneApi.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? Bio { get; set; }
    
    public Guid? ProfileImageId { get; set; }
    public Image? ProfileImage { get; set; }
    
    public Guid? BackgroundImageId { get; set; }
    public Image? BackgroundImage { get; set; }
    
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    
    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
    // Computed properties (not stored in DB)
    public int FollowerCount => Followers.Count;
    public int FollowingCount => Following.Count;
    public int PostCount => Posts.Count(p => p.ParentPostId == null);
}
```

### 2. Application Layer

**Purpose**: Contains application business rules and orchestrates the flow of data.

**Characteristics**:
- Depends only on Domain layer
- Defines interfaces that Infrastructure implements
- Uses CQRS pattern (Commands and Queries)
- Contains DTOs for data transfer
- Validation logic with FluentValidation

**Contains**:
```
src/Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs      # EF Core DbContext interface
│   │   ├── IEmailService.cs              # Email sending
│   │   ├── IImageService.cs              # Image processing
│   │   ├── IJwtService.cs                # JWT token generation
│   │   ├── IPasswordHasher.cs            # Password hashing
│   │   ├── ICurrentUserService.cs        # Get current authenticated user
│   │   └── IFileStorage.cs               # File storage abstraction
│   ├── Models/
│   │   ├── Result.cs                     # Operation result wrapper
│   │   ├── PaginatedList.cs              # Pagination helper
│   │   └── ImageUrls.cs                  # Image variant URLs
│   └── Exceptions/
│       ├── ValidationException.cs
│       ├── NotFoundException.cs
│       ├── UnauthorizedException.cs
│       └── ConflictException.cs
├── Features/                             # CQRS style organization
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── Register/
│   │   │   │   ├── RegisterCommand.cs
│   │   │   │   ├── RegisterCommandValidator.cs
│   │   │   │   └── RegisterCommandHandler.cs
│   │   │   ├── Login/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── LoginCommandValidator.cs
│   │   │   │   └── LoginCommandHandler.cs
│   │   │   ├── VerifyEmail/
│   │   │   ├── ResendVerification/
│   │   │   ├── ForgotPassword/
│   │   │   ├── ResetPassword/
│   │   │   └── RefreshToken/
│   │   └── Queries/
│   │       └── CheckUsername/
│   ├── Users/
│   │   ├── Commands/
│   │   │   ├── UpdateProfile/
│   │   │   ├── ChangePassword/
│   │   │   └── DeleteAccount/
│   │   └── Queries/
│   │       ├── GetUserProfile/
│   │       ├── GetUserPosts/
│   │       ├── GetFollowers/
│   │       └── GetFollowing/
│   ├── Posts/
│   ├── Images/
│   ├── Feed/
│   ├── Search/
│   └── Notifications/
├── DTOs/
│   ├── Auth/
│   │   ├── LoginResponse.cs
│   │   └── TokenResponse.cs
│   ├── Users/
│   │   └── UserDto.cs
│   ├── Posts/
│   │   └── PostDto.cs
│   └── Images/
│       └── ImageDto.cs
├── Mappings/
│   └── MappingProfile.cs                 # AutoMapper configuration
└── Application.csproj
```

**Example Command**:
```csharp
namespace TwitterCloneApi.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result<AuthResponse>>
{
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(1, 100)
            .Matches("^[a-zA-Z0-9]+$");
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
            
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
            
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .Length(1, 100);
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    
    // Implementation...
}
```

### 3. Infrastructure Layer

**Purpose**: Implements interfaces defined in Application layer.

**Characteristics**:
- Depends on Application and Domain
- Contains all external concerns
- Database access (Entity Framework Core)
- Email sending (MailKit)
- File storage (local filesystem)
- Background jobs (Hangfire)
- External API integrations

**Contains**:
```
src/Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs           # EF Core DbContext
│   ├── Configurations/                   # Fluent API configurations
│   │   ├── UserConfiguration.cs
│   │   ├── PostConfiguration.cs
│   │   ├── ImageConfiguration.cs
│   │   └── ...
│   └── Migrations/                       # EF Core migrations
│       └── (auto-generated)
├── Services/
│   ├── JwtService.cs                     # JWT token generation/validation
│   ├── EmailService.cs                   # Email sending with MailKit
│   ├── ImageService.cs                   # Image processing with ImageSharp
│   ├── PasswordHasher.cs                 # BCrypt password hashing
│   └── CurrentUserService.cs             # Get user from HttpContext
├── Storage/
│   ├── LocalFileStorage.cs               # Local filesystem implementation
│   └── (Future: AzureBlobStorage.cs)
├── BackgroundJobs/
│   └── ImageProcessingJob.cs             # Hangfire job for image processing
├── DependencyInjection.cs                # Service registration
└── Infrastructure.csproj
```

**Example Configuration**:
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(u => u.Username)
            .IsUnique();
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.HasOne(u => u.ProfileImage)
            .WithMany()
            .HasForeignKey(u => u.ProfileImageId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Ignore computed properties
        builder.Ignore(u => u.FollowerCount);
        builder.Ignore(u => u.FollowingCount);
        builder.Ignore(u => u.PostCount);
    }
}
```

### 4. API Layer (Presentation)

**Purpose**: Handles HTTP requests and responses.

**Characteristics**:
- Depends on Application and Infrastructure
- Controllers are thin (delegates to Application layer)
- Handles authentication/authorization
- Request/response transformation
- Global exception handling
- Middleware pipeline

**Contains**:
```
src/API/
├── Controllers/
│   ├── AuthController.cs                 # Authentication endpoints
│   ├── UsersController.cs                # User management
│   ├── PostsController.cs                # Post CRUD
│   ├── ImagesController.cs               # Image upload
│   ├── FeedController.cs                 # Feed generation
│   ├── SearchController.cs               # Search endpoints
│   ├── NotificationsController.cs        # Notifications
│   └── BookmarksController.cs            # Bookmarks
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs    # Global error handling
│   └── JwtMiddleware.cs                  # JWT validation (optional)
├── Filters/
│   └── ValidateModelAttribute.cs         # Model validation filter
├── Extensions/
│   └── ServiceCollectionExtensions.cs    # DI registration
├── Program.cs                            # Application startup
├── appsettings.json
├── appsettings.Development.json
└── API.csproj
```

**Example Controller**:
```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        return CreatedAtAction(nameof(GetUser), new { username = result.Value.User.Username }, result.Value);
    }
}
```

## Design Patterns Used

### 1. CQRS (Command Query Responsibility Segregation)

Separates read and write operations for better scalability and clarity.

**Commands**: Modify state (Create, Update, Delete)
**Queries**: Read state (Get, List, Search)

```csharp
// Command
public record CreatePostCommand : IRequest<Result<PostDto>>
{
    public string Content { get; init; }
    public List<Guid> ImageIds { get; init; }
}

// Query
public record GetUserProfileQuery : IRequest<Result<UserDto>>
{
    public string Username { get; init; }
}
```

### 2. Repository Pattern

Abstracts data access behind interfaces (using EF Core DbContext as repository).

```csharp
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Post> Posts { get; }
    DbSet<Image> Images { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 3. Unit of Work

EF Core DbContext acts as Unit of Work, coordinating changes across multiple repositories.

```csharp
// All changes saved atomically
await _context.Users.AddAsync(user);
await _context.EmailVerificationTokens.AddAsync(token);
await _context.SaveChangesAsync(); // Single transaction
```

### 4. Dependency Injection

All dependencies injected via constructor, enabling testability and loose coupling.

```csharp
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    
    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }
}
```

### 5. Strategy Pattern

Different implementations of same interface (e.g., storage providers).

```csharp
public interface IFileStorage
{
    Task<string> SaveAsync(Stream stream, string path);
    Task<Stream> GetAsync(string path);
    Task DeleteAsync(string path);
}

// Local implementation
public class LocalFileStorage : IFileStorage { }

// Future: Cloud implementation
public class AzureBlobStorage : IFileStorage { }
```

## Data Flow

### Request Flow (Example: Create Post)

```
1. HTTP Request
   POST /api/posts
   {
     "content": "Hello world!",
     "imageIds": ["img-123"]
   }
   ↓

2. Controller
   PostsController.CreatePost()
   - Validates request
   - Maps to CreatePostCommand
   ↓

3. Mediator
   IMediator.Send(CreatePostCommand)
   - Routes to handler
   ↓

4. Command Handler
   CreatePostCommandHandler
   - Validates business rules
   - Retrieves user from ICurrentUserService
   - Checks image ownership
   - Creates Post entity
   - Extracts hashtags
   - Saves to database via IApplicationDbContext
   ↓

5. Database
   ApplicationDbContext (EF Core)
   - Begins transaction
   - Inserts Post
   - Inserts PostImage records
   - Inserts/links Hashtag records
   - Commits transaction
   ↓

6. Response Mapping
   Handler maps Post → PostDto
   ↓

7. HTTP Response
   201 Created
   {
     "id": "post-456",
     "content": "Hello world!",
     "user": { ... },
     "images": [ ... ],
     "createdAt": "2025-11-10T12:00:00Z"
   }
```

## Dependency Graph

```
API Layer
  ├── Depends on: Application, Infrastructure
  └── References: Application, Infrastructure

Infrastructure Layer
  ├── Depends on: Application (interfaces), Domain
  └── References: Application, Domain

Application Layer
  ├── Depends on: Domain
  └── References: Domain

Domain Layer
  └── Depends on: Nothing (pure C#)
```

## Cross-Cutting Concerns

### Logging
- Structured logging with Serilog
- Log levels: Debug, Information, Warning, Error, Critical
- Log sinks: Console, File, Seq (optional)

### Error Handling
- Global exception middleware
- Custom exceptions (ValidationException, NotFoundException, etc.)
- Consistent error response format

### Validation
- FluentValidation for command/query validation
- Automatic validation in pipeline (MediatR behavior)
- Model validation in controllers

### Authentication
- JWT bearer tokens
- Automatic authentication middleware
- [Authorize] attribute on controllers

### Rate Limiting
- AspNetCoreRateLimit middleware
- Per-user limits
- Different limits per endpoint category

## Scalability Considerations

### Current Design (MVP)
- Single database connection
- Synchronous feed generation
- In-memory background jobs (Hangfire)

### Future Improvements
- **Database**: Read replicas for queries
- **Caching**: Redis for feed, user profiles, post counts
- **Background Jobs**: Distributed Hangfire with Redis/SQL Server storage
- **File Storage**: CDN with edge caching
- **Search**: Elasticsearch for full-text search
- **Messaging**: Message queue for notifications (RabbitMQ/Azure Service Bus)

## Security Architecture

### Defense in Depth
1. **HTTPS**: All communication encrypted
2. **Authentication**: JWT tokens with short expiration
3. **Authorization**: Role/claim-based access control
4. **Input Validation**: FluentValidation + model validation
5. **SQL Injection**: EF Core parameterized queries
6. **XSS**: Content sanitization
7. **CSRF**: Not needed (stateless API)
8. **Rate Limiting**: Prevent abuse
9. **Password Hashing**: BCrypt with salt
10. **CORS**: Restricted origins

## Testing Strategy

### Unit Tests
- Test Application layer handlers in isolation
- Mock all dependencies (IApplicationDbContext, IEmailService, etc.)
- Fast execution (no database, no I/O)

### Integration Tests
- Test API endpoints end-to-end
- Use Testcontainers for real PostgreSQL
- WebApplicationFactory for in-memory API

### Architecture Tests
- Enforce dependency rules
- Ensure no cyclic dependencies
- Validate naming conventions

See **11-testing-strategy.md** for details.

## Migration Path

### From Local to Cloud

**Current State**:
- PostgreSQL in Docker
- Local filesystem storage
- In-memory Hangfire

**Cloud Migration** (Azure example):
1. Database: Azure Database for PostgreSQL
2. File Storage: Azure Blob Storage
3. Background Jobs: Hangfire with SQL Server storage
4. Email: Azure Communication Services
5. Monitoring: Application Insights
6. Hosting: Azure App Service / Kubernetes

**Code Changes Required**: Minimal (swap implementations)
- Change connection string
- Register AzureBlobStorage instead of LocalFileStorage
- Configure Azure services in DI

## Summary

Clean Architecture provides:
- ✅ **Testability**: Business logic isolated and mockable
- ✅ **Maintainability**: Clear separation of concerns
- ✅ **Flexibility**: Swap implementations without changing business logic
- ✅ **Scalability**: Independent scaling of layers
- ✅ **Team Collaboration**: Clear boundaries for parallel work

This architecture may seem complex for an MVP, but it teaches industry best practices and makes the codebase professional and maintainable.
