using MediatR;
using TwitterCloneApi.Application.Features.Users.Common;

namespace TwitterCloneApi.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommand : IRequest<UserDto>
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
}
