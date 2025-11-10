# Project Overview

## Vision

Build a Twitter-like social media platform as a learning project to master .NET 9, Clean Architecture, and modern backend development practices. This MVP will implement core Twitter features including user authentication, posts with images, social interactions (follow, like, bookmark), and a chronological feed.

## Learning Objectives

### Primary Goals
- **Master .NET 9**: Learn modern C# features, minimal APIs, and ASP.NET Core
- **Clean Architecture**: Implement proper separation of concerns and dependency management
- **Entity Framework Core**: Database-first design with migrations and relationships
- **Authentication & Security**: JWT tokens, password hashing, rate limiting
- **Background Processing**: Asynchronous image processing with Hangfire
- **RESTful API Design**: Industry-standard API patterns and best practices
- **Testing**: Unit and integration testing with xUnit
- **Docker**: Container orchestration for local development

### Secondary Goals
- Database-agnostic design (PostgreSQL → SQL Server migration path)
- Cloud-ready architecture (local filesystem → cloud storage migration path)
- Email integration (verification, password reset)
- Image processing and optimization
- Performance optimization patterns

## Project Scope

### ✅ In Scope (MVP)

**Authentication & Users**
- User registration with email verification
- Login with JWT access tokens + refresh tokens
- Password reset flow
- User profiles (display name, bio, profile image, background image)
- Username availability check

**Posts & Content**
- Create posts (1-280 characters)
- Attach up to 4 images per post
- Reply to posts (nested threading)
- Delete own posts
- View post details with parent/replies

**Social Features**
- Follow/unfollow users
- Like/unlike posts
- Bookmark posts (private)
- Notifications (new follower, replies)
- Hashtag extraction and search
- User search (username, display name)
- Post search (content, hashtags)

**Feed**
- Personalized feed (posts from followed users)
- Public feed (all posts, chronological)
- User timeline (specific user's posts)

**Images**
- Upload images (max 10MB, JPEG/PNG/GIF/WebP)
- Automatic processing to WebP format
- Generate 4 variants (thumbnail, small, medium, large)
- EXIF data stripping for privacy
- Background processing with Hangfire

### ❌ Out of Scope (Future Phases)

**Not in MVP**
- Direct messaging
- Retweet/share functionality
- Real-time updates (WebSockets/SignalR)
- Video support
- User blocking/muting
- Trending topics algorithm
- Recommended users
- Advanced analytics
- Email notifications (transactional emails only)
- Two-factor authentication
- Mobile push notifications
- Content moderation tools
- Ads system
- Verified badges

**Future Enhancements**
- Cloud deployment (Azure/AWS)
- Elasticsearch for advanced search
- Redis caching layer
- CDN integration
- Metrics and monitoring (Application Insights)
- CI/CD pipeline
- API rate limiting per endpoint
- GraphQL API
- Mobile native apps

## Target Audience

This project is designed for:
- **Software developers** learning .NET backend development
- **Students** studying modern web architecture
- **Portfolio projects** demonstrating full-stack skills
- **Framework comparison** (implement same spec in different languages)
- **Interview preparation** with a real-world project

## Success Criteria

### Technical Excellence
- ✅ Clean Architecture principles followed
- ✅ SOLID principles applied
- ✅ 80%+ code coverage with tests
- ✅ All endpoints documented with Swagger
- ✅ Docker Compose setup works out-of-the-box
- ✅ Database migrations apply cleanly
- ✅ No hardcoded credentials
- ✅ Proper error handling and logging

### Functionality
- ✅ Users can register, verify email, and login
- ✅ Users can create posts with images
- ✅ Users can follow others and see a personalized feed
- ✅ Users can interact (like, bookmark, reply)
- ✅ Images are processed asynchronously
- ✅ Search works for users, posts, and hashtags
- ✅ Notifications are generated correctly

### Performance
- ✅ API responses < 200ms (without images)
- ✅ Image processing completes within 30 seconds
- ✅ Feed generation < 500ms for 20 posts
- ✅ Database queries optimized (no N+1 problems)

## Technology Decisions

### Why .NET 9?
- Modern, performant, cross-platform framework
- Excellent tooling (Visual Studio, Rider, VS Code)
- Strong typing and compile-time safety
- Large ecosystem of libraries
- Industry demand for .NET developers
- First-class cloud support (Azure)

### Why PostgreSQL?
- Open-source and free
- Powerful features (JSONB, full-text search)
- Excellent Entity Framework Core support
- Can migrate to SQL Server later
- Good Docker support
- Industry standard

### Why Clean Architecture?
- Testable code (business logic isolated)
- Database/framework independence
- Clear separation of concerns
- Easy to understand for teams
- Scalable for large projects
- Industry best practice

### Why JWT + Refresh Tokens?
- Stateless authentication (horizontal scaling)
- Industry standard for REST APIs
- Works with any frontend (React, Vue, Angular, mobile)
- Secure when implemented correctly
- Teaches real-world authentication patterns

## Project Timeline

### Estimated Duration: 8 weeks (part-time)

**Week 1-2: Foundation**
- Project structure setup
- Docker Compose configuration
- Database design and migrations
- Domain entities
- Basic authentication (register, login)

**Week 3-4: Core Features**
- User profiles with images
- Post creation with images
- Image processing pipeline
- Follow/unfollow system
- Like system

**Week 5-6: Social Features**
- Reply system
- Hashtag extraction and linking
- Feed generation
- Bookmarks
- Notifications

**Week 7: Email & Security**
- Email verification flow
- Password reset flow
- Refresh token rotation
- Rate limiting
- Security hardening

**Week 8: Search & Polish**
- User search
- Post search
- Hashtag search
- Swagger documentation
- Unit tests
- Integration tests
- README and deployment guide

## Documentation Structure

This documentation is organized into focused guides:

1. **01-project-overview.md** (this file) - Vision, scope, goals
2. **02-system-architecture.md** - Clean Architecture, layers, dependencies
3. **03-technology-stack.md** - .NET 9, libraries, tools, justifications
4. **04-database-design.md** - Complete schema, relationships, indexes
5. **05-domain-models.md** - Entities, value objects, domain rules
6. **06-api-specification.md** - All endpoints, request/response contracts
7. **07-authentication-security.md** - JWT, refresh tokens, rate limiting
8. **08-image-handling.md** - Upload, processing, storage strategy
9. **09-email-system.md** - Verification, password reset, Mailpit setup
10. **10-development-setup.md** - Docker, running locally, IDE configuration
11. **11-testing-strategy.md** - xUnit, integration tests, coverage goals
12. **12-coding-standards.md** - C# conventions, naming, patterns
13. **13-deployment-roadmap.md** - Future cloud migration, CI/CD

## Getting Started

New to this project? Follow these steps:

1. **Read this overview** to understand the vision and scope
2. **Review the system architecture** (doc 02) to understand the layers
3. **Check the technology stack** (doc 03) to see what you'll be using
4. **Study the database design** (doc 04) to understand data relationships
5. **Follow the development setup** (doc 10) to get running locally
6. **Read the API specification** (doc 06) to understand the endpoints
7. **Start coding!** Begin with Phase 1 features

## Contributing

This is a learning project, but best practices still apply:

- Write clean, readable code
- Follow the coding standards (doc 12)
- Write tests for new features
- Update documentation when changing behavior
- Use meaningful commit messages
- Keep PRs focused and small

## Questions?

Refer to the specific documentation files for detailed information. Each doc covers a specific aspect of the system in depth.

**Happy coding! 🚀**
