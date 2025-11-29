using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitterCloneApi.Application.Common.Exceptions;
using TwitterCloneApi.Application.Common.Interfaces;
using TwitterCloneApi.Application.Features.Auth.Common;

namespace TwitterCloneApi.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            throw new UnauthorizedException("Invalid token claims.");
        }

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedException("User not found.");
        }

        var existingRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);

        if (existingRefreshToken == null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (!existingRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Refresh token is expired or revoked.");
        }

        // Revoke current refresh token
        existingRefreshToken.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshTokenString = _jwtService.GenerateRefreshToken();

        var newRefreshToken = new TwitterCloneApi.Domain.Entities.RefreshToken
        {
            Token = newRefreshTokenString,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshToken.Token, 3600);
    }
}
