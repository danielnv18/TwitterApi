using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TwitterCloneApi.Application.Features.Auth.Commands.Login;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;
using TwitterCloneApi.Application.Features.Users.Common;

namespace TwitterCloneApi.IntegrationTests.Features.Users;

public class GetUserProfileTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetUserProfileTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUserProfile_ValidUsername_ReturnsUserProfile()
    {
        // Arrange - Register a user
        var username = $"testuser_{Guid.NewGuid():N}";
        var email = $"test_{Guid.NewGuid():N}@example.com";
        var registerCommand = new RegisterCommand(username, email, "Test1234");

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

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
