using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TwitterCloneApi.Application.Features.Auth.Commands.RefreshToken;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;

namespace TwitterCloneApi.IntegrationTests.Features.Auth;

public class RefreshTokenTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public RefreshTokenTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TwitterCloneApi.Infrastructure.Data.ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNewTokens_WhenTokensAreValid()
    {
        // Arrange - Register a user to get initial tokens
        var registerCommand = new RegisterCommand("refreshuser", "refresh@example.com", "Password123");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshCommand = new RefreshTokenCommand(authResponse!.AccessToken, authResponse.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        var newAuthResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        newAuthResponse.Should().NotBeNull();
        newAuthResponse!.AccessToken.Should().NotBeNullOrEmpty();
        newAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();

        // Refresh token should always be different (old one is revoked, new one generated)
        newAuthResponse.RefreshToken.Should().NotBe(authResponse.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenIsInvalid()
    {
        // Arrange - Register a user to get a valid access token
        var registerCommand = new RegisterCommand("refreshuser2", "refresh2@example.com", "Password123");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshCommand = new RefreshTokenCommand(authResponse!.AccessToken, "invalid-refresh-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshCommand);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
