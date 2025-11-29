using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TwitterCloneApi.Application.Features.Auth.Commands.Login;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;

namespace TwitterCloneApi.IntegrationTests.Features.Auth;

public class LoginTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public LoginTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TwitterCloneApi.Infrastructure.Data.ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Login_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        // Arrange - First register a user
        var registerCommand = new RegisterCommand("loginuser", "login@example.com", "Password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand("login@example.com", "Password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange - First register a user
        var registerCommand = new RegisterCommand("loginuser2", "login2@example.com", "Password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand("login2@example.com", "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var loginCommand = new LoginCommand("nonexistent@example.com", "Password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
