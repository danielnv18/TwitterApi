using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TwitterCloneApi.Application.Common.Exceptions;
using TwitterCloneApi.Application.Common.Interfaces;
using TwitterCloneApi.Application.Features.Users.Commands.ChangePassword;
using TwitterCloneApi.Domain.Entities;
using TwitterCloneApi.Infrastructure.Data;

namespace TwitterCloneApi.UnitTests.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public ChangePasswordCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
    }

    [Fact]
    public async Task Handle_ValidRequest_ChangesPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test",
            PasswordHash = "oldhash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _passwordHasherMock.Setup(x => x.VerifyPassword("oldpassword", "oldhash")).Returns(true);
        _passwordHasherMock.Setup(x => x.HashPassword("newpassword")).Returns("newhash");

        var handler = new ChangePasswordCommandHandler(_context, _currentUserServiceMock.Object, _passwordHasherMock.Object);
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Password changed successfully");

        var updatedUser = await _context.Users.FindAsync(userId);
        updatedUser!.PasswordHash.Should().Be("newhash");
    }

    [Fact]
    public async Task Handle_IncorrectCurrentPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test",
            PasswordHash = "oldhash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _passwordHasherMock.Setup(x => x.VerifyPassword("wrongpassword", "oldhash")).Returns(false);

        var handler = new ChangePasswordCommandHandler(_context, _currentUserServiceMock.Object, _passwordHasherMock.Object);
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "newpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
