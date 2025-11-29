namespace TwitterCloneApi.Application.Features.Auth.Common;

public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresIn);
