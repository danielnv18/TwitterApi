using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitterCloneApi.Application.Common.Interfaces;

namespace TwitterCloneApi.Application.Features.Users.Queries.CheckUsernameAvailable;

public class CheckUsernameAvailableQueryHandler : IRequestHandler<CheckUsernameAvailableQuery, UsernameAvailabilityDto>
{
    private readonly IApplicationDbContext _context;

    public CheckUsernameAvailableQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UsernameAvailabilityDto> Handle(CheckUsernameAvailableQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        return new UsernameAvailabilityDto
        {
            Username = request.Username,
            Available = !exists
        };
    }
}
