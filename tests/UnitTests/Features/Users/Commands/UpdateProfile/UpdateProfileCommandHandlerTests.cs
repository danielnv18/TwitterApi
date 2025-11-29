using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TwitterCloneApi.Application.Common.Exceptions;
using TwitterCloneApi.Application.Common.Interfaces;
using TwitterCloneApi.Application.Features.Users.Commands.UpdateProfile;
using TwitterCloneApi.Application.Features.Users.Common;
using TwitterCloneApi.Domain.Entities;
using TwitterCloneApi.Infrastructure.Data;

namespace TwitterCloneApi.UnitTests.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _currentUserServiceMock = new Mock<ICurrentUserService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Old Name",
            Bio = "Old bio",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new UpdateProfileCommandHandler(_context, _currentUserServiceMock.Object, _mapper);
        var command = new UpdateProfileCommand
        {
            DisplayName = "New Name",
            Bio = "New bio"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be("New Name");
        result.Bio.Should().Be("New bio");

        var updatedUser = await _context.Users.FindAsync(userId);
        updatedUser!.DisplayName.Should().Be("New Name");
        updatedUser.Bio.Should().Be("New bio");
    }

    [Fact]
    public async Task Handle_NotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = new UpdateProfileCommandHandler(_context, _currentUserServiceMock.Object, _mapper);
        var command = new UpdateProfileCommand { DisplayName = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

        var handler = new UpdateProfileCommandHandler(_context, _currentUserServiceMock.Object, _mapper);
        var command = new UpdateProfileCommand { DisplayName = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
