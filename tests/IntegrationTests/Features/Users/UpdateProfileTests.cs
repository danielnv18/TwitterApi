using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TwitterCloneApi.Application.Features.Auth.Commands.Login;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;
using TwitterCloneApi.Application.Features.Users.Commands.UpdateProfile;
using TwitterCloneApi.Application.Features.Users.Common;

namespace TwitterCloneApi.IntegrationTests.Features.Users;

public class UpdateProfileTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UpdateProfileTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TwitterCloneApi.Infrastructure.Data.ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task UpdateProfile_ValidRequest_UpdatesProfile()
    {
        // Arrange - Register and login
        var username = $"user{Guid.NewGuid():N}"[..15];
        var email = $"test_{Guid.NewGuid():N}@example.com";
        var password = "Test1234";
        var registerCommand = new RegisterCommand(username, email, password);

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        // Act - Update profile
        var updateCommand = new UpdateProfileCommand
        {
            DisplayName = "New Name",
            Bio = "This is my new bio"
        };

        var response = await _client.PatchAsJsonAsync("/api/users/me", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.DisplayName.Should().Be("New Name");
        userDto.Bio.Should().Be("This is my new bio");
    }

    [Fact]
    public async Task UpdateProfile_Unauthorized_ReturnsUnauthorized()
    {
        // Act - Try to update without authentication
        var updateCommand = new UpdateProfileCommand
        {
            DisplayName = "New Name"
        };

        var response = await _client.PatchAsJsonAsync("/api/users/me", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
