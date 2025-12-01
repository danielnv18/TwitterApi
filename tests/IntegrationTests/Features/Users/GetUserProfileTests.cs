using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Users.Common;

namespace TwitterCloneApi.IntegrationTests.Features.Users;

public class GetUserProfileTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public GetUserProfileTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TwitterCloneApi.Infrastructure.Data.ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetUserProfile_ValidUsername_ReturnsUserProfile()
    {
        // Arrange - Register a user
        var username = $"user{Guid.NewGuid():N}"[..15]; // Max 20 chars, using 15 to be safe
        var email = $"test_{Guid.NewGuid():N}@example.com";
        var registerCommand = new RegisterCommand(username, email, "Test1234");

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        registerResponse.EnsureSuccessStatusCode(); // Ensure registration succeeded

        // Act - Get user profile
        var response = await _client.GetAsync($"/api/users/{username}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.Username.Should().Be(username);
        userDto.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserProfile_InvalidUsername_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/users/nonexistentuser");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
