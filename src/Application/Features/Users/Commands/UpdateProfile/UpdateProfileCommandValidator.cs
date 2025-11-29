using FluentValidation;

namespace TwitterCloneApi.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        When(x => x.DisplayName != null, () =>
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("Display name cannot be empty")
                .MinimumLength(1)
                .MaximumLength(50)
                .WithMessage("Display name must be between 1 and 50 characters");
        });

        When(x => x.Bio != null, () =>
        {
            RuleFor(x => x.Bio)
                .MaximumLength(500)
                .WithMessage("Bio cannot exceed 500 characters");
        });
    }
}
