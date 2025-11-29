using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitterCloneApi.Application.Features.Users.Commands.ChangePassword;
using TwitterCloneApi.Application.Features.Users.Commands.DeleteAccount;
using TwitterCloneApi.Application.Features.Users.Commands.UpdateProfile;
using TwitterCloneApi.Application.Features.Users.Common;
using TwitterCloneApi.Application.Features.Users.Queries.GetUserProfile;

namespace TwitterCloneApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user profile by username
    /// </summary>
    [HttpGet("{username}")]
    public async Task<ActionResult<UserDto>> GetUserProfile(string username)
    {
        var query = new GetUserProfileQuery(username);
        return Ok(await _mediator.Send(query));
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [Authorize]
    [HttpPatch("me")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Change current user's password
    /// </summary>
    [Authorize]
    [HttpPatch("me/password")]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePassword(ChangePasswordCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Delete current user's account
    /// </summary>
    [Authorize]
    [HttpDelete("me")]
    public async Task<ActionResult<DeleteAccountResponse>> DeleteAccount(DeleteAccountCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}
