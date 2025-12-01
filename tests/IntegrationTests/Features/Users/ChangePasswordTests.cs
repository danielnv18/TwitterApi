using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TwitterCloneApi.Application.Features.Auth.Commands.Login;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;
using TwitterCloneApi.Application.Features.Users.Commands.ChangePassword;

namespace TwitterCloneApi.IntegrationTests.Features.Users;

public class ChangePasswordTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ChangePasswordTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TwitterCloneApi.Infrastructure.Data.ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task ChangePassword_ValidRequest_ChangesPasswordSuccessfully()
    {
        // Arrange - Register and login
        var username = $"user{Guid.NewGuid():N}"[..15];
        var email = $"test_{Guid.NewGuid():N}@example.com";
        var password = "OldPassword123";
        var registerCommand = new RegisterCommand(username, email, password);

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        // Act - Change password
        var changePasswordCommand = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword123"
        };

        var response = await _client.PatchAsJsonAsync("/api/users/me/password", changePasswordCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify can login with new password
        _client.DefaultRequestHeaders.Authorization = null;
        var newLoginCommand = new LoginCommand(email, "NewPassword123");

        var newLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", newLoginCommand);
        newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword_ReturnsUnauthorized()
    {
        // Arrange - Register and login
        var username = $"user{Guid.NewGuid():N}"[..15];
        var email = $"test_{Guid.NewGuid():N}@example.com";
        var password = "CorrectPassword123";
        var registerCommand = new RegisterCommand(username, email, password);

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        // Act - Try to change password with wrong current password
        var changePasswordCommand = new ChangePasswordCommand
        {
            CurrentPassword = "WrongPassword123",
            NewPassword = "NewPassword123"
        };

        var response = await _client.PatchAsJsonAsync("/api/users/me/password", changePasswordCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
