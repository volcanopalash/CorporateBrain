using FluentValidation;

namespace CorporateBrain.Application.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        // Rule 1: First Name is required and max 50 chars
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        // Rule 2: Last Name is required
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        // Rule 3: Email must be valid
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
