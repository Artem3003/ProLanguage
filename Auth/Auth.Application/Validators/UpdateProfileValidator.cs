using Auth.Application.DTOs;
using FluentValidation;

namespace Auth.Application.Validators;

/// <summary>
/// Validator for the UpdateProfileDto.
/// </summary>
public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProfileValidator"/> class.
    /// </summary>
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Invalid phone number format. Please use a valid international format, e.g., +1234567890.")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
