using MediatR;
using TwitterCloneApi.Application.Features.Users.Common;

namespace TwitterCloneApi.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(string Username) : IRequest<UserDto>;
