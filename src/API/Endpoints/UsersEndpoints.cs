using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitterCloneApi.Application.Features.Users.Commands.ChangePassword;
using TwitterCloneApi.Application.Features.Users.Commands.DeleteAccount;
using TwitterCloneApi.Application.Features.Users.Commands.UpdateProfile;
using TwitterCloneApi.Application.Features.Users.Common;
using TwitterCloneApi.Application.Features.Users.Queries.GetUserProfile;

namespace TwitterCloneApi.API.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/{username}", async (string username, IMediator mediator) =>
        {
            var query = new GetUserProfileQuery(username);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetUserProfile")
        .WithSummary("Get user profile by username")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/me", async ([FromBody] UpdateProfileCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("UpdateProfile")
        .WithSummary("Update current user's profile")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/me/password", async ([FromBody] ChangePasswordCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("ChangePassword")
        .WithSummary("Change current user's password")
        .Produces<ChangePasswordResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/me", async ([FromBody] DeleteAccountCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("DeleteAccount")
        .WithSummary("Delete current user's account")
        .Produces<DeleteAccountResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
