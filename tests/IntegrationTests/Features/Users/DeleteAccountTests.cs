using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TwitterCloneApi.Application.Features.Auth.Commands.Login;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;
using TwitterCloneApi.Application.Features.Users.Commands.DeleteAccount;

namespace TwitterCloneApi.IntegrationTests.Features.Users;

public class DeleteAccountTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DeleteAccountTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeleteAccount_ValidRequest_DeletesAccountSuccessfully()
    {
        // Arrange - Register and login
        var username = $"testuser_{Guid.NewGuid():N}";
        var email = $"test_{Guid.NewGuid():N}@example.com";
        var password = "Test1234";
        var registerCommand = new RegisterCommand(username, email, password);

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        // Act - Delete account
        var deleteCommand = new DeleteAccountCommand
        {
            Password = password
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/users/me")
        {
            Content = JsonContent.Create(deleteCommand)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify cannot login with deleted account
        _client.DefaultRequestHeaders.Authorization = null;
        var loginAgainResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        loginAgainResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
