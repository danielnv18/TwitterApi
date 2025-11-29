using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitterCloneApi.Application.Common.Exceptions;
using TwitterCloneApi.Application.Common.Interfaces;
using TwitterCloneApi.Application.Features.Users.Common;

namespace TwitterCloneApi.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User with username '{request.Username}' not found");
        }

        return _mapper.Map<UserDto>(user);
    }
}
