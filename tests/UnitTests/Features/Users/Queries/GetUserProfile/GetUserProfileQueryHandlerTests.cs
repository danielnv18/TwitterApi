using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TwitterCloneApi.Application.Common.Exceptions;
using TwitterCloneApi.Application.Features.Users.Common;
using TwitterCloneApi.Application.Features.Users.Queries.GetUserProfile;
using TwitterCloneApi.Domain.Entities;
using TwitterCloneApi.Infrastructure.Data;

namespace TwitterCloneApi.UnitTests.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ValidUsername_ReturnsUserDto()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Bio = "Test bio",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var handler = new GetUserProfileQueryHandler(_context, _mapper);
        var query = new GetUserProfileQuery("testuser");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("Test User");
        result.Bio.Should().Be("Test bio");
    }

    [Fact]
    public async Task Handle_InvalidUsername_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new GetUserProfileQueryHandler(_context, _mapper);
        var query = new GetUserProfileQuery("nonexistent");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }
}
