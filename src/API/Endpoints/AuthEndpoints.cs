using MediatR;
using TwitterCloneApi.Application.Features.Auth.Commands.Login;
using TwitterCloneApi.Application.Features.Auth.Commands.RefreshToken;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;
using TwitterCloneApi.Application.Features.Users.Queries.CheckUsernameAvailable;

namespace TwitterCloneApi.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (RegisterCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("Register")
        .WithSummary("Register a new user")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Login user")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", async (RefreshTokenCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh access token")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/check-username", async (string username, IMediator mediator) =>
        {
            var query = new CheckUsernameAvailableQuery(username);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("CheckUsernameAvailability")
        .WithSummary("Check if a username is available")
        .Produces<UsernameAvailabilityDto>(StatusCodes.Status200OK);

        return app;
    }
}
