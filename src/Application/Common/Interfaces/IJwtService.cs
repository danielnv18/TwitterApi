using TwitterCloneApi.Domain.Entities;

namespace TwitterCloneApi.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
