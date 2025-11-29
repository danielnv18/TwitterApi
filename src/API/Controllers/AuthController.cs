using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitterCloneApi.Application.Features.Auth.Commands.Login;
using TwitterCloneApi.Application.Features.Auth.Commands.RefreshToken;
using TwitterCloneApi.Application.Features.Auth.Commands.Register;
using TwitterCloneApi.Application.Features.Auth.Common;
using TwitterCloneApi.Application.Features.Users.Queries.CheckUsernameAvailable;

namespace TwitterCloneApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Check if a username is available
    /// </summary>
    [HttpGet("check-username")]
    public async Task<ActionResult<UsernameAvailabilityDto>> CheckUsername([FromQuery] string username)
    {
        var query = new CheckUsernameAvailableQuery(username);
        return Ok(await _mediator.Send(query));
    }
}
