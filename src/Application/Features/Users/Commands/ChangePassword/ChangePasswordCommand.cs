using MediatR;

namespace TwitterCloneApi.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<ChangePasswordResponse>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordResponse
{
    public string Message { get; set; } = "Password changed successfully";
}
