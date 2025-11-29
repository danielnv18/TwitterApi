using MediatR;

namespace TwitterCloneApi.Application.Features.Users.Queries.CheckUsernameAvailable;

public record CheckUsernameAvailableQuery(string Username) : IRequest<UsernameAvailabilityDto>;

public class UsernameAvailabilityDto
{
    public string Username { get; set; } = string.Empty;
    public bool Available { get; set; }
}
