using MediatR;

namespace TwitterCloneApi.Application.Features.Users.Commands.DeleteAccount;

public class DeleteAccountCommand : IRequest<DeleteAccountResponse>
{
    public string Password { get; set; } = string.Empty;
}

public class DeleteAccountResponse
{
    public string Message { get; set; } = "Account deleted successfully";
}
