using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;

namespace TwitterCloneApi.IntegrationTests.Features.Auth;

public class RegisterTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public RegisterTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TwitterCloneApi.Infrastructure.Data.ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Register_ShouldReturnAuthResponse_WhenRequestIsValid()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@example.com", "Password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }
}
